#Requires -Version 5.1
<#
    ŞahinSoft Ön Muhasebe - Otomatik Kurulum Betiği
    ------------------------------------------------
    Bu betik şunları otomatik yapmaya çalışır:
      1) IIS (Web Server) rolünü ve gerekli bileşenleri kurar
      2) ASP.NET Core Hosting Bundle'ı (.NET 10) kontrol eder, yoksa indirip kurar
      3) Yerel SQL Server'ı kontrol eder, yoksa SQL Server Express'i indirip kurar
      4) Uygulama zip dosyasını C:\SitesSahinSoft klasörüne açar
      5) IIS Uygulama Havuzu ve Site'ı oluşturur/günceller (port 1666)
      6) Veritabanı migration script'ini çalıştırır (sqlcmd varsa)

    NOT: Bu betik Yönetici (Administrator) olarak PowerShell'de çalıştırılmalıdır.
    Kurulum.bat dosyasına çift tıklarsanız bu otomatik olarak sağlanır.
#>

$ErrorActionPreference = "Stop"

$SiteName        = "SahinSoft"
$AppPoolName     = "SahinSoft"
$SitePath        = "C:\SitesSahinSoft"
$SitePort        = 1666
$ScriptDir       = Split-Path -Parent $MyInvocation.MyCommand.Path
$ZipCandidates   = Get-ChildItem -Path $ScriptDir -Filter "SahinSoft-Publish-Fresh.zip" -ErrorAction SilentlyContinue
$MigrationScript = Join-Path $ScriptDir "SahinSoftDb_Migration.sql"

function Write-Step($message) {
    Write-Host ""
    Write-Host "==> $message" -ForegroundColor Cyan
}

function Write-Ok($message) {
    Write-Host "    [OK] $message" -ForegroundColor Green
}

function Write-Warn($message) {
    Write-Host "    [UYARI] $message" -ForegroundColor Yellow
}

function Write-Fail($message) {
    Write-Host "    [HATA] $message" -ForegroundColor Red
}

function Test-IsAdministrator {
    $currentUser = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    return $currentUser.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-IsAdministrator)) {
    Write-Fail "Bu betik Yönetici olarak çalıştırılmalıdır. Kurulum.bat dosyasını kullanın veya PowerShell'i 'Yönetici olarak çalıştır' ile açıp tekrar deneyin."
    exit 1
}

Write-Host "========================================" -ForegroundColor DarkGray
Write-Host " ŞahinSoft Ön Muhasebe - Otomatik Kurulum" -ForegroundColor DarkGray
Write-Host "========================================" -ForegroundColor DarkGray

# ---------------------------------------------------------------------------
# 1) IIS rolü
# ---------------------------------------------------------------------------
Write-Step "IIS (Web Server) rolü kontrol ediliyor..."
try {
    $iisFeature = Get-WindowsFeature -Name Web-Server -ErrorAction Stop
    if ($iisFeature.InstallState -ne "Installed") {
        Write-Warn "IIS kurulu değil, kuruluyor (bu birkaç dakika sürebilir)..."
        Install-WindowsFeature -Name Web-Server, Web-Asp-Net45, Web-Net-Ext45, Web-WebSockets, Web-Http-Redirect, Web-Common-Http, Web-Static-Content, Web-Default-Doc, Web-Mgmt-Console -IncludeManagementTools | Out-Null
        Write-Ok "IIS kuruldu."
    }
    else {
        Write-Ok "IIS zaten kurulu."
    }
}
catch {
    Write-Warn "IIS durumu Get-WindowsFeature ile kontrol edilemedi (Server olmayan bir Windows sürümü olabilir). IIS'in zaten kurulu olduğunu varsayıp devam ediliyor."
}

Import-Module WebAdministration -ErrorAction SilentlyContinue

