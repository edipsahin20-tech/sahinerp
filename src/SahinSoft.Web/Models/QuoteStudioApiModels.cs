namespace SahinSoft.Web.Models;

public sealed class QuoteStudioSaveRequest
{
    public int? Id { get; set; }
    public int? CustomerId { get; set; }
    public string Company { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxOffice { get; set; }
    public string? Address { get; set; }
    public DateTime? QuoteDate { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;
    public string? Notes { get; set; }
    public string Status { get; set; } = "Taslak";
    public List<QuoteStudioSaveLine> Items { get; set; } = [];
}

public sealed class QuoteStudioSaveLine
{
    public int? DbId { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = "Adet";
    public decimal Qty { get; set; }
    public decimal Price { get; set; }
    public decimal Kdv { get; set; }
    public decimal Discount { get; set; }
}

public sealed class QuoteStudioViewModel
{
    public string CompanyName { get; set; } = "ŞahinSoft";
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? BankName { get; set; }
    public string? Iban { get; set; }
    public QuoteStudioExistingData? Existing { get; set; }
}

// "Düzenle" ile kaydedilmiş bir taslak teklif Teklif Stüdyosu'na yüklendiğinde formu doldurmak için kullanılır.
public sealed class QuoteStudioExistingData
{
    public int Id { get; set; }
    public string QuoteNumber { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string Company { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxOffice { get; set; }
    public string? Address { get; set; }
    public string QuoteDate { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public string? Notes { get; set; }
    public List<QuoteStudioExistingItem> Items { get; set; } = [];
}

public sealed class QuoteStudioExistingItem
{
    public int? DbId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal Price { get; set; }
    public decimal Kdv { get; set; }
    public decimal Discount { get; set; }
}

public sealed class CustomProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
