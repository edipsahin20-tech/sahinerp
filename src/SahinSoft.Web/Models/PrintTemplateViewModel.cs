using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Services.Printing;

namespace SahinSoft.Web.Models;

public sealed class PrintTemplateListViewModel
{
    public List<PrintTemplateListItem> Items { get; set; } = [];
}

public sealed record PrintTemplateListItem(int Id, string Name, PrintTemplateType TemplateType, int PaperWidthMm, bool IsDefault, bool IsActive, int Version);

public sealed class PrintTemplateDesignerViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PrintTemplateType TemplateType { get; set; }
    public int PaperWidthMm { get; set; } = 80;
    public bool IsDefault { get; set; }
    public int Version { get; set; }
    public string LayoutJson { get; set; } = "[]";
    public int? BranchId { get; set; }
    public string? BranchDisplay { get; set; }
    public List<Branch> Branches { get; set; } = [];

    public IReadOnlyList<PrintFieldDefinition> AvailableFields { get; set; } = [];
    public string PreviewHtml { get; set; } = "";
}
