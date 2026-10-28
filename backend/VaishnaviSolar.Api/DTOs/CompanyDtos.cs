namespace VaishnaviSolar.Api.DTOs;

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public string Gstin { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string BankAccountNo { get; set; } = string.Empty;
    public string BankIfsc { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
    public string BankBranch { get; set; } = string.Empty;
    public string InvoicePrefix { get; set; } = "INV-";
    public int NextInvoiceNumber { get; set; } = 1;
    public string QuotationPrefix { get; set; } = "VS/SL###/2026-27";
    public int NextQuotationNumber { get; set; } = 1;
    public string ProformaPrefix { get; set; } = "PRO-";
    public int NextProformaNumber { get; set; } = 1;
}
