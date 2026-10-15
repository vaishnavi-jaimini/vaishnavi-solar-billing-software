using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.DTOs;
using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.Services;

public class ProformaInvoiceService
{
    private readonly AppDbContext _db;

    public ProformaInvoiceService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProformaInvoice> CreateProformaInvoiceAsync(ProformaInvoiceUpsertDto dto)
    {
        var company = await _db.Companies.FirstAsync();
        var customer = await _db.Customers.FindAsync(dto.CustomerId)
            ?? throw new InvalidOperationException("Customer not found");
        bool interState = !string.Equals(customer.State.Trim(), company.State.Trim(), StringComparison.OrdinalIgnoreCase);

        bool useBillingAddress = dto.ShipSameAsBilling || string.IsNullOrWhiteSpace(dto.ShipToName);

        var proforma = new ProformaInvoice
        {
            ProformaNumber = $"{company.ProformaPrefix}{company.NextProformaNumber:D4}",
            ProformaDate = dto.ProformaDate,
            CustomerId = dto.CustomerId,
            PlaceOfSupply = customer.State,
            IsInterState = interState,
            ShipToName = useBillingAddress ? customer.Name : dto.ShipToName!,
            ShipToAddress = useBillingAddress ? customer.Address : dto.ShipToAddress ?? string.Empty,
            ShipToCity = useBillingAddress ? customer.City : dto.ShipToCity ?? string.Empty,
            ShipToState = useBillingAddress ? customer.State : dto.ShipToState ?? string.Empty,
            ShipToPincode = useBillingAddress ? customer.Pincode : dto.ShipToPincode ?? string.Empty,
            Notes = dto.Notes
        };

        decimal subtotal = 0, totalCgst = 0, totalSgst = 0, totalIgst = 0;

        foreach (var line in dto.Items)
        {
            var item = await _db.Items.FindAsync(line.ItemId)
                ?? throw new InvalidOperationException($"Item {line.ItemId} not found");

            decimal amount = line.Quantity * line.Rate;
            decimal gstAmount = Math.Round(amount * line.GstPercent / 100, 2);
            decimal cgst = 0, sgst = 0, igst = 0;
            if (interState)
            {
                igst = gstAmount;
            }
            else
            {
                cgst = Math.Round(gstAmount / 2, 2);
                sgst = gstAmount - cgst;
            }

            proforma.Items.Add(new ProformaInvoiceItem
            {
                ItemId = item.Id,
                Description = string.IsNullOrWhiteSpace(line.Description) ? item.Name : line.Description,
                HsnSacCode = item.HsnSacCode,
                Quantity = line.Quantity,
                Unit = item.Unit,
                Rate = line.Rate,
                GstPercent = line.GstPercent,
                CgstAmount = cgst,
                SgstAmount = sgst,
                IgstAmount = igst,
                Amount = amount
            });

            subtotal += amount;
            totalCgst += cgst;
            totalSgst += sgst;
            totalIgst += igst;
        }

        proforma.Subtotal = subtotal;
        proforma.CgstAmount = totalCgst;
        proforma.SgstAmount = totalSgst;
        proforma.IgstAmount = totalIgst;
        proforma.TotalGst = totalCgst + totalSgst + totalIgst;
        proforma.GrandTotal = subtotal + proforma.TotalGst;

        _db.ProformaInvoices.Add(proforma);
        company.NextProformaNumber += 1;

        await _db.SaveChangesAsync();
        return proforma;
    }
}
