namespace SahinSoft.Web.Models;

public sealed class PdfCompanyInfoViewModel
{
    public string CompanyName { get; set; } = "ŞahinSoft";
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? BankName { get; set; }
    public string? Iban { get; set; }
}
