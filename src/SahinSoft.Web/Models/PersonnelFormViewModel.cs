using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SahinSoft.Web.Models;

public sealed class PersonnelFormViewModel
{
    public string? Id { get; set; }

    [Required]
    [Display(Name = "Adı Soyadı")]
    public string FullName { get; set; } = string.Empty;

    // Restoran personeli (garson/kasiyer) PIN ile giriş yapar, e-posta ZORUNLU DEĞİL (bkz.
    // PersonnelController.Create yorumu) - string? OLMALI, aksi halde ASP.NET Core'un implicit
    // non-nullable required kuralı bu alanı zorunlu kılıp PIN-only personel oluşturmayı
    // engelliyordu (2026-09-04'te Test Garson oluştururken bulunan gerçek bug).
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string? Email { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string? Password { get; set; }

    [Display(Name = "Telefon")]
    public string? PhoneNumber { get; set; }

    [Required]
    [Display(Name = "Kullanıcı Grubu")]
    public string RoleName { get; set; } = string.Empty;

    [Display(Name = "Aktif mi?")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Görevi")]
    public string? JobTitle { get; set; }

    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "İşe Başlama Tarihi")]
    public DateTime? HireDateUtc { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "İşten Çıkış Tarihi")]
    public DateTime? TerminationDateUtc { get; set; }

    [Display(Name = "Maaş")]
    public decimal? Salary { get; set; }

    [Display(Name = "Kesinti")]
    public decimal? Deduction { get; set; }

    [Display(Name = "Kesinti Açıklaması")]
    public string? DeductionNote { get; set; }

    [Display(Name = "IBAN")]
    public string? Iban { get; set; }

    [Display(Name = "Banka Numarası")]
    public string? BankAccountNumber { get; set; }

    [Display(Name = "Prim Oranı %")]
    public decimal? CommissionRate { get; set; }

    [Display(Name = "Personel Kodu")]
    public string? PersonnelCode { get; set; }

    [Display(Name = "Lokasyon")]
    public int? BranchId { get; set; }

    [Display(Name = "Kasa")]
    public int? DefaultFinancialAccountId { get; set; }

    [Display(Name = "Varsayılan Fiyat")]
    public int? DefaultPriceListId { get; set; }

    [Display(Name = "Mola Süresi (dk)")]
    public int? BreakDurationMinutes { get; set; }

    [Display(Name = "Plaka")]
    public string? LicensePlate { get; set; }

    // Restoran POS ekranı girişi - e-posta/şifre yerine bu kısa PIN kullanılır. Var olan bir
    // PIN'i korumak için boş bırakılabilir (Edit'te); değiştirmek için yeniden girilir.
    [StringLength(6, MinimumLength = 1)]
    [Display(Name = "PIN (Restoran Girişi)")]
    public string? Pin { get; set; }

    [Display(Name = "İndirim Alt Limit %")]
    [Range(typeof(decimal), "0", "100", ParseLimitsInInvariantCulture = true)]
    public decimal DiscountLowerLimitPercent { get; set; }

    [Display(Name = "İndirim Üst Limit %")]
    [Range(typeof(decimal), "0", "100", ParseLimitsInInvariantCulture = true)]
    public decimal DiscountUpperLimitPercent { get; set; }

    public List<SelectListItem> Roles { get; set; } = [];
    public List<SelectListItem> JobTitleOptions { get; set; } = [];

    public string? BranchDisplay { get; set; }
    public string? FinancialAccountDisplay { get; set; }
    public string? PriceListDisplay { get; set; }

    // Yetki Listesi (Edip, 2026-09-04, madde 21) - bir personele birden fazla yetki profili
    // atanabilir, seçilenlerin ID'leri burada tutulur.
    [Display(Name = "Yetki Listesi")]
    public List<int> PermissionProfileIds { get; set; } = [];
    public List<RestaurantPermissionProfileOption> AvailablePermissionProfiles { get; set; } = [];
}

public sealed class RestaurantPermissionProfileOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
