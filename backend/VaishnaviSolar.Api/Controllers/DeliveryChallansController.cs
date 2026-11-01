using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.DTOs;
using VaishnaviSolar.Api.Models;
using VaishnaviSolar.Api.Services;

namespace VaishnaviSolar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveryChallansController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly DeliveryChallanService _challanService;

    public DeliveryChallansController(AppDbContext db, DeliveryChallanService challanService)
    {
        _db = db;
        _challanService = challanService;
    }

    private static DeliveryChallanDto ToDto(DeliveryChallan c) => new()
    {
        Id = c.Id,
        ChallanNumber = c.ChallanNumber,
        ChallanDate = c.ChallanDate,
        CustomerId = c.CustomerId,
        CustomerName = c.Customer?.Name ?? string.Empty,
        CustomerAddress = c.Customer?.Address ?? string.Empty,
        CustomerCity = c.Customer?.City ?? string.Empty,
        CustomerState = c.Customer?.State ?? string.Empty,
        CustomerPincode = c.Customer?.Pincode ?? string.Empty,
        CustomerPhone = c.Customer?.Phone ?? string.Empty,
        CustomerGstin = c.Customer?.Gstin,
        PlaceOfSupply = c.PlaceOfSupply,
        IsInterState = c.IsInterState,
        ShipToName = c.ShipToName,
        ShipToAddress = c.ShipToAddress,
        ShipToCity = c.ShipToCity,
        ShipToState = c.ShipToState,
        ShipToPincode = c.ShipToPincode,
        ReferenceNo = c.ReferenceNo,
        TransporterName = c.TransporterName,
        VehicleNumber = c.VehicleNumber,
        LrNumber = c.LrNumber,
        EwayBillNumber = c.EwayBillNumber,
        Subtotal = c.Subtotal,
        CgstAmount = c.CgstAmount,
        SgstAmount = c.SgstAmount,
        IgstAmount = c.IgstAmount,
        TotalGst = c.TotalGst,
        GrandTotal = c.GrandTotal,
        Notes = c.Notes,
        Items = c.Items.Select(i => new DeliveryChallanItemDto
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
    public async Task<ActionResult<List<DeliveryChallanDto>>> GetAll()
    {
        var list = await _db.DeliveryChallans
            .Include(c => c.Customer)
            .Include(c => c.Items).ThenInclude(i => i.Item)
            .OrderByDescending(c => c.ChallanDate)
            .ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DeliveryChallanDto>> GetById(int id)
    {
        var c = await _db.DeliveryChallans
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        return Ok(ToDto(c));
    }

    [HttpPost]
    public async Task<ActionResult<DeliveryChallanDto>> Create(DeliveryChallanUpsertDto dto)
    {
        try
        {
            var c = await _challanService.CreateDeliveryChallanAsync(dto);
            return await GetById(c.Id);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DeliveryChallanDto>> Update(int id, DeliveryChallanUpsertDto dto)
    {
        try
        {
            await _challanService.UpdateDeliveryChallanAsync(id, dto);
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
        var c = await _db.DeliveryChallans.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        _db.DeliveryChallans.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
