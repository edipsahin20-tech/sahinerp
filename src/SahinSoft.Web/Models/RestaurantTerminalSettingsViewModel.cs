using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Models;

// Terminal Ayarları (Edip, 2026-09-27: "giriş sayfasına ayarlar bölümü... kasa numarası kredi ve
// nakit kasası, şube ayarlarını yapabileceğim, yazar kasa ayarlarını yapabileceğim programa özel
// ayarlar bölümü - direkt muhasebe ayarlar bölümüne bağlanmasın ama aynı tabloya yazsın") - bu
// ekran RestaurantCashRegister/Branch/InventorySettings'e (ana ERP'nin GERÇEK tablolarına) yazar,
// yeni bir "restoran-only" ayrı ayar deposu İCAT EDİLMEDİ - sadece kiosk terminaline özel, sade bir
// arayüzle sunulur (BranchesController/RestaurantCashRegistersController/SettingsController'ın
// genel ERP ekranlarına yönlendirmez).
public sealed class RestaurantTerminalSettingsViewModel
{
    public List<Branch> Branches { get; set; } = [];
    public List<RestaurantCashRegister> CashRegisters { get; set; } = [];
    public List<FinancialAccount> FinancialAccounts { get; set; } = [];
    public FiscalDeviceType FiscalDeviceType { get; set; }
    public string? FiscalAgentUrl { get; set; }
    public int? TerminalBranchId { get; set; }
    public int? TerminalRegisterId { get; set; }
    public List<DatabaseProfile> DatabaseProfiles { get; set; } = [];
    public string? CurrentDatabaseName { get; set; }
    public bool IsDesktopTerminal { get; set; }
}
