using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.DTOs;

public class QuotationItemDto
{
    public int Id { get; set; }
    public int? ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HsnSacCode { get; set; } = string.Empty;
    public ItemUnit Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal GstPercent { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstAmount { get; set; }
    public decimal Amount { get; set; }
}

public class QuotationDto
{
    public int Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime? ValidUntil { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string CustomerCity { get; set; } = string.Empty;
    public string CustomerState { get; set; } = string.Empty;
    public string CustomerPincode { get; set; } = string.Empty;
    public string? CustomerGstin { get; set; }
    public QuotationStatus Status { get; set; }
    public string PlaceOfSupply { get; set; } = string.Empty;
    public bool IsInterState { get; set; }
    public decimal Subtotal { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstAmount { get; set; }
    public decimal TotalGst { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    public string TermsAndConditions { get; set; } = string.Empty;
    public int? ConvertedInvoiceId { get; set; }
    public List<QuotationItemDto> Items { get; set; } = new();
}

public class QuotationItemInputDto
{
    public int? ItemId { get; set; }
    public string? Description { get; set; }
    public string? HsnSacCode { get; set; }
    public ItemUnit? Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal GstPercent { get; set; }
}

public class QuotationUpsertDto
{
    // Only used when editing; new quotations are numbered from the company's quotation format.
    public string? QuotationNumber { get; set; }
    public int CustomerId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public DateTime? ValidUntil { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
    public List<QuotationItemInputDto> Items { get; set; } = new();
}
