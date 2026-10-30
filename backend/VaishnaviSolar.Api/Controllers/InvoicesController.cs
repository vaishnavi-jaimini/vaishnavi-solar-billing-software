using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.DTOs;
using VaishnaviSolar.Api.Models;
using VaishnaviSolar.Api.Services;

namespace VaishnaviSolar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly InvoiceService _invoiceService;

    public InvoicesController(AppDbContext db, InvoiceService invoiceService)
    {
        _db = db;
        _invoiceService = invoiceService;
    }

    private static InvoiceDto ToDto(Invoice inv) => new()
    {
        Id = inv.Id,
        InvoiceNumber = inv.InvoiceNumber,
        InvoiceDate = inv.InvoiceDate,
        CustomerId = inv.CustomerId,
        CustomerName = inv.Customer?.Name ?? string.Empty,
        CustomerAddress = inv.Customer?.Address ?? string.Empty,
        CustomerCity = inv.Customer?.City ?? string.Empty,
        CustomerState = inv.Customer?.State ?? string.Empty,
        CustomerPincode = inv.Customer?.Pincode ?? string.Empty,
        CustomerGstin = inv.Customer?.Gstin,
        QuotationId = inv.QuotationId,
        PlaceOfSupply = inv.PlaceOfSupply,
        IsInterState = inv.IsInterState,
        ShipToName = inv.ShipToName,
        ShipToAddress = inv.ShipToAddress,
        ShipToCity = inv.ShipToCity,
        ShipToState = inv.ShipToState,
        ShipToPincode = inv.ShipToPincode,
        Terms = inv.Terms,
        DueDate = inv.DueDate,
        WorkOrderNo = inv.WorkOrderNo,
        PurchaseOrderNo = inv.PurchaseOrderNo,
        PurchaseDate = inv.PurchaseDate,
        Subtotal = inv.Subtotal,
        CgstAmount = inv.CgstAmount,
        SgstAmount = inv.SgstAmount,
        IgstAmount = inv.IgstAmount,
        TotalGst = inv.TotalGst,
        GrandTotal = inv.GrandTotal,
        AmountPaid = inv.AmountPaid,
        Status = inv.Status,
        Notes = inv.Notes,
        Items = inv.Items.Select(i => new InvoiceItemDto
        {
            Id = i.Id,
            ItemId = i.ItemId,
            ItemName = i.Item?.Name ?? string.Empty,
            Description = i.Description,
            HsnSacCode = i.HsnSacCode,
            Quantity = i.Quantity,
            Unit = i.Unit,
            Rate = i.Rate,
            GstPercent = i.GstPercent,
            CgstAmount = i.CgstAmount,
            SgstAmount = i.SgstAmount,
            IgstAmount = i.IgstAmount,
            Amount = i.Amount
        }).ToList()
    };

    [HttpGet]
    public async Task<ActionResult<List<InvoiceDto>>> GetAll(
        [FromQuery] InvoiceStatus? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = _db.Invoices.Include(i => i.Customer).Include(i => i.Items).ThenInclude(x => x.Item).AsQueryable();
        if (status.HasValue) query = query.Where(i => i.Status == status);
        if (from.HasValue) query = query.Where(i => i.InvoiceDate >= from.Value);
        if (to.HasValue) query = query.Where(i => i.InvoiceDate <= to.Value);
        var list = await query.OrderByDescending(i => i.InvoiceDate).ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetById(int id)
    {
        var inv = await _db.Invoices.Include(i => i.Customer).Include(i => i.Items).ThenInclude(x => x.Item)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (inv == null) return NotFound();
        return Ok(ToDto(inv));
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create(InvoiceUpsertDto dto)
    {
        try
        {
            var inv = await _invoiceService.CreateInvoiceAsync(dto);
            return await GetById(inv.Id);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<InvoiceDto>> Update(int id, InvoiceUpsertDto dto)
    {
        try
        {
            await _invoiceService.UpdateInvoiceAsync(id, dto);
            return await GetById(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _invoiceService.DeleteInvoiceAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("convert-quotation/{quotationId}")]
    public async Task<ActionResult<InvoiceDto>> ConvertQuotation(int quotationId)
    {
        try
        {
            var inv = await _invoiceService.ConvertQuotationToInvoiceAsync(quotationId);
            return await GetById(inv.Id);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
