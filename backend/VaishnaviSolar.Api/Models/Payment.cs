namespace VaishnaviSolar.Api.Models;

public class Payment
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public PaymentMode Mode { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
}
