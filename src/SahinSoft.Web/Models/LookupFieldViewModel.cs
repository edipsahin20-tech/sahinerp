namespace SahinSoft.Web.Models;

public sealed class LookupFieldViewModel
{
    public required string Name { get; init; }
    public required string Id { get; init; }
    public string? Value { get; init; }
    public string? DisplayValue { get; init; }
    public required string Endpoint { get; init; }
    public required string Title { get; init; }
    public string Placeholder { get; init; } = "Seçmek için tıklayın";
    public bool AllowQuickSearch { get; init; }
}
