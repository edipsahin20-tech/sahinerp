using System.Security.Claims;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Services;

public enum ConversionAutoApprovalKind
{
    OrderToDispatch,
    OrderToInvoice,
    DispatchToInvoice
}

// Sipariş→İrsaliye, Sipariş→Fatura ve İrsaliye→Fatura dönüşümlerinde hedef belgenin taslak mı
// yoksa (kaydedilip hemen ardından) otomatik onaylı mı oluşacağını belirler — bkz. InventorySettings
// üzerindeki 6 parametre. Yalnızca Administrator rolündeki kullanıcılar için true dönebilir.
public static class ConversionAutoApprovalPolicy
{
    public static bool ShouldAutoApprove(
        InventorySettings settings,
        ConversionAutoApprovalKind kind,
        InvoiceType direction,
        ClaimsPrincipal user)
    {
        if (!user.IsInRole(AppRoles.Administrator))
        {
            return false;
        }

        return kind switch
        {
            ConversionAutoApprovalKind.OrderToDispatch => direction == InvoiceType.Sales
                ? settings.OrderToDispatchSalesAutoApprove
                : settings.OrderToDispatchPurchaseAutoApprove,
            ConversionAutoApprovalKind.OrderToInvoice => direction == InvoiceType.Sales
                ? settings.OrderToInvoiceSalesAutoApprove
                : settings.OrderToInvoicePurchaseAutoApprove,
            ConversionAutoApprovalKind.DispatchToInvoice => direction == InvoiceType.Sales
                ? settings.DispatchToInvoiceSalesAutoApprove
                : settings.DispatchToInvoicePurchaseAutoApprove,
            _ => false
        };
    }
}
