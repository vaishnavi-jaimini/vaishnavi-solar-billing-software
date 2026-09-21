using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.DTOs;
using VaishnaviSolar.Api.Models;
using VaishnaviSolar.Api.Services;

namespace VaishnaviSolar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuotationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly QuotationService _quotationService;

    public QuotationsController(AppDbContext db, QuotationService quotationService)
    {
        _db = db;
        _quotationService = quotationService;
    }

    private static QuotationDto ToDto(Quotation q) => new()
    {
        Id = q.Id,
        QuotationNumber = q.QuotationNumber,
        Date = q.Date,
        ValidUntil = q.ValidUntil,
        CustomerId = q.CustomerId,
        CustomerName = q.Customer?.Name ?? string.Empty,
        Status = q.Status,
        Subtotal = q.Subtotal,
        TotalGst = q.TotalGst,
        GrandTotal = q.GrandTotal,
        Notes = q.Notes,
        ConvertedInvoiceId = q.ConvertedInvoiceId,
        Items = q.Items.Select(i => new QuotationItemDto
        {
            Id = i.Id,
            ItemId = i.ItemId,
            ItemName = i.Item?.Name ?? string.Empty,
            Description = i.Description,
            Quantity = i.Quantity,
            Rate = i.Rate,
            GstPercent = i.GstPercent,
            Amount = i.Amount
        }).ToList()
    };

    [HttpGet]
    public async Task<ActionResult<List<QuotationDto>>> GetAll([FromQuery] QuotationStatus? status)
    {
        var query = _db.Quotations.Include(q => q.Customer).Include(q => q.Items).ThenInclude(i => i.Item).AsQueryable();
        if (status.HasValue) query = query.Where(q => q.Status == status);
        var list = await query.OrderByDescending(q => q.Date).ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuotationDto>> GetById(int id)
    {
        var q = await _db.Quotations.Include(x => x.Customer).Include(x => x.Items).ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (q == null) return NotFound();
        return Ok(ToDto(q));
    }

    [HttpPost]
    public async Task<ActionResult<QuotationDto>> Create(QuotationUpsertDto dto)
    {
        try
        {
            var q = await _quotationService.CreateQuotationAsync(dto);
            return await GetById(q.Id);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] QuotationStatus status)
    {
        var q = await _db.Quotations.FindAsync(id);
        if (q == null) return NotFound();
        if (q.Status == QuotationStatus.Converted)
        {
            return BadRequest("Cannot change status of a converted quotation.");
        }
        q.Status = status;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var q = await _db.Quotations.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (q == null) return NotFound();
        if (q.Status == QuotationStatus.Converted)
        {
            return BadRequest("Cannot delete a quotation already converted to an invoice.");
        }
        _db.Quotations.Remove(q);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
