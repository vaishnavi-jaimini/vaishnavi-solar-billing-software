namespace VaishnaviSolar.Api.Models;

public class ProformaInvoice
{
    public int Id { get; set; }
    public string ProformaNumber { get; set; } = string.Empty;
    public DateTime ProformaDate { get; set; } = DateTime.UtcNow;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
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

    public ICollection<ProformaInvoiceItem> Items { get; set; } = new List<ProformaInvoiceItem>();
}

public class ProformaInvoiceItem
{
    public int Id { get; set; }
    public int ProformaInvoiceId { get; set; }
    public ProformaInvoice? ProformaInvoice { get; set; }
    public int? ItemId { get; set; }
    public Item? Item { get; set; }
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
