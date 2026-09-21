using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMode Mode { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
}

public class PaymentUpsertDto
{
    public int InvoiceId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public PaymentMode Mode { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
}
