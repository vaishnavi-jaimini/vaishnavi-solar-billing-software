using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.DTOs;
using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.Services;

public class InvoiceService
{
    private readonly AppDbContext _db;

    public InvoiceService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(decimal cgst, decimal sgst, decimal igst)> SplitGstAsync(decimal gstAmount, int customerId)
    {
        var company = await _db.Companies.FirstAsync();
        var customer = await _db.Customers.FindAsync(customerId);
        bool interState = customer != null &&
            !string.Equals(customer.State.Trim(), company.State.Trim(), StringComparison.OrdinalIgnoreCase);

        if (interState)
        {
            return (0, 0, gstAmount);
        }
        var half = Math.Round(gstAmount / 2, 2);
        return (half, gstAmount - half, 0);
    }

    public async Task<bool> IsInterStateAsync(int customerId)
    {
        var company = await _db.Companies.FirstAsync();
        var customer = await _db.Customers.FindAsync(customerId);
        return customer != null &&
            !string.Equals(customer.State.Trim(), company.State.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public async Task<Invoice> CreateInvoiceAsync(InvoiceUpsertDto dto)
    {
        var company = await _db.Companies.FirstAsync();
        var customer = await _db.Customers.FindAsync(dto.CustomerId)
            ?? throw new InvalidOperationException("Customer not found");
        bool interState = !string.Equals(customer.State.Trim(), company.State.Trim(), StringComparison.OrdinalIgnoreCase);

        bool useBillingAddress = dto.ShipSameAsBilling || string.IsNullOrWhiteSpace(dto.ShipToName);

        var invoice = new Invoice
        {
            InvoiceNumber = FormatInvoiceNumber(company.InvoicePrefix, company.NextInvoiceNumber),
            InvoiceDate = dto.InvoiceDate,
            CustomerId = dto.CustomerId,
            PlaceOfSupply = string.IsNullOrWhiteSpace(dto.PlaceOfSupply) ? customer.State : dto.PlaceOfSupply.Trim(),
            IsInterState = interState,
            ShipToName = useBillingAddress ? customer.Name : dto.ShipToName!,
            ShipToAddress = useBillingAddress ? customer.Address : dto.ShipToAddress ?? string.Empty,
            ShipToCity = useBillingAddress ? customer.City : dto.ShipToCity ?? string.Empty,
            ShipToState = useBillingAddress ? customer.State : dto.ShipToState ?? string.Empty,
            ShipToPincode = useBillingAddress ? customer.Pincode : dto.ShipToPincode ?? string.Empty,
            Terms = string.IsNullOrWhiteSpace(dto.Terms) ? "Due on Receipt" : dto.Terms.Trim(),
            DueDate = string.IsNullOrWhiteSpace(dto.DueDate) ? "As per Agreement" : dto.DueDate.Trim(),
            WorkOrderNo = dto.WorkOrderNo?.Trim() ?? string.Empty,
            PurchaseOrderNo = dto.PurchaseOrderNo?.Trim() ?? string.Empty,
            // Calendar date only; tagged UTC because PostgreSQL rejects unspecified-kind timestamps.
            PurchaseDate = dto.PurchaseDate.HasValue
                ? DateTime.SpecifyKind(dto.PurchaseDate.Value.Date, DateTimeKind.Utc)
                : null,
            Notes = dto.Notes,
            Status = InvoiceStatus.Unpaid
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

            invoice.Items.Add(new InvoiceItem
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

            if (!item.IsService)
            {
                item.CurrentStock -= line.Quantity;
                _db.StockTransactions.Add(new StockTransaction
                {
                    ItemId = item.Id,
                    Date = dto.InvoiceDate,
                    Type = StockTransactionType.Sale,
                    Quantity = -line.Quantity,
                    ReferenceType = "Invoice",
                    Notes = "Auto-deducted on invoice"
                });
            }
        }

        invoice.Subtotal = subtotal;
        invoice.CgstAmount = totalCgst;
        invoice.SgstAmount = totalSgst;
        invoice.IgstAmount = totalIgst;
        invoice.TotalGst = totalCgst + totalSgst + totalIgst;
        invoice.GrandTotal = subtotal + invoice.TotalGst;

        _db.Invoices.Add(invoice);
        company.NextInvoiceNumber += 1;

        await _db.SaveChangesAsync();
        return invoice;
    }

    // A run of '#' in the prefix marks where the number goes, zero-padded to the run's width
    // (e.g. "VS/###/2026-27" -> "VS/003/2026-27"); otherwise the number is appended as 4 digits.
    public static string FormatInvoiceNumber(string prefix, int number)
    {
        var match = System.Text.RegularExpressions.Regex.Match(prefix, "#+");
        if (!match.Success)
        {
            return $"{prefix}{number:D4}";
        }
        var padded = number.ToString().PadLeft(match.Length, '0');
        return prefix.Remove(match.Index, match.Length).Insert(match.Index, padded);
    }

    public async Task<Invoice> ConvertQuotationToInvoiceAsync(int quotationId)
    {
        var quotation = await _db.Quotations
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == quotationId)
            ?? throw new InvalidOperationException("Quotation not found");

        if (quotation.Status == QuotationStatus.Converted)
        {
            throw new InvalidOperationException("Quotation already converted to an invoice");
        }

        var upsert = new InvoiceUpsertDto
        {
            CustomerId = quotation.CustomerId,
            InvoiceDate = DateTime.UtcNow,
            Notes = quotation.Notes,
            Items = quotation.Items.Select(i => new InvoiceItemInputDto
            {
                ItemId = i.ItemId,
                Description = i.Description,
                Quantity = i.Quantity,
                Rate = i.Rate,
                GstPercent = i.GstPercent
            }).ToList()
        };

        var invoice = await CreateInvoiceAsync(upsert);
        invoice.QuotationId = quotation.Id;
        quotation.Status = QuotationStatus.Converted;
        quotation.ConvertedInvoiceId = invoice.Id;
        await _db.SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice> ConvertProformaToInvoiceAsync(int proformaId)
    {
        var proforma = await _db.ProformaInvoices
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == proformaId)
            ?? throw new InvalidOperationException("Proforma invoice not found");

        if (proforma.ConvertedInvoiceId.HasValue)
        {
            throw new InvalidOperationException("Proforma invoice already converted to an invoice");
        }

        var upsert = new InvoiceUpsertDto
        {
            CustomerId = proforma.CustomerId,
            InvoiceDate = DateTime.UtcNow,
            Notes = proforma.Notes,
            ShipSameAsBilling = false,
            ShipToName = proforma.ShipToName,
            ShipToAddress = proforma.ShipToAddress,
            ShipToCity = proforma.ShipToCity,
            ShipToState = proforma.ShipToState,
            ShipToPincode = proforma.ShipToPincode,
            Items = proforma.Items.Select(i => new InvoiceItemInputDto
            {
                ItemId = i.ItemId,
                Description = i.Description,
                Quantity = i.Quantity,
                Rate = i.Rate,
                GstPercent = i.GstPercent
            }).ToList()
        };

        var invoice = await CreateInvoiceAsync(upsert);
        proforma.ConvertedInvoiceId = invoice.Id;
        await _db.SaveChangesAsync();
        return invoice;
    }

    public async Task ApplyPaymentAsync(Invoice invoice)
    {
        var paid = await _db.Payments.Where(p => p.InvoiceId == invoice.Id).SumAsync(p => p.Amount);
        invoice.AmountPaid = paid;
        if (paid <= 0)
        {
            invoice.Status = InvoiceStatus.Unpaid;
        }
        else if (paid >= invoice.GrandTotal)
        {
            invoice.Status = InvoiceStatus.Paid;
        }
        else
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }
        await _db.SaveChangesAsync();
    }
}
