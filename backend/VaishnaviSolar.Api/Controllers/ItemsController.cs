using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.DTOs;
using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ItemsController(AppDbContext db) => _db = db;

    private static ItemDto ToDto(Item i) => new()
    {
        Id = i.Id,
        Name = i.Name,
        Category = i.Category,
        HsnSacCode = i.HsnSacCode,
        Unit = i.Unit,
        Rate = i.Rate,
        GstPercent = i.GstPercent,
        CurrentStock = i.CurrentStock,
        LowStockThreshold = i.LowStockThreshold,
        IsService = i.IsService,
        IsActive = i.IsActive
    };

    [HttpGet]
    public async Task<ActionResult<List<ItemDto>>> GetAll([FromQuery] string? search, [FromQuery] bool activeOnly = false)
    {
        var query = _db.Items.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i => i.Name.Contains(search) || i.HsnSacCode.Contains(search));
        }
        if (activeOnly)
        {
            query = query.Where(i => i.IsActive);
        }
        var list = await query.OrderBy(i => i.Category).ThenBy(i => i.Name).ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetById(int id)
    {
        var i = await _db.Items.FindAsync(id);
        if (i == null) return NotFound();
        return Ok(ToDto(i));
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create(ItemUpsertDto dto)
    {
        var i = new Item
        {
            Name = dto.Name,
            Category = dto.Category,
            HsnSacCode = dto.HsnSacCode,
            Unit = dto.Unit,
            Rate = dto.Rate,
            GstPercent = dto.GstPercent,
            LowStockThreshold = dto.LowStockThreshold,
            IsService = dto.IsService,
            IsActive = dto.IsActive,
            CurrentStock = 0
        };
        _db.Items.Add(i);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = i.Id }, ToDto(i));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ItemUpsertDto dto)
    {
        var i = await _db.Items.FindAsync(id);
        if (i == null) return NotFound();
        i.Name = dto.Name;
        i.Category = dto.Category;
        i.HsnSacCode = dto.HsnSacCode;
        i.Unit = dto.Unit;
        i.Rate = dto.Rate;
        i.GstPercent = dto.GstPercent;
        i.LowStockThreshold = dto.LowStockThreshold;
        i.IsService = dto.IsService;
        i.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return Ok(ToDto(i));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var i = await _db.Items.FindAsync(id);
        if (i == null) return NotFound();
        bool used = await _db.InvoiceItems.AnyAsync(x => x.ItemId == id) || await _db.QuotationItems.AnyAsync(x => x.ItemId == id);
        if (used)
        {
            i.IsActive = false;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Item is used in invoices/quotations, marked inactive instead of deleted." });
        }
        _db.Items.Remove(i);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
