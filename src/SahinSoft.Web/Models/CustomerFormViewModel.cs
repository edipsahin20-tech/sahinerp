using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class CustomerFormViewModel
{
    public int Id { get; set; }

    // [Required] kasıtlı olarak yok: boş bırakılırsa controller otomatik kod üretir (StockCode'daki
    // aynı desen) — [Required] olsaydı istemci tarafı doğrulama, sunucuya hiç ulaşmadan boş göndermeyi
    // engellerdi ve "boş bırakılırsa otomatik atanır" vaadi hiçbir zaman gerçekleşmezdi.
    [StringLength(30)]
    [Display(Name = "Cari kodu")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(200)]
    [Display(Name = "Unvan / Ad Soyad")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Hesap türü")]
    public CustomerAccountType AccountType { get; set; } = CustomerAccountType.Corporate;

    [StringLength(100)]
    [Display(Name = "Vergi dairesi")]
    public string? TaxOffice { get; set; }

    [StringLength(11)]
    [Display(Name = "Vergi numarası")]
    public string? TaxNumber { get; set; }

    [StringLength(11)]
    [Display(Name = "TC kimlik no")]
    public string? IdentityNumber { get; set; }

    [StringLength(100)]
    [Display(Name = "Cari grup")]
    public string? CustomerGroup { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    [Display(Name = "Risk limiti")]
    public decimal RiskLimit { get; set; }

    [Range(0, 3650)]
    [Display(Name = "Varsayılan vade (gün)")]
    public int? DefaultPaymentTermDays { get; set; }

    [StringLength(150)]
    [Display(Name = "Yetkili kişi")]
    public string? AuthorizedPerson { get; set; }

    [Range(typeof(decimal), "-999999999", "999999999")]
    [Display(Name = "Açılış bakiyesi")]
    public decimal OpeningBalance { get; set; }

    [StringLength(30)]
    [Display(Name = "Telefon")]
    public string? Phone { get; set; }

    [StringLength(200)]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string? Email { get; set; }

    [StringLength(500)]
    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [StringLength(100)]
    [Display(Name = "İl")]
    public string? City { get; set; }

    [StringLength(100)]
    [Display(Name = "İlçe")]
    public string? District { get; set; }

    [StringLength(1000)]
    [Display(Name = "Notlar")]
    public string? Notes { get; set; }

    [Display(Name = "Müşteri")]
    public bool IsCustomer { get; set; } = true;

    [Display(Name = "Tedarikçi")]
    public bool IsSupplier { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    // Çift tıklama/mükerrer POST koruması — bkz. Customer.SubmissionKey.
    public Guid SubmissionKey { get; set; } = Guid.NewGuid();
}
