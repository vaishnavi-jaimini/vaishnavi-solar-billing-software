using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.DTOs;
using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReportsController(AppDbContext db) => _db = db;

    [HttpGet("sales")]
    public async Task<ActionResult<SalesReportDto>> Sales([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _db.Invoices.Include(i => i.Customer).AsQueryable();
        if (from.HasValue) query = query.Where(i => i.InvoiceDate >= from.Value);
        if (to.HasValue) query = query.Where(i => i.InvoiceDate <= to.Value);
        var invoices = await query.OrderByDescending(i => i.InvoiceDate).ToListAsync();

        var dto = new SalesReportDto
        {
            Rows = invoices.Select(i => new SalesReportRow
            {
                InvoiceId = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                InvoiceDate = i.InvoiceDate,
                CustomerName = i.Customer?.Name ?? string.Empty,
                Subtotal = i.Subtotal,
                TotalGst = i.TotalGst,
                GrandTotal = i.GrandTotal,
                Status = i.Status.ToString()
            }).ToList(),
            TotalSales = invoices.Sum(i => i.GrandTotal),
            TotalGstCollected = invoices.Sum(i => i.TotalGst),
            InvoiceCount = invoices.Count
        };
        return Ok(dto);
    }

    [HttpGet("gst-summary")]
    public async Task<ActionResult<GstSummaryDto>> GstSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _db.InvoiceItems.Include(x => x.Invoice).AsQueryable();
        if (from.HasValue) query = query.Where(x => x.Invoice!.InvoiceDate >= from.Value);
        if (to.HasValue) query = query.Where(x => x.Invoice!.InvoiceDate <= to.Value);
        var lines = await query.ToListAsync();

        var grouped = lines
            .GroupBy(x => new { x.HsnSacCode, x.Description })
            .Select(g => new GstSummaryRow
            {
                HsnSacCode = g.Key.HsnSacCode,
                ItemName = g.Key.Description,
                TaxableValue = g.Sum(x => x.Amount),
                CgstAmount = g.Sum(x => x.CgstAmount),
                SgstAmount = g.Sum(x => x.SgstAmount),
                IgstAmount = g.Sum(x => x.IgstAmount),
                TotalTax = g.Sum(x => x.CgstAmount + x.SgstAmount + x.IgstAmount)
            })
            .OrderBy(r => r.HsnSacCode)
            .ToList();

        return Ok(new GstSummaryDto
        {
            Rows = grouped,
            TotalTaxableValue = grouped.Sum(r => r.TaxableValue),
            TotalCgst = grouped.Sum(r => r.CgstAmount),
            TotalSgst = grouped.Sum(r => r.SgstAmount),
            TotalIgst = grouped.Sum(r => r.IgstAmount)
        });
    }

    [HttpGet("outstanding-dues")]
    public async Task<ActionResult<List<OutstandingDueRow>>> OutstandingDues()
    {
        var invoices = await _db.Invoices
            .Include(i => i.Customer)
            .Where(i => i.Status != InvoiceStatus.Paid)
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync();

        var rows = invoices.Select(i => new OutstandingDueRow
        {
            InvoiceId = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            InvoiceDate = i.InvoiceDate,
            CustomerName = i.Customer?.Name ?? string.Empty,
            CustomerPhone = i.Customer?.Phone ?? string.Empty,
            GrandTotal = i.GrandTotal,
            AmountPaid = i.AmountPaid,
            Balance = i.GrandTotal - i.AmountPaid
        }).ToList();

        return Ok(rows);
    }

    [HttpGet("stock")]
    public async Task<ActionResult<List<StockReportRow>>> Stock([FromQuery] bool lowOnly = false)
    {
        var items = await _db.Items.Where(i => !i.IsService && i.IsActive).OrderBy(i => i.Category).ThenBy(i => i.Name).ToListAsync();
        var rows = items.Select(i => new StockReportRow
        {
            ItemId = i.Id,
            ItemName = i.Name,
            Category = i.Category.ToString(),
            CurrentStock = i.CurrentStock,
            LowStockThreshold = i.LowStockThreshold,
            IsLow = i.CurrentStock <= i.LowStockThreshold
        }).ToList();

        if (lowOnly) rows = rows.Where(r => r.IsLow).ToList();
        return Ok(rows);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var invoicesThisMonth = await _db.Invoices
            .Where(i => i.InvoiceDate >= monthStart)
            .ToListAsync();

        var outstanding = await _db.Invoices
            .Where(i => i.Status != InvoiceStatus.Paid)
            .SumAsync(i => i.GrandTotal - i.AmountPaid);

        var lowStockCount = await _db.Items
            .Where(i => !i.IsService && i.IsActive && i.CurrentStock <= i.LowStockThreshold)
            .CountAsync();

        var trend = new List<SalesTrendPoint>();
        for (int i = 5; i >= 0; i--)
        {
            var monthDate = monthStart.AddMonths(-i);
            var nextMonth = monthDate.AddMonths(1);
            var total = await _db.Invoices
                .Where(inv => inv.InvoiceDate >= monthDate && inv.InvoiceDate < nextMonth)
                .SumAsync(inv => (decimal?)inv.GrandTotal) ?? 0;
            trend.Add(new SalesTrendPoint { Label = monthDate.ToString("MMM yyyy"), Total = total });
        }

        return Ok(new DashboardDto
        {
            SalesThisMonth = invoicesThisMonth.Sum(i => i.GrandTotal),
            OutstandingDues = outstanding,
            LowStockCount = lowStockCount,
            InvoiceCountThisMonth = invoicesThisMonth.Count,
            SalesTrend = trend
        });
    }
}
