using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.DTOs;

public class ItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ItemCategory Category { get; set; }
    public string HsnSacCode { get; set; } = string.Empty;
    public ItemUnit Unit { get; set; }
    public decimal Rate { get; set; }
    public decimal GstPercent { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal LowStockThreshold { get; set; }
    public bool IsService { get; set; }
    public bool IsActive { get; set; }
}

public class ItemUpsertDto
{
    public string Name { get; set; } = string.Empty;
    public ItemCategory Category { get; set; }
    public string HsnSacCode { get; set; } = string.Empty;
    public ItemUnit Unit { get; set; }
    public decimal Rate { get; set; }
    public decimal GstPercent { get; set; }
    public decimal LowStockThreshold { get; set; } = 5;
    public bool IsService { get; set; }
    public bool IsActive { get; set; } = true;
}

public class StockAdjustmentDto
{
    public int ItemId { get; set; }
    public StockTransactionType Type { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}
