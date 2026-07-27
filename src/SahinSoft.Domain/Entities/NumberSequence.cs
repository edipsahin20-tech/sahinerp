using SahinSoft.Domain.Common;

namespace SahinSoft.Domain.Entities;

public sealed class NumberSequence : EntityBase
{
    public string Key { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public long NextNumber { get; set; } = 1;
    public int Padding { get; set; } = 3;
}
