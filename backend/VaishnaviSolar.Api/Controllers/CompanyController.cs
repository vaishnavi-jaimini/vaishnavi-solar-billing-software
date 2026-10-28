using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VaishnaviSolar.Api.Data;
using VaishnaviSolar.Api.DTOs;

namespace VaishnaviSolar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly AppDbContext _db;
    public CompanyController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<CompanyDto>> Get()
    {
        var c = await _db.Companies.FirstAsync();
        return Ok(new CompanyDto
        {
            Id = c.Id,
            Name = c.Name,
            Address = c.Address,
            City = c.City,
            State = c.State,
            Pincode = c.Pincode,
            Gstin = c.Gstin,
            Phone = c.Phone,
            Email = c.Email,
            BankName = c.BankName,
            BankAccountNo = c.BankAccountNo,
            BankIfsc = c.BankIfsc,
            BankAccountName = c.BankAccountName,
            BankBranch = c.BankBranch,
            InvoicePrefix = c.InvoicePrefix,
            QuotationPrefix = c.QuotationPrefix,
            NextQuotationNumber = c.NextQuotationNumber,
            ProformaPrefix = c.ProformaPrefix,
            NextProformaNumber = c.NextProformaNumber
        });
    }

    [HttpPut]
    public async Task<ActionResult<CompanyDto>> Update(CompanyDto dto)
    {
        var c = await _db.Companies.FirstAsync();
        c.Name = dto.Name;
        c.Address = dto.Address;
        c.City = dto.City;
        c.State = dto.State;
        c.Pincode = dto.Pincode;
        c.Gstin = dto.Gstin;
        c.Phone = dto.Phone;
        c.Email = dto.Email;
        c.BankName = dto.BankName;
        c.BankAccountNo = dto.BankAccountNo;
        c.BankIfsc = dto.BankIfsc;
        c.BankAccountName = dto.BankAccountName;
        c.BankBranch = dto.BankBranch;
        if (!string.IsNullOrWhiteSpace(dto.InvoicePrefix))
        {
            c.InvoicePrefix = dto.InvoicePrefix;
        }
        if (!string.IsNullOrWhiteSpace(dto.QuotationPrefix))
        {
            c.QuotationPrefix = dto.QuotationPrefix;
        }
        if (dto.NextQuotationNumber > 0)
        {
            c.NextQuotationNumber = dto.NextQuotationNumber;
        }
        if (!string.IsNullOrWhiteSpace(dto.ProformaPrefix))
        {
            c.ProformaPrefix = dto.ProformaPrefix;
        }
        if (dto.NextProformaNumber > 0)
        {
            c.NextProformaNumber = dto.NextProformaNumber;
        }
        await _db.SaveChangesAsync();
        return await Get();
    }
}
