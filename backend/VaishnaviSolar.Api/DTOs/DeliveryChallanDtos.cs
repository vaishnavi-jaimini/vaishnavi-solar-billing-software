using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.DTOs;

public class DeliveryChallanItemDto
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

public class DeliveryChallanDto
{
    public int Id { get; set; }
    public string ChallanNumber { get; set; } = string.Empty;
    public DateTime ChallanDate { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string CustomerCity { get; set; } = string.Empty;
    public string CustomerState { get; set; } = string.Empty;
    public string CustomerPincode { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerGstin { get; set; }
    public string PlaceOfSupply { get; set; } = string.Empty;
    public bool IsInterState { get; set; }
    public string ShipToName { get; set; } = string.Empty;
    public string ShipToAddress { get; set; } = string.Empty;
    public string ShipToCity { get; set; } = string.Empty;
    public string ShipToState { get; set; } = string.Empty;
    public string ShipToPincode { get; set; } = string.Empty;
    public string ReferenceNo { get; set; } = string.Empty;
    public string TransporterName { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public string LrNumber { get; set; } = string.Empty;
    public string EwayBillNumber { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstAmount { get; set; }
    public decimal TotalGst { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
    public List<DeliveryChallanItemDto> Items { get; set; } = new();
}

public class DeliveryChallanUpsertDto
{
    // Only used when editing; new challans are numbered from the company's challan format.
    public string? ChallanNumber { get; set; }
    public int CustomerId { get; set; }
    public DateTime ChallanDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public bool ShipSameAsBilling { get; set; } = true;
    public string? ShipToName { get; set; }
    public string? ShipToAddress { get; set; }
    public string? ShipToCity { get; set; }
    public string? ShipToState { get; set; }
    public string? ShipToPincode { get; set; }
    public string? ReferenceNo { get; set; }
    public string? TransporterName { get; set; }
    public string? VehicleNumber { get; set; }
    public string? LrNumber { get; set; }
    public string? EwayBillNumber { get; set; }
    public List<InvoiceItemInputDto> Items { get; set; } = new();
}
