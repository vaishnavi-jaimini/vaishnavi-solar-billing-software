using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.DTOs;
using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.Services;

// A challan only records goods being sent out. Stock is deducted by the invoice, so it is not touched here,
// otherwise goods that are both delivered and invoiced would be deducted twice.
public class DeliveryChallanService
{
    private readonly AppDbContext _db;

    public DeliveryChallanService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DeliveryChallan> CreateDeliveryChallanAsync(DeliveryChallanUpsertDto dto)
    {
        var company = await _db.Companies.FirstAsync();
        var challan = new DeliveryChallan
        {
            ChallanNumber = InvoiceService.FormatInvoiceNumber(company.DeliveryChallanPrefix, company.NextDeliveryChallanNumber)
        };
        await ApplyDetailsAsync(challan, dto, company);

        _db.DeliveryChallans.Add(challan);
        company.NextDeliveryChallanNumber += 1;

        await _db.SaveChangesAsync();
        return challan;
    }

    public async Task<DeliveryChallan> UpdateDeliveryChallanAsync(int id, DeliveryChallanUpsertDto dto)
    {
        var challan = await _db.DeliveryChallans.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Delivery challan not found");
        var company = await _db.Companies.FirstAsync();

        var number = dto.ChallanNumber?.Trim();
        if (!string.IsNullOrEmpty(number) && number != challan.ChallanNumber)
        {
            if (await _db.DeliveryChallans.AnyAsync(c => c.Id != id && c.ChallanNumber == number))
                throw new InvalidOperationException($"Challan number {number} is already used by another delivery challan.");
            challan.ChallanNumber = number;
        }

        // Lines are replaced wholesale.
        _db.DeliveryChallanItems.RemoveRange(challan.Items);
        challan.Items.Clear();
        await ApplyDetailsAsync(challan, dto, company);

        await _db.SaveChangesAsync();
        return challan;
    }

    private async Task ApplyDetailsAsync(DeliveryChallan challan, DeliveryChallanUpsertDto dto, Company company)
    {
        var customer = await _db.Customers.FindAsync(dto.CustomerId)
            ?? throw new InvalidOperationException("Customer not found");
        bool interState = !string.Equals(customer.State.Trim(), company.State.Trim(), StringComparison.OrdinalIgnoreCase);

        bool useBillingAddress = dto.ShipSameAsBilling || string.IsNullOrWhiteSpace(dto.ShipToName);

        challan.ChallanDate = dto.ChallanDate;
        challan.CustomerId = dto.CustomerId;
        challan.PlaceOfSupply = customer.State;
        challan.IsInterState = interState;
        challan.ShipToName = useBillingAddress ? customer.Name : dto.ShipToName!;
        challan.ShipToAddress = useBillingAddress ? customer.Address : dto.ShipToAddress ?? string.Empty;
        challan.ShipToCity = useBillingAddress ? customer.City : dto.ShipToCity ?? string.Empty;
        challan.ShipToState = useBillingAddress ? customer.State : dto.ShipToState ?? string.Empty;
        challan.ShipToPincode = useBillingAddress ? customer.Pincode : dto.ShipToPincode ?? string.Empty;
        challan.ReferenceNo = dto.ReferenceNo?.Trim() ?? string.Empty;
        challan.TransporterName = dto.TransporterName?.Trim() ?? string.Empty;
        challan.VehicleNumber = dto.VehicleNumber?.Trim() ?? string.Empty;
        challan.LrNumber = dto.LrNumber?.Trim() ?? string.Empty;
        challan.EwayBillNumber = dto.EwayBillNumber?.Trim() ?? string.Empty;
        challan.Notes = dto.Notes;

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

            challan.Items.Add(new DeliveryChallanItem
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

        challan.Subtotal = subtotal;
        challan.CgstAmount = totalCgst;
        challan.SgstAmount = totalSgst;
        challan.IgstAmount = totalIgst;
        challan.TotalGst = totalCgst + totalSgst + totalIgst;
        challan.GrandTotal = subtotal + challan.TotalGst;
    }
}
