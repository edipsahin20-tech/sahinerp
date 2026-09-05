using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

// Kasa Tanımları (Edip, 2026-09-04/05, madde 27) - "Her kasa için şube bazlı tanım yapılabilmelidir
// ... Hangi şubedeki hangi kasanın hangi hesabı kullandığı KESİN OLARAK İZLENMELİDİR." Öncesinde
// restoran ödemeleri hangi ödeme yöntemi (Nakit/Kredi Kartı/Yemek Çeki) olursa olsun HER ZAMAN
// FinancialAccounts listesinin ilk (alfabetik) kaydına gidiyordu (bkz. restaurant-close-payment.js
// eski "financialAccounts[0]" deseni) - gerçek bir muhasebe hatasıydı. Bu tablo şube bazlı bir kasa
// tanımının hangi FinancialAccount'a (Nakit/Kredi Kartı-Banka/Yemek Kartı) bağlı olduğunu tutar;
// ayrı bir "restoran-only" hesap kopyası DEĞİL, ana ERP'nin FinancialAccounts tablosuna referans.
public sealed class RestaurantCashRegister : EntityBase
{
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public int CashFinancialAccountId { get; set; }
    public FinancialAccount CashFinancialAccount { get; set; } = null!;

    public int CreditCardFinancialAccountId { get; set; }
    public FinancialAccount CreditCardFinancialAccount { get; set; } = null!;

    // Yemek kartı için ayrı hesap opsiyonel - tanımlanmazsa ödeme kredi kartı hesabına düşer
    // (çoğu işletmede ikisi zaten aynı POS/banka hesabından geçer).
    public int? MealCardFinancialAccountId { get; set; }
    public FinancialAccount? MealCardFinancialAccount { get; set; }

    // Yazar kasa/POS-İmPOS/diğer entegrasyon bağlantı notu - serbest metin. Gerçek cihaz eşleşmesi
    // hâlâ InventorySettings'teki (sistem geneli) FiscalDeviceType/FiscalAgentUrl üzerinden;
    // kasa bazlı ayrı cihaz eşleştirmesi bu pakette İCAT EDİLMEDİ (spec'in "sonraki paket" dediği
    // Paket Operasyon Merkezi entegrasyonlarıyla aynı kapsam dışı sınıf - bkz. madde 35 notu).
    public string? Note { get; set; }
}
