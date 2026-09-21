using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.DTOs;
using VaishnaviSolar.Api.Models;

namespace VaishnaviSolar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;
    public CustomersController(AppDbContext db) => _db = db;

    private static CustomerDto ToDto(Customer c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Address = c.Address,
        City = c.City,
        State = c.State,
        Pincode = c.Pincode,
        Phone = c.Phone,
        Email = c.Email,
        Gstin = c.Gstin
    };

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll([FromQuery] string? search)
    {
        var query = _db.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search) || c.Phone.Contains(search));
        }
        var list = await query.OrderBy(c => c.Name).ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var c = await _db.Customers.FindAsync(id);
        if (c == null) return NotFound();
        return Ok(ToDto(c));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CustomerUpsertDto dto)
    {
        var c = new Customer
        {
            Name = dto.Name,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Pincode = dto.Pincode,
            Phone = dto.Phone,
            Email = dto.Email,
            Gstin = dto.Gstin
        };
        _db.Customers.Add(c);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, ToDto(c));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CustomerUpsertDto dto)
    {
        var c = await _db.Customers.FindAsync(id);
        if (c == null) return NotFound();
        c.Name = dto.Name;
        c.Address = dto.Address;
        c.City = dto.City;
        c.State = dto.State;
        c.Pincode = dto.Pincode;
        c.Phone = dto.Phone;
        c.Email = dto.Email;
        c.Gstin = dto.Gstin;
        await _db.SaveChangesAsync();
        return Ok(ToDto(c));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Customers.FindAsync(id);
        if (c == null) return NotFound();
        bool hasInvoices = await _db.Invoices.AnyAsync(i => i.CustomerId == id);
        if (hasInvoices)
        {
            return BadRequest("Cannot delete a customer with existing invoices.");
        }
        _db.Customers.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
