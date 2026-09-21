using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.DTOs;

public class QuotationItemDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal GstPercent { get; set; }
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
    public QuotationStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TotalGst { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    public int? ConvertedInvoiceId { get; set; }
    public List<QuotationItemDto> Items { get; set; } = new();
}

public class QuotationItemInputDto
{
    public int ItemId { get; set; }
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal GstPercent { get; set; }
}

public class QuotationUpsertDto
{
    public int CustomerId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public DateTime? ValidUntil { get; set; }
    public string? Notes { get; set; }
    public List<QuotationItemInputDto> Items { get; set; } = new();
}
