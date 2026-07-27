using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class Currency : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsBaseCurrency { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ExchangeRate> ExchangeRates { get; set; } = new List<ExchangeRate>();
}
