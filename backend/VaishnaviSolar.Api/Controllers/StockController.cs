using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.DTOs;
using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly AppDbContext _db;
    public StockController(AppDbContext db) => _db = db;

    [HttpGet("transactions/{itemId}")]
    public async Task<IActionResult> GetTransactions(int itemId)
    {
        var list = await _db.StockTransactions
            .Where(t => t.ItemId == itemId)
            .OrderByDescending(t => t.Date)
            .Select(t => new
            {
                t.Id,
                t.Date,
                t.Type,
                t.Quantity,
                t.ReferenceType,
                t.Notes
            })
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust(StockAdjustmentDto dto)
    {
        var item = await _db.Items.FindAsync(dto.ItemId);
        if (item == null) return NotFound("Item not found");
        if (item.IsService) return BadRequest("Services do not track stock.");
        if (dto.Quantity <= 0) return BadRequest("Quantity must be positive.");

        decimal delta = dto.Type switch
        {
            StockTransactionType.Purchase => dto.Quantity,
            StockTransactionType.Opening => dto.Quantity,
            StockTransactionType.Adjustment => dto.Quantity,
            StockTransactionType.Sale => -dto.Quantity,
            _ => dto.Quantity
        };

        item.CurrentStock += delta;
        _db.StockTransactions.Add(new StockTransaction
        {
            ItemId = dto.ItemId,
            Date = DateTime.UtcNow,
            Type = dto.Type,
            Quantity = delta,
            ReferenceType = "Manual",
            Notes = dto.Notes
        });
        await _db.SaveChangesAsync();
        return Ok(new { item.Id, item.CurrentStock });
    }
}
