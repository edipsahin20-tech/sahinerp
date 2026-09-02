using Inpos;
using SahinSoft.FiscalAgent.Config;
using SahinSoft.FiscalAgent.Inpos;
using SahinSoft.FiscalAgent.Models;

var builder = WebApplication.CreateBuilder(args);

// agent.config.json = tek yapılandırma kaynağı (appsettings.json değil, kasa PC'sine kurulacak
// bu küçük agent için tek dosyayı elle düzenlemek/gmp3 sonrası doldurmak daha basit).
builder.Configuration.AddJsonFile("agent.config.json", optional: false, reloadOnChange: true);
builder.Services.Configure<FiscalAgentConfig>(builder.Configuration);

builder.Services.AddSingleton<InposDeviceService>();
builder.Services.AddSingleton<SaleOrchestrator>();

// Ödeme/satış işlemleri aynı anda TEK cihaza karşı çalışabilir - eşzamanlı iki isteğin
// InposExt.Net.dll'in (thread-safe olduğu belirtilmemiş) native durumunu karıştırmaması için
// tüm satış çağrıları bu semaphore ile sıraya alınır.
builder.Services.AddSingleton(new SemaphoreSlim(1, 1));

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

var listenPort = builder.Configuration.GetValue<int>("Listen:Port", 9595);
builder.WebHost.UseUrls($"http://localhost:{listenPort}");

var app = builder.Build();
app.UseCors();

// Basit istek günlüğü - kasa PC'sinde çalışacağı için ayrıntılı bir loglama altyapısına gerek
// yok, konsola yazan varsayılan logger yeterli (bkz. appsettings altyapısı kurulmadı, bilinçli).
app.Logger.LogInformation("ŞahinSoft Fiscal Agent başlatılıyor - dinleme portu {Port}", listenPort);

app.MapGet("/health", () => Results.Ok(new { ok = true, version = InposExt.Version() }));

app.MapGet("/device/status", async (InposDeviceService device, CancellationToken ct) =>
{
    if (device.SimulationMode)
    {
        return Results.Ok(new DeviceStatusResult
        {
            Connected = true,
            EcrState = "Simulated",
            SaleState = "Simulated",
            SimulationMode = true
        });
    }

    try
    {
        await device.EnsureInitializedAsync(ct);
        var (ecrError, ecrState) = await device.CheckEcrStatusAsync(ct);
        var (saleError, saleState, _) = await device.CheckSaleStatusAsync(ct);

        return Results.Ok(new DeviceStatusResult
        {
            Connected = ecrError == InposExtError.InposNoError,
            EcrState = ecrState.ToString(),
            SaleState = saleState.ToString(),
            SimulationMode = false,
            ErrorMessage = ecrError != InposExtError.InposNoError || saleError != InposExtError.InposNoError
                ? $"{ecrError}/{saleError} ({InposDeviceService.SafeErrorDetail()})"
                : null
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Ok(new DeviceStatusResult { Connected = false, SimulationMode = false, ErrorMessage = ex.Message });
    }
});

app.MapPost("/sale/process", async (FiscalSaleRequest request, SaleOrchestrator orchestrator, SemaphoreSlim gate, CancellationToken ct) =>
{
    await gate.WaitAsync(ct);
    try
    {
        var result = await orchestrator.ProcessSaleAsync(request, ct);
        return Results.Ok(result);
    }
    finally
    {
        gate.Release();
    }
});

app.MapPost("/sale/cancel", async (SaleOrchestrator orchestrator, SemaphoreSlim gate, CancellationToken ct) =>
{
    await gate.WaitAsync(ct);
    try
    {
        var result = await orchestrator.CancelSaleAsync(ct);
        return Results.Ok(result);
    }
    finally
    {
        gate.Release();
    }
});

app.Run();
