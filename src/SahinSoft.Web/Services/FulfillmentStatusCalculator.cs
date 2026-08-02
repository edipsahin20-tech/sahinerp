using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Services;

// BusinessOrder ve DispatchNote'un "ne kadarı aşağı akış belgesine dönüştürüldü" durumunu
// (Approved/PartiallyFulfilled/Fulfilled) satır bazındaki Quantity/Karşılanan çiftlerinden hesaplar.
// İki modülde de (Sipariş→İrsaliye/Fatura, İrsaliye→Fatura) aynı mantık kullanıldığı için tek yerde.
public static class FulfillmentStatusCalculator
{
    public static BusinessDocumentStatus Calculate(IEnumerable<(decimal Quantity, decimal Fulfilled)> lines)
    {
        var list = lines.ToList();
        if (list.Count == 0)
        {
            return BusinessDocumentStatus.Approved;
        }

        if (list.All(x => x.Fulfilled >= x.Quantity))
        {
            return BusinessDocumentStatus.Fulfilled;
        }

        return list.Any(x => x.Fulfilled > 0)
            ? BusinessDocumentStatus.PartiallyFulfilled
            : BusinessDocumentStatus.Approved;
    }
}
