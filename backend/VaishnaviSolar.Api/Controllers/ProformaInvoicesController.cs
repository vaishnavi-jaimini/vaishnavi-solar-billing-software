using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.DTOs;
using VaishnaviSolar.Api.Models;
using VaishnaviSolar.Api.Services;

namespace VaishnaviSolar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProformaInvoicesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProformaInvoiceService _proformaService;
    private readonly InvoiceService _invoiceService;

    public ProformaInvoicesController(AppDbContext db, ProformaInvoiceService proformaService, InvoiceService invoiceService)
    {
        _db = db;
        _proformaService = proformaService;
        _invoiceService = invoiceService;
    }

    private static ProformaInvoiceDto ToDto(ProformaInvoice p) => new()
    {
        Id = p.Id,
        ProformaNumber = p.ProformaNumber,
        ProformaDate = p.ProformaDate,
        CustomerId = p.CustomerId,
        CustomerName = p.Customer?.Name ?? string.Empty,
        CustomerAddress = p.Customer?.Address ?? string.Empty,
        CustomerCity = p.Customer?.City ?? string.Empty,
        CustomerState = p.Customer?.State ?? string.Empty,
        CustomerPincode = p.Customer?.Pincode ?? string.Empty,
        CustomerGstin = p.Customer?.Gstin,
        PlaceOfSupply = p.PlaceOfSupply,
        IsInterState = p.IsInterState,
        ShipToName = p.ShipToName,
        ShipToAddress = p.ShipToAddress,
        ShipToCity = p.ShipToCity,
        ShipToState = p.ShipToState,
        ShipToPincode = p.ShipToPincode,
        Subtotal = p.Subtotal,
        CgstAmount = p.CgstAmount,
        SgstAmount = p.SgstAmount,
        IgstAmount = p.IgstAmount,
        TotalGst = p.TotalGst,
        GrandTotal = p.GrandTotal,
        Notes = p.Notes,
        ConvertedInvoiceId = p.ConvertedInvoiceId,
        Items = p.Items.Select(i => new ProformaInvoiceItemDto
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
    public async Task<ActionResult<List<ProformaInvoiceDto>>> GetAll()
    {
        var list = await _db.ProformaInvoices
            .Include(p => p.Customer)
            .Include(p => p.Items).ThenInclude(i => i.Item)
            .OrderByDescending(p => p.ProformaDate)
            .ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProformaInvoiceDto>> GetById(int id)
    {
        var p = await _db.ProformaInvoices
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        return Ok(ToDto(p));
    }

    [HttpPost]
    public async Task<ActionResult<ProformaInvoiceDto>> Create(ProformaInvoiceUpsertDto dto)
    {
        try
        {
            var p = await _proformaService.CreateProformaInvoiceAsync(dto);
            return await GetById(p.Id);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProformaInvoiceDto>> Update(int id, ProformaInvoiceUpsertDto dto)
    {
        try
        {
            await _proformaService.UpdateProformaInvoiceAsync(id, dto);
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

    [HttpPost("{id}/convert-to-invoice")]
    public async Task<IActionResult> ConvertToInvoice(int id)
    {
        try
        {
            var invoice = await _invoiceService.ConvertProformaToInvoiceAsync(id);
            return Ok(new { invoiceId = invoice.Id, invoiceNumber = invoice.InvoiceNumber });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.ProformaInvoices.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        if (p.ConvertedInvoiceId.HasValue)
        {
            return BadRequest("Cannot delete a proforma invoice already converted to an invoice.");
        }
        _db.ProformaInvoices.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
