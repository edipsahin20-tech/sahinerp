using System.ComponentModel.DataAnnotations;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Models;

public sealed class PrinterFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Yazıcı Adı")]
    [Required(ErrorMessage = "Yazıcı adı zorunludur.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Rol")]
    public PrinterRole Role { get; set; } = PrinterRole.Adisyon;

    [Display(Name = "Şube")]
    [Required(ErrorMessage = "Şube seçimi zorunludur.")]
    public int? BranchId { get; set; }
    public string? BranchDisplay { get; set; }

    [Display(Name = "Bağlantı Tipi")]
    public PrinterConnectionType ConnectionType { get; set; } = PrinterConnectionType.NetworkRaw9100;

    [Display(Name = "Bağlantı Adresi")]
    [Required(ErrorMessage = "Bağlantı adresi zorunludur.")]
    public string ConnectionAddress { get; set; } = string.Empty;

    [Display(Name = "Agent Adresi (sadece Windows Yazıcı için)")]
    public string? AgentBaseUrl { get; set; }

    [Display(Name = "Kağıt Genişliği")]
    public PrinterPaperWidth PaperWidth { get; set; } = PrinterPaperWidth.Mm80;

    [Display(Name = "Otomatik Kesici")]
    public bool AutoCut { get; set; } = true;

    [Display(Name = "Kopya Sayısı")]
    [Range(1, 10)]
    public int CopyCount { get; set; } = 1;

    [Display(Name = "Şablon")]
    public int? PrintTemplateId { get; set; }
    public List<(int Id, string Name)> AvailableTemplates { get; set; } = [];

    [Display(Name = "Mutfak İstasyonu")]
    public int? KitchenStationId { get; set; }
    public List<(int Id, string Name)> AvailableKitchenStations { get; set; } = [];

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
