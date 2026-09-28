using SahinSoft.PrintAgent.Config;
using SahinSoft.PrintAgent.Spooler;

var builder = WebApplication.CreateBuilder(args);

// agent.config.json = SahinSoft.FiscalAgent ile AYNI desen - kasa PC'sine kurulan bu küçük
// agent için tek yapılandırma dosyası.
builder.Configuration.AddJsonFile("agent.config.json", optional: false, reloadOnChange: true);
builder.Services.Configure<PrintAgentConfig>(builder.Configuration);

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
        }
    });
});

var listenPort = builder.Configuration.GetValue<int>("Listen:Port", 5058);
builder.WebHost.UseUrls($"http://localhost:{listenPort}");

var app = builder.Build();
app.UseCors();

app.Logger.LogInformation("ŞahinSoft Print Agent başlatılıyor - dinleme portu {Port}", listenPort);

app.MapGet("/health", () => Results.Ok(new { ok = true }));

app.MapGet("/printers", () =>
{
    if (!OperatingSystem.IsWindows())
    {
        return Results.Ok(Array.Empty<string>());
    }
    return Results.Ok(RawPrinterHelper.GetInstalledPrinterNames());
});

app.MapPost("/print/job", (PrintJobRequest request, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(request.PrinterName))
    {
        return Results.BadRequest(new { success = false, error = "printerName zorunludur." });
    }
    if (string.IsNullOrWhiteSpace(request.DataBase64))
    {
        return Results.BadRequest(new { success = false, error = "dataBase64 zorunludur." });
    }

    if (!OperatingSystem.IsWindows())
    {
        logger.LogWarning("Bu platform Windows değil - RAW spooler yazımı atlandı (geliştirme/test ortamı).");
        return Results.Ok(new { success = false, error = "PrintAgent sadece Windows üzerinde çalışır." });
    }

    byte[] bytes;
    try
    {
        bytes = Convert.FromBase64String(request.DataBase64);
    }
    catch (FormatException)
    {
        return Results.BadRequest(new { success = false, error = "dataBase64 geçersiz." });
    }

    var (success, error) = RawPrinterHelper.SendRawBytes(request.PrinterName, bytes);
    if (!success)
    {
        logger.LogWarning("Yazdırma başarısız: {Error}", error);
        return Results.Ok(new { success = false, error });
    }

    return Results.Ok(new { success = true });
});

app.Run();

public sealed record PrintJobRequest(string PrinterName, string DataBase64);
