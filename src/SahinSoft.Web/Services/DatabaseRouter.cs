namespace SahinSoft.Web.Services;

// Her istek kendi veritabanını kullanır: muhasebe giriş formunda seçilen firma (ilk POST'ta Items'a
// konur), sonra oturum çerezi ("ss_db"). Masaüstü penceresi (SahinSoft.DesktopShell, User-Agent'ında
// DesktopUserAgentMarker taşır) HER ZAMAN etkin (ConfigTool'dan Etkinleştirilen) veritabanını kullanır -
// seçim yapamaz. Varsayılan = profil listesinde "varsayılan" işaretli firma; yoksa ConnectionStrings:DefaultConnection.
// Arka plan görevleri de aynı varsayılanı kullanır.
public sealed class DatabaseRouter(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, DatabaseCatalog catalog)
{
    public const string SelectionKey = "ss_db";
    public const string DesktopUserAgentMarker = "SahinSoftDesktop";

    public static bool IsDesktop(HttpContext? httpContext) =>
        httpContext?.Request.Headers.UserAgent.ToString().Contains(DesktopUserAgentMarker, StringComparison.OrdinalIgnoreCase) == true;

    public string ConnectionString
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (!IsDesktop(httpContext))
            {
                var name = httpContext?.Items[SelectionKey] as string
                    ?? httpContext?.Request.Cookies[SelectionKey];

                var profile = catalog.Find(name);
                if (profile is not null)
                {
                    return profile.BuildConnectionString();
                }
            }

            var defaultProfile = catalog.Default();
            if (defaultProfile is not null)
            {
                return defaultProfile.BuildConnectionString();
            }

            return configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }
    }
}
