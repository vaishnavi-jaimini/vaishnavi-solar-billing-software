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

        await ApplyLinesAsync(quotation, dto.Items);

        _db.Quotations.Add(quotation);
        await _db.SaveChangesAsync();
        return quotation;
    }

    public async Task<Quotation> UpdateQuotationAsync(int id, QuotationUpsertDto dto)
    {
        var quotation = await _db.Quotations.Include(q => q.Items).FirstOrDefaultAsync(q => q.Id == id)
            ?? throw new KeyNotFoundException("Quotation not found");
        if (quotation.Status == QuotationStatus.Converted)
            throw new InvalidOperationException("Cannot edit a quotation already converted to an invoice.");
        _ = await _db.Customers.FindAsync(dto.CustomerId)
            ?? throw new InvalidOperationException("Customer not found");

        quotation.Date = dto.Date;
        quotation.ValidUntil = dto.ValidUntil;
        quotation.CustomerId = dto.CustomerId;
        quotation.Notes = dto.Notes;

        // Lines are replaced wholesale rather than diffed; the quotation number and status stay the same.
        _db.QuotationItems.RemoveRange(quotation.Items);
        quotation.Items.Clear();
        await ApplyLinesAsync(quotation, dto.Items);

        await _db.SaveChangesAsync();
        return quotation;
    }

    private async Task ApplyLinesAsync(Quotation quotation, IEnumerable<QuotationItemInputDto> lines)
    {
        decimal subtotal = 0, totalGst = 0;
        foreach (var line in lines)
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
    }
}
