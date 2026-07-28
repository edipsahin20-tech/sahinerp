namespace SahinSoft.Web.Models;

public sealed class EvrakToolbarViewModel
{
    public int? Id { get; set; }
    public string Controller { get; set; } = string.Empty;
    public Dictionary<string, string>? CreateRouteValues { get; set; }
    public int? PreviousId { get; set; }
    public int? NextId { get; set; }
    public bool CanDelete { get; set; }
    public string? DeleteBlockedReason { get; set; }
    public bool HasDetails { get; set; }
}
