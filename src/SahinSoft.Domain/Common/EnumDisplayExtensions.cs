using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SahinSoft.Domain.Common;

public static class EnumDisplayExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var display = field?.GetCustomAttribute<DisplayAttribute>();
        return display?.Name ?? value.ToString();
    }
}
