using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class ExchangeRate : EntityBase
{
    public DateTime RateDateUtc { get; set; }
    public decimal BuyingRate { get; set; }
    public decimal SellingRate { get; set; }
    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;
}
