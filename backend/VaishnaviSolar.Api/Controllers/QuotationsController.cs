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

    // Quotations don't store a CGST/SGST/IGST split, so it's derived here with the same rule invoices use:
    // customer state == company state → CGST + SGST (half each), otherwise IGST.
    private static QuotationDto ToDto(Quotation q, string companyState)
    {
        var customerState = q.Customer?.State ?? string.Empty;
        bool interState = !string.Equals(customerState.Trim(), companyState.Trim(), StringComparison.OrdinalIgnoreCase);

        var items = q.Items.Select(i =>
        {
            decimal gst = Math.Round(i.Amount * i.GstPercent / 100, 2);
            decimal cgst = interState ? 0 : Math.Round(gst / 2, 2);
            return new QuotationItemDto
            {
                Id = i.Id,
                ItemId = i.ItemId,
                ItemName = i.Item?.Name ?? string.Empty,
                Description = i.Description,
                HsnSacCode = i.Item?.HsnSacCode ?? string.Empty,
                Unit = i.Item?.Unit ?? default,
                Quantity = i.Quantity,
                Rate = i.Rate,
                GstPercent = i.GstPercent,
                CgstAmount = cgst,
                SgstAmount = interState ? 0 : gst - cgst,
                IgstAmount = interState ? gst : 0,
                Amount = i.Amount
            };
        }).ToList();

        return new QuotationDto
        {
            Id = q.Id,
            QuotationNumber = q.QuotationNumber,
            Date = q.Date,
            ValidUntil = q.ValidUntil,
            CustomerId = q.CustomerId,
            CustomerName = q.Customer?.Name ?? string.Empty,
            CustomerAddress = q.Customer?.Address ?? string.Empty,
            CustomerCity = q.Customer?.City ?? string.Empty,
            CustomerState = customerState,
            CustomerPincode = q.Customer?.Pincode ?? string.Empty,
            CustomerGstin = q.Customer?.Gstin,
            Status = q.Status,
            PlaceOfSupply = customerState,
            IsInterState = interState,
            Subtotal = q.Subtotal,
            CgstAmount = items.Sum(i => i.CgstAmount),
            SgstAmount = items.Sum(i => i.SgstAmount),
            IgstAmount = items.Sum(i => i.IgstAmount),
            TotalGst = q.TotalGst,
            GrandTotal = q.GrandTotal,
            Notes = q.Notes,
            ConvertedInvoiceId = q.ConvertedInvoiceId,
            Items = items
        };
    }

    private async Task<string> GetCompanyStateAsync() =>
        (await _db.Companies.FirstOrDefaultAsync())?.State ?? string.Empty;

    [HttpGet]
    public async Task<ActionResult<List<QuotationDto>>> GetAll([FromQuery] QuotationStatus? status)
    {
        var query = _db.Quotations.Include(q => q.Customer).Include(q => q.Items).ThenInclude(i => i.Item).AsQueryable();
        if (status.HasValue) query = query.Where(q => q.Status == status);
        var list = await query.OrderByDescending(q => q.Date).ToListAsync();
        var companyState = await GetCompanyStateAsync();
        return Ok(list.Select(q => ToDto(q, companyState)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuotationDto>> GetById(int id)
    {
        var q = await _db.Quotations.Include(x => x.Customer).Include(x => x.Items).ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (q == null) return NotFound();
        return Ok(ToDto(q, await GetCompanyStateAsync()));
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
