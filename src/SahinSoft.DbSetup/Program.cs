namespace SahinSoft.DbSetup;

// İKİ MOD tek exe'de:
// 1) Kurulum.ps1 tarafından argümanlarla çağrıldığında (SahinSoftDbKur.exe <sunucu> <kullanıcı>
//    <şifre>) - eskisi gibi sessiz konsol modu, tam kurulum akışının bir parçası, GUI açılmaz.
// 2) Edip elle çift tıkladığında (argümansız) - artık otomatik sıfırlamıyor, bunun yerine
//    Oluştur/Güncelle/Yedek Al butonları + değiştirilebilir veritabanı adı olan bir GUI açılıyor.
internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length > 0)
        {
            return RunSilentConsoleMode(args);
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
        return 0;
    }

    private static int RunSilentConsoleMode(string[] args)
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
        catch
        {
            // Kritik değil.
        }

        var server = args[0];
        var user = args.Length > 1 ? args[1] : "sa";
        var password = args.Length > 2 ? args[2] : "SahinSoft2026!Kurulum";
        const string databaseName = "SahinSoftDb";

        Console.WriteLine("========================================");
        Console.WriteLine(" ŞahinSoft Veritabanı Kurulum Aracı");
        Console.WriteLine("========================================");
        Console.WriteLine($"Sunucu: {server}");
        Console.WriteLine();

        try
        {
            DbOperations.DropDatabaseIfExists(server, user, password, databaseName, Console.WriteLine);
            DbOperations.CreateDatabaseIfNotExists(server, user, password, databaseName, Console.WriteLine);
            DbOperations.RunMigration(server, user, password, databaseName, Console.WriteLine);
            DbOperations.RecycleIisAppPool(Console.WriteLine);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Veritabanı başarıyla kuruldu.");
            Console.ResetColor();
            return WaitAndExit(0);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine($"HATA: {ex.Message}");
            Console.ResetColor();
            return WaitAndExit(1);
        }
    }

    private static int WaitAndExit(int code)
    {
        // Kurulum.ps1 bu exe'yi çıktısı yönlendirilmiş şekilde ("-RedirectStandardOutput")
        // otomatik çağırıyor - o durumda tuş bekleme SONSUZA KADAR asılı kalırdı (kimse
        // basamaz). Sadece kullanıcı çift tıklayıp elle çalıştırdığında (çıktı yönlendirilmemiş)
        // bekliyoruz.
        if (!Console.IsOutputRedirected)
        {
            Console.WriteLine();
            Console.WriteLine("Kapatmak için bir tuşa basın...");
            Console.ReadKey();
        }
        return code;
    }
}
