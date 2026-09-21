using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.DTOs;
using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.Services;

public class QuotationService
{
    private readonly AppDbContext _db;

    public QuotationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Quotation> CreateQuotationAsync(QuotationUpsertDto dto)
    {
        _ = await _db.Customers.FindAsync(dto.CustomerId)
            ?? throw new InvalidOperationException("Customer not found");

        int count = await _db.Quotations.CountAsync();
        var quotation = new Quotation
        {
            QuotationNumber = $"QTN-{count + 1:D4}",
            Date = dto.Date,
            ValidUntil = dto.ValidUntil,
            CustomerId = dto.CustomerId,
            Notes = dto.Notes,
            Status = QuotationStatus.Draft
        };

        decimal subtotal = 0, totalGst = 0;
        foreach (var line in dto.Items)
        {
            var item = await _db.Items.FindAsync(line.ItemId)
                ?? throw new InvalidOperationException($"Item {line.ItemId} not found");
            decimal amount = line.Quantity * line.Rate;
            decimal gst = Math.Round(amount * line.GstPercent / 100, 2);

            quotation.Items.Add(new QuotationItem
            {
                ItemId = item.Id,
                Description = string.IsNullOrWhiteSpace(line.Description) ? item.Name : line.Description,
                Quantity = line.Quantity,
                Rate = line.Rate,
                GstPercent = line.GstPercent,
                Amount = amount
            });

            subtotal += amount;
            totalGst += gst;
        }

        quotation.Subtotal = subtotal;
        quotation.TotalGst = totalGst;
        quotation.GrandTotal = subtotal + totalGst;

        _db.Quotations.Add(quotation);
        await _db.SaveChangesAsync();
        return quotation;
    }
}