# ---------------------------------------------------------------------------
# 2) ASP.NET Core Hosting Bundle (.NET 10)
# ---------------------------------------------------------------------------
Write-Step ".NET 10 ASP.NET Core Hosting Bundle kontrol ediliyor..."
$hostingBundleInstalled = $false
try {
    $runtimes = & dotnet --list-runtimes 2>$null
    if ($runtimes -match "Microsoft.AspNetCore.App 10\.") {
        $hostingBundleInstalled = $true
    }
}
catch {
    $hostingBundleInstalled = $false
}

if ($hostingBundleInstalled) {
    Write-Ok "ASP.NET Core 10 çalışma zamanı zaten kurulu."
}
else {
    Write-Warn "ASP.NET Core 10 Hosting Bundle bulunamadı, indiriliyor..."
    $hostingBundleUrl = "https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-aspnetcore-10.0.0-windows-hosting-bundle-installer"
    $hostingBundlePath = Join-Path $env:TEMP "dotnet-hosting-bundle.exe"
    try {
        Invoke-WebRequest -Uri $hostingBundleUrl -OutFile $hostingBundlePath -UseBasicParsing
        Write-Ok "İndirme tamamlandı, kuruluyor..."
        Start-Process -FilePath $hostingBundlePath -ArgumentList "/quiet", "/install", "/norestart" -Wait
        Write-Ok "Hosting Bundle kuruldu. IIS'in bunu tanıması için birazdan iisreset çalıştırılacak."
    }
    catch {
        Write-Fail "Hosting Bundle otomatik indirilemedi/kurulamadı: $($_.Exception.Message)"
        Write-Warn "Lütfen https://dotnet.microsoft.com/download/dotnet/10.0 adresinden 'ASP.NET Core Runtime 10.x - Windows Hosting Bundle' dosyasını elle indirip kurun, sonra bu betiği tekrar çalıştırın."
    }
}

# ---------------------------------------------------------------------------
# 3) SQL Server kontrolü
# ---------------------------------------------------------------------------
Write-Step "Yerel SQL Server bağlantısı kontrol ediliyor..."
$sqlAvailable = $false
try {
    Add-Type -AssemblyName "System.Data" -ErrorAction SilentlyContinue
    $testConnection = New-Object System.Data.SqlClient.SqlConnection("Server=localhost;Integrated Security=True;Connection Timeout=5;TrustServerCertificate=True")
    $testConnection.Open()
    $testConnection.Close()
    $sqlAvailable = $true
}
catch {
    $sqlAvailable = $false
}

if ($sqlAvailable) {
    Write-Ok "Yerel SQL Server bulundu ve erişilebilir."
}
else {
    Write-Warn "Yerel SQL Server bulunamadı. SQL Server Express indiriliyor (bu işlem büyük bir dosya olduğundan uzun sürebilir)..."
    $sqlExpressUrl = "https://go.microsoft.com/fwlink/?linkid=866658"
    $sqlExpressInstaller = Join-Path $env:TEMP "SQL2022-SSEI-Expr.exe"
    try {
        Invoke-WebRequest -Uri $sqlExpressUrl -OutFile $sqlExpressInstaller -UseBasicParsing
        Write-Ok "İndirme tamamlandı, sessiz kurulum başlatılıyor..."
        Start-Process -FilePath $sqlExpressInstaller -ArgumentList "/ACTION=Install", "/IACCEPTSQLSERVERLICENSETERMS", "/QUIET", "/INSTANCENAME=MSSQLSERVER" -Wait
        Write-Ok "SQL Server Express kurulumu tamamlandı."
    }
    catch {
        Write-Fail "SQL Server Express otomatik kurulamadı: $($_.Exception.Message)"
        Write-Warn "Lütfen SQL Server'ı elle kurun (SQL Server Express yeterlidir), sonra bu betiği tekrar çalıştırın."
    }
}

