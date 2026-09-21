namespace VaishnaviSolar.Api.Models;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = "Vaishnavi Solar";
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
    public int NextInvoiceNumber { get; set; } = 1;
    public string InvoicePrefix { get; set; } = "INV-";
    public int NextProformaNumber { get; set; } = 1;
    public string ProformaPrefix { get; set; } = "PRO-";
}
