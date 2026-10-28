namespace VaishnaviSolar.Api.Models;

public class Quotation
{
    public int Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public DateTime? ValidUntil { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public QuotationStatus Status { get; set; } = QuotationStatus.Draft;
    public decimal Subtotal { get; set; }
    public decimal TotalGst { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    // Numbered terms printed on the quotation, one per line. Empty means the standard list is printed.
    public string TermsAndConditions { get; set; } = string.Empty;
    public int? ConvertedInvoiceId { get; set; }

    public ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
}

public class QuotationItem
{
    public int Id { get; set; }
    public int QuotationId { get; set; }
    public Quotation? Quotation { get; set; }
    public int ItemId { get; set; }
    public Item? Item { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal GstPercent { get; set; }
    public decimal Amount { get; set; }
}
