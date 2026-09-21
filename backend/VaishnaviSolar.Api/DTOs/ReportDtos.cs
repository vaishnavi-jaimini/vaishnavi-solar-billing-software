namespace VaishnaviSolar.Api.DTOs;

public class SalesReportRow
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TotalGst { get; set; }
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SalesReportDto
{
    public List<SalesReportRow> Rows { get; set; } = new();
    public decimal TotalSales { get; set; }
    public decimal TotalGstCollected { get; set; }
    public int InvoiceCount { get; set; }
}

public class GstSummaryRow
{
    public string HsnSacCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal TaxableValue { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstAmount { get; set; }
    public decimal TotalTax { get; set; }
}

public class GstSummaryDto
{
    public List<GstSummaryRow> Rows { get; set; } = new();
    public decimal TotalTaxableValue { get; set; }
    public decimal TotalCgst { get; set; }
    public decimal TotalSgst { get; set; }
    public decimal TotalIgst { get; set; }
}

public class OutstandingDueRow
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Balance { get; set; }
}

public class StockReportRow
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal LowStockThreshold { get; set; }
    public bool IsLow { get; set; }
}

public class DashboardDto
{
    public decimal SalesThisMonth { get; set; }
    public decimal OutstandingDues { get; set; }
    public int LowStockCount { get; set; }
    public int InvoiceCountThisMonth { get; set; }
    public List<SalesTrendPoint> SalesTrend { get; set; } = new();
}

public class SalesTrendPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
