namespace VaishnaviSolar.Api.Models;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ItemCategory Category { get; set; }
    public string HsnSacCode { get; set; } = string.Empty;
    public ItemUnit Unit { get; set; }
    public decimal Rate { get; set; }
    public decimal GstPercent { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal LowStockThreshold { get; set; } = 5;
    public bool IsService { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
