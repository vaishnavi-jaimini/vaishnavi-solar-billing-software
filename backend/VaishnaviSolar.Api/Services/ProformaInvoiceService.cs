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
        var proforma = new ProformaInvoice
        {
            ProformaNumber = InvoiceService.FormatInvoiceNumber(company.ProformaPrefix, company.NextProformaNumber)
        };
        await ApplyDetailsAsync(proforma, dto, company);

        _db.ProformaInvoices.Add(proforma);
        company.NextProformaNumber += 1;

        await _db.SaveChangesAsync();
        return proforma;
    }

    public async Task<ProformaInvoice> UpdateProformaInvoiceAsync(int id, ProformaInvoiceUpsertDto dto)
    {
        var proforma = await _db.ProformaInvoices.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Proforma invoice not found");
        if (proforma.ConvertedInvoiceId.HasValue)
            throw new InvalidOperationException("Cannot edit a proforma invoice already converted to an invoice.");
        var company = await _db.Companies.FirstAsync();

        var number = dto.ProformaNumber?.Trim();
        if (!string.IsNullOrEmpty(number) && number != proforma.ProformaNumber)
        {
            if (await _db.ProformaInvoices.AnyAsync(p => p.Id != id && p.ProformaNumber == number))
                throw new InvalidOperationException($"Proforma invoice number {number} is already used by another proforma invoice.");
            proforma.ProformaNumber = number;
        }

        // Lines are replaced wholesale.
        _db.ProformaInvoiceItems.RemoveRange(proforma.Items);
        proforma.Items.Clear();
        await ApplyDetailsAsync(proforma, dto, company);

        await _db.SaveChangesAsync();
        return proforma;
    }

    private async Task ApplyDetailsAsync(ProformaInvoice proforma, ProformaInvoiceUpsertDto dto, Company company)
    {
        var customer = await _db.Customers.FindAsync(dto.CustomerId)
            ?? throw new InvalidOperationException("Customer not found");
        bool interState = !string.Equals(customer.State.Trim(), company.State.Trim(), StringComparison.OrdinalIgnoreCase);

        bool useBillingAddress = dto.ShipSameAsBilling || string.IsNullOrWhiteSpace(dto.ShipToName);

        proforma.ProformaDate = dto.ProformaDate;
        proforma.CustomerId = dto.CustomerId;
        proforma.PlaceOfSupply = customer.State;
        proforma.IsInterState = interState;
        proforma.ShipToName = useBillingAddress ? customer.Name : dto.ShipToName!;
        proforma.ShipToAddress = useBillingAddress ? customer.Address : dto.ShipToAddress ?? string.Empty;
        proforma.ShipToCity = useBillingAddress ? customer.City : dto.ShipToCity ?? string.Empty;
        proforma.ShipToState = useBillingAddress ? customer.State : dto.ShipToState ?? string.Empty;
        proforma.ShipToPincode = useBillingAddress ? customer.Pincode : dto.ShipToPincode ?? string.Empty;
        proforma.Notes = dto.Notes;

        decimal subtotal = 0, totalCgst = 0, totalSgst = 0, totalIgst = 0;

        foreach (var line in dto.Items)
        {
            var (item, description, hsn, unit) = await LineDetails.ResolveAsync(_db, line.ItemId, line.Description, line.HsnSacCode, line.Unit);

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
                ItemId = item?.Id,
                Description = description,
                HsnSacCode = hsn,
                Quantity = line.Quantity,
                Unit = unit,
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
    }
}
