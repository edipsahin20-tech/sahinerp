using Microsoft.AspNetCore.Identity;

namespace SahinSoft.Web.Data;

public sealed class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
