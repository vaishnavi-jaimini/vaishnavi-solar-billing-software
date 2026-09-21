namespace VaishnaviSolar.Api.Models;

public class StockTransaction
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public Item? Item { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public StockTransactionType Type { get; set; }
    public decimal Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }
}
