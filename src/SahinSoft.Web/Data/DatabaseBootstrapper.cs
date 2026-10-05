using Microsoft.EntityFrameworkCore;

namespace SahinSoft.Web.Data;

// Bir veritabanını programın çalışabilir hale getirir: tablolar (migration), roller, yönetici hesabı,
// sistem kasiyeri (PIN 66) ve yazdırma şablonları. Uygulama açılışındaki adımların aynısı - tek yerde
// tutulur ki yeni bir firma veritabanı da tam aynı şekilde hazırlansın. Çağıran, DbContext'in hedef
// veritabanına yönlendirilmiş bir istek kapsamında olmalı (bkz. DatabaseRouter).
public static class DatabaseBootstrapper
{
    public static async Task PrepareAsync(IServiceProvider services, IConfiguration configuration)
    {
        using (var scope = services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        await IdentitySeed.InitializeAsync(services, configuration);
        await PrintTemplateSeed.InitializeAsync(services);
    }
}