# ---------------------------------------------------------------------------
# 4) Uygulama dosyalarını yayma
# ---------------------------------------------------------------------------
Write-Step "Uygulama dosyaları $SitePath klasörüne açılıyor..."
if (-not (Test-Path $SitePath)) {
    New-Item -Path $SitePath -ItemType Directory | Out-Null
}

if ($ZipCandidates) {
    $zipPath = $ZipCandidates[0].FullName
    try {
        if (Get-Service -Name "W3SVC" -ErrorAction SilentlyContinue) {
            Stop-Service -Name "W3SVC" -ErrorAction SilentlyContinue
        }
        Expand-Archive -Path $zipPath -DestinationPath $SitePath -Force
        Write-Ok "Uygulama dosyaları yayıldı ($zipPath)."
    }
    catch {
        Write-Fail "Zip açılırken hata: $($_.Exception.Message)"
    }
}
else {
    Write-Warn "SahinSoft-Publish-Fresh.zip bu klasörde bulunamadı ($ScriptDir). Bu adımı atlıyorum; dosyaları elle $SitePath içine kopyalamanız gerekir."
}

# ---------------------------------------------------------------------------
# 5) IIS Uygulama Havuzu ve Site
# ---------------------------------------------------------------------------
Write-Step "IIS Uygulama Havuzu ve Site kontrol ediliyor..."
try {
    if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
        New-WebAppPool -Name $AppPoolName | Out-Null
        Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
        Write-Ok "Uygulama havuzu '$AppPoolName' oluşturuldu (No Managed Code)."
    }
    else {
        Write-Ok "Uygulama havuzu '$AppPoolName' zaten mevcut."
    }

    if (-not (Test-Path "IIS:\Sites\$SiteName")) {
        New-Website -Name $SiteName -PhysicalPath $SitePath -ApplicationPool $AppPoolName -Port $SitePort | Out-Null
        Write-Ok "Site '$SiteName' oluşturuldu (port $SitePort)."
    }
    else {
        Write-Ok "Site '$SiteName' zaten mevcut, ayarlar korunuyor."
    }

    Start-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
    Start-Website -Name $SiteName -ErrorAction SilentlyContinue
}
catch {
    Write-Fail "IIS site/havuz ayarlanırken hata: $($_.Exception.Message)"
    Write-Warn "Bu adımı IIS Yöneticisi (inetmgr) üzerinden elle yapmanız gerekebilir."
}

# ---------------------------------------------------------------------------
# 6) Veritabanı migration script'i
# ---------------------------------------------------------------------------
Write-Step "Veritabanı migration script'i uygulanıyor..."
if (Test-Path $MigrationScript) {
    $sqlcmdPath = Get-Command sqlcmd -ErrorAction SilentlyContinue
    if ($sqlcmdPath) {
        try {
            & sqlcmd -S localhost -E -i $MigrationScript
            Write-Ok "Migration script'i çalıştırıldı."
        }
        catch {
            Write-Fail "Migration script'i çalıştırılırken hata: $($_.Exception.Message)"
            Write-Warn "Bu script'i SSMS üzerinden elle çalıştırabilirsiniz: $MigrationScript"
        }
    }
    else {
        Write-Warn "sqlcmd bulunamadı. Lütfen '$MigrationScript' dosyasını SSMS üzerinden elle çalıştırın."
    }
}
else {
    Write-Warn "Migration script'i bu klasörde bulunamadı ($MigrationScript). Bu adım atlandı."
}

# ---------------------------------------------------------------------------
# Bitti
# ---------------------------------------------------------------------------
Write-Step "Kurulum tamamlandı."
Write-Host ""
Write-Host "  Uygulama adresi: http://localhost:$SitePort" -ForegroundColor White
Write-Host ""
Write-Warn "Yukarıdaki adımlarda [UYARI]/[HATA] varsa lütfen ilgili bölümü tekrar okuyup elle tamamlayın."
Write-Host ""
Read-Host "Kapatmak için Enter'a basın"
