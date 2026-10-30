using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.DTOs;

public class ProformaInvoiceItemDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
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

public class ProformaInvoiceDto
{
    public int Id { get; set; }
    public string ProformaNumber { get; set; } = string.Empty;
    public DateTime ProformaDate { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string CustomerCity { get; set; } = string.Empty;
    public string CustomerState { get; set; } = string.Empty;
    public string CustomerPincode { get; set; } = string.Empty;
    public string? CustomerGstin { get; set; }
    public string PlaceOfSupply { get; set; } = string.Empty;
    public bool IsInterState { get; set; }
    public string ShipToName { get; set; } = string.Empty;
    public string ShipToAddress { get; set; } = string.Empty;
    public string ShipToCity { get; set; } = string.Empty;
    public string ShipToState { get; set; } = string.Empty;
    public string ShipToPincode { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstAmount { get; set; }
    public decimal TotalGst { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    public int? ConvertedInvoiceId { get; set; }
    public List<ProformaInvoiceItemDto> Items { get; set; } = new();
}

public class ProformaInvoiceUpsertDto
{
    // Only used when editing; new proforma invoices are numbered from the company's proforma format.
    public string? ProformaNumber { get; set; }
    public int CustomerId { get; set; }
    public DateTime ProformaDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public bool ShipSameAsBilling { get; set; } = true;
    public string? ShipToName { get; set; }
    public string? ShipToAddress { get; set; }
    public string? ShipToCity { get; set; }
    public string? ShipToState { get; set; }
    public string? ShipToPincode { get; set; }
    public List<InvoiceItemInputDto> Items { get; set; } = new();
}
