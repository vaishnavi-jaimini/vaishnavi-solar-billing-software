using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.DTOs;
using VaishnaviSolar.Api.Models;
using VaishnaviSolar.Api.Services;

namespace VaishnaviSolar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly InvoiceService _invoiceService;

    public PaymentsController(AppDbContext db, InvoiceService invoiceService)
    {
        _db = db;
        _invoiceService = invoiceService;
    }

    private static PaymentDto ToDto(Payment p) => new()
    {
        Id = p.Id,
        InvoiceId = p.InvoiceId,
        InvoiceNumber = p.Invoice?.InvoiceNumber ?? string.Empty,
        CustomerName = p.Invoice?.Customer?.Name ?? string.Empty,
        PaymentDate = p.PaymentDate,
        Amount = p.Amount,
        Mode = p.Mode,
        ReferenceNo = p.ReferenceNo,
        Notes = p.Notes
    };

    [HttpGet]
    public async Task<ActionResult<List<PaymentDto>>> GetAll([FromQuery] int? invoiceId)
    {
        var query = _db.Payments.Include(p => p.Invoice).ThenInclude(i => i!.Customer).AsQueryable();
        if (invoiceId.HasValue) query = query.Where(p => p.InvoiceId == invoiceId);
        var list = await query.OrderByDescending(p => p.PaymentDate).ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create(PaymentUpsertDto dto)
    {
        var invoice = await _db.Invoices.FindAsync(dto.InvoiceId);
        if (invoice == null) return NotFound("Invoice not found");
        if (dto.Amount <= 0) return BadRequest("Amount must be positive.");

        var currentPaid = await _db.Payments.Where(p => p.InvoiceId == dto.InvoiceId).SumAsync(p => p.Amount);
        if (currentPaid + dto.Amount > invoice.GrandTotal + 0.01m)
        {
            return BadRequest("Payment exceeds outstanding balance.");
        }

        var payment = new Payment
        {
            InvoiceId = dto.InvoiceId,
            PaymentDate = dto.PaymentDate,
            Amount = dto.Amount,
            Mode = dto.Mode,
            ReferenceNo = dto.ReferenceNo,
            Notes = dto.Notes
        };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        await _invoiceService.ApplyPaymentAsync(invoice);

        var saved = await _db.Payments.Include(p => p.Invoice).ThenInclude(i => i!.Customer)
            .FirstAsync(p => p.Id == payment.Id);
        return Ok(ToDto(saved));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var payment = await _db.Payments.FindAsync(id);
        if (payment == null) return NotFound();
        var invoice = await _db.Invoices.FindAsync(payment.InvoiceId);
        _db.Payments.Remove(payment);
        await _db.SaveChangesAsync();
        if (invoice != null)
        {
            await _invoiceService.ApplyPaymentAsync(invoice);
        }
        return NoContent();
    }
}
