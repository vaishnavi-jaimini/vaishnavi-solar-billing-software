using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.DTOs;

public class InvoiceItemDto
{
    public int Id { get; set; }
    public int? ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string HsnSacCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public ItemUnit Unit { get; set; }
    public decimal Rate { get; set; }
    public decimal GstPercent { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstAmount { get; set; }
    public decimal Amount { get; set; }
}

public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string CustomerCity { get; set; } = string.Empty;
    public string CustomerState { get; set; } = string.Empty;
    public string CustomerPincode { get; set; } = string.Empty;
    public string? CustomerGstin { get; set; }
    public int? QuotationId { get; set; }
    public string PlaceOfSupply { get; set; } = string.Empty;
    public bool IsInterState { get; set; }
    public string ShipToName { get; set; } = string.Empty;
    public string ShipToAddress { get; set; } = string.Empty;
    public string ShipToCity { get; set; } = string.Empty;
    public string ShipToState { get; set; } = string.Empty;
    public string ShipToPincode { get; set; } = string.Empty;
    public string Terms { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public string WorkOrderNo { get; set; } = string.Empty;
    public string PurchaseOrderNo { get; set; } = string.Empty;
    public DateTime? PurchaseDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstAmount { get; set; }
    public decimal TotalGst { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Balance => GrandTotal - AmountPaid;
    public InvoiceStatus Status { get; set; }
    public string? Notes { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
}

public class InvoiceItemInputDto
{
    public int? ItemId { get; set; }
    public string? Description { get; set; }
    public string? HsnSacCode { get; set; }
    public ItemUnit? Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal GstPercent { get; set; }
}

public class InvoiceUpsertDto
{
    public int CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public bool ShipSameAsBilling { get; set; } = true;
    public string? ShipToName { get; set; }
    public string? ShipToAddress { get; set; }
    public string? ShipToCity { get; set; }
    public string? ShipToState { get; set; }
    public string? ShipToPincode { get; set; }
    public string? PlaceOfSupply { get; set; }
    public string? Terms { get; set; }
    public string? DueDate { get; set; }
    public string? WorkOrderNo { get; set; }
    public string? PurchaseOrderNo { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public List<InvoiceItemInputDto> Items { get; set; } = new();
}

public class ConvertQuotationDto
{
    public int QuotationId { get; set; }
}
