#Requires -Version 5.1
<#
    ŞahinSoft Ön Muhasebe - Otomatik Kurulum Betiği
    ------------------------------------------------
    Bu betik şunları otomatik yapmaya çalışır:
      1) IIS (Web Server) rolünü ve gerekli bileşenleri kurar
      2) ASP.NET Core Hosting Bundle'ı (.NET 10) kontrol eder, yoksa indirip kurar
      3) Yerel SQL Server'ı kontrol eder, yoksa SQL Server Express'i indirip kurar
      4) Uygulama zip dosyasını C:\SitesSahinSoft klasörüne açar
      5) appsettings.json içindeki bağlantı dizesini bu makinenin SQL Server'ına ayarlar
      6) IIS Uygulama Havuzu ve Site'ı oluşturur/günceller (port 1666)
      7) Veritabanı migration script'ini çalıştırır (sqlcmd varsa)

    Bu betik hem tek-sunucu (bulut VPS) kurulumunda hem de bir şube bilgisayarına
    tamamen yerel/offline çalışacak bir kurulum yapmakta kullanılabilir - her iki
    durumda da IIS ve SQL Server aynı makinede, appsettings.json "localhost"a bakar.
    Şube daha sonra merkeze bağlanacaksa (hibrit senkron), bu adım tamamlandıktan
    sonra SahinSoftVeritabaniAyarlari.exe (ConfigTool) ile bağlantı hedefi değiştirilebilir.

    NOT: Bu betik Yönetici (Administrator) olarak PowerShell'de çalıştırılmalıdır.
    Kurulum.bat dosyasına çift tıklarsanız bu otomatik olarak sağlanır.
#>

param(
    # Varsayılan: bu makinede SQL Server Express kurulur (Karma Kimlik Doğrulama), sa şifresi
    # aşağıdaki sabit değerdir - appsettings.json'a yazılan bağlantı dizesi de aynısını kullanır.
    [string]$SqlSaPassword = "SahinSoft2026!Kurulum",
    # Bir şube kurulumunu baştan var olan bir sunucuya (merkez PC / bulut) bağlamak isterseniz bu
    # parametreyi geçin, örn: -RemoteSqlServer "10.0.0.5" -SqlUser sa -SqlPassword "..."
    [string]$RemoteSqlServer = "",
    [string]$SqlUser = "",
    [string]$SqlPassword = ""
)

$ErrorActionPreference = "Stop"

# Windows PowerShell 5.1, bazı Windows 10 kurulumlarında HTTPS istekleri için varsayılan olarak
# TLS 1.2'yi AÇMAZ (eski SSL3/TLS1.0 ile dener). Microsoft'un indirme sunucuları (Hosting Bundle,
# SQL Server Express) TLS 1.2 zorunlu kılıyor - bu ayarlanmadan yapılan indirmeler ya hata verir
# ya da sessizce küçük bir hata sayfası indirir (indirilen dosya "geçerli bir uygulama değil"
# hatası verir - tam bunu yaşadık). Her Invoke-WebRequest'ten ÖNCE bunu ayarlamak gerekiyor.
try {
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
}
catch {
    # Çok eski bir .NET Framework'te Tls12 sabiti bile yoksa (çok nadir) - kritik değil, devam et.
}

# Konsol Türkçe karakterleri (Ş, ı, ğ...) kodlama uyuşmazlığı yüzünden bozuk (mojibake)
# gösteriyordu. [Console]::OutputEncoding = UTF8 TEK BAŞINA yetmiyor - Windows PowerShell 5.1,
# çıktı bir işlem tarafından yönlendirilmişse (bizim durumumuzda: kurulum penceresi çıktıyı
# yakalayıp kendi ekranında gösteriyor) bu ayarı güvenilir şekilde uygulamıyor, hâlâ eski
# OEM kod sayfasıyla yazıyor. Kesin çözüm: stdout'un ALT YAZICISINI (StreamWriter) doğrudan
# UTF-8 olarak DEĞİŞTİRMEK - bu, [Console]::OutputEncoding'in güvenilmez olduğu durumlarda bile
# çalışır.
try {
    $stdout = [System.Console]::OpenStandardOutput()
    $utf8Writer = New-Object System.IO.StreamWriter($stdout, [System.Text.Encoding]::UTF8)
    $utf8Writer.AutoFlush = $true
    [System.Console]::SetOut($utf8Writer)
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    $OutputEncoding = [System.Text.Encoding]::UTF8
}
catch {
    # Bazı kısıtlı konsol ortamlarında (ör. ISE) ayarlanamayabilir - kritik değil, devam et.
}

$SiteName        = "SahinSoft"
$AppPoolName     = "SahinSoft"
$SitePath        = "C:\SitesSahinSoft"
$SitePort        = 1666

# Büyük indirmeler (Hosting Bundle ~113MB, SQL Server paketi ~250MB) SABİT bir önbellek
# klasörüne konur - her deneme rastgele bir %TEMP% alt klasörü kullandığı için (kilitli dosya
# sorununu önlemek için, bkz. aşağıdaki $sqlWorkRoot) önbelleksiz her tekrar denemede aynı
# devasa dosyalar sıfırdan iniyordu. Bu klasör kalıcı - bir kere inen dosya bir daha inmez.
$DownloadCacheDir = "$env:ProgramData\SahinSoftKurulumOnbellek"
if (-not (Test-Path $DownloadCacheDir)) {
    New-Item -Path $DownloadCacheDir -ItemType Directory -Force | Out-Null
}
$ScriptDir       = Split-Path -Parent $MyInvocation.MyCommand.Path
$ZipCandidates   = Get-ChildItem -Path $ScriptDir -Filter "SahinSoft-Publish-Fresh.zip" -ErrorAction SilentlyContinue
$MigrationScript = Join-Path $ScriptDir "SahinSoftDb_Migration.sql"

# Türkçe karakterler konsol/pipe üzerinden bozuk (mojibake) çıkıyordu - [Console]::OutputEncoding
# ve chcp 65001 gibi çözümler denendi, hiçbiri PowerShell 5.1'in yönlendirilmiş çıktısında
# güvenilir olmadı. KESİN çözüm: konsolu tamamen devre dışı bırakıp DOSYAYA yazmak - dosya
# yazma her zaman istenen kodlamayla çalışır, konsolun/pipe'ın iç mekanizmasına bağlı değildir.
# Kurulum penceresi (SahinSoft.Setup) bu dosyayı okuyup gösteriyor.
$LogFilePath = Join-Path $ScriptDir "kurulum.log"
if (Test-Path $LogFilePath) { Remove-Item $LogFilePath -Force -ErrorAction SilentlyContinue }

function Write-Log([string]$text) {
    try {
        [System.IO.File]::AppendAllText($LogFilePath, $text + "`r`n", [System.Text.Encoding]::UTF8)
    }
    catch {
        # Dosyaya yazılamazsa sessizce devam - Write-Host çıktısı yine de konsolda kalır.
    }
}

function Write-Step($message) {
    Write-Host ""
    Write-Host "==> $message" -ForegroundColor Cyan
    Write-Log "==> $message"
}

function Write-Ok($message) {
    Write-Host "    [OK] $message" -ForegroundColor Green
    Write-Log "[OK] $message"
}

function Write-Warn($message) {
    Write-Host "    [UYARI] $message" -ForegroundColor Yellow
    Write-Log "[UYARI] $message"
}

function Write-Fail($message) {
    Write-Host "    [HATA] $message" -ForegroundColor Red
    Write-Log "[HATA] $message"
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
# Get-WindowsFeature/Install-WindowsFeature YALNIZCA Windows Server'da vardır (ServerManager
# modülü). Windows 10/11 (şube PC'lerinin çoğu bu) gibi normal masaüstü sürümlerinde bu komutlar
# YOKTUR - önceki sürümde bu durumda hata try/catch ile yutuluyor, "zaten kurulu olduğu
# varsayılıyor" deniyor ama IIS GERÇEKTEN KURULMUYORDU (Edip'in dokunmatik ekran PC'sinde tam
# bu şekilde fark edildi). Doğru komut Windows 10/11'de Get-WindowsOptionalFeature/
# Enable-WindowsOptionalFeature (DISM tabanlı) - Dinosoft'un kendi kurulumunda da aynısı
# kullanılıyor (bkz. DINOSOFT SETUP.exe içindeki "Enable-WindowsOptionalFeature -Online
# -FeatureName IIS-WebServer -All -NoRestart" metni).
Write-Step "IIS (Web Server) rolü kontrol ediliyor..."
$isServerSku = $null -ne (Get-Command Get-WindowsFeature -ErrorAction SilentlyContinue)

if ($isServerSku) {
    try {
        $iisFeature = Get-WindowsFeature -Name Web-Server -ErrorAction Stop
        if ($iisFeature.InstallState -ne "Installed") {
            Write-Warn "IIS kurulu değil, kuruluyor (bu birkaç dakika sürebilir)..."
            Install-WindowsFeature -Name Web-Server, Web-Asp-Net45, Web-Net-Ext45, Web-WebSockets, Web-Http-Redirect, Web-Common-Http, Web-Static-Content, Web-Default-Doc, Web-Mgmt-Console -IncludeManagementTools | Out-Null
            Write-Ok "IIS kuruldu (Windows Server)."
        }
        else {
            Write-Ok "IIS zaten kurulu."
        }
    }
    catch {
        Write-Fail "IIS kurulamadı: $($_.Exception.Message)"
    }
}
else {
    # Windows 10/11 masaüstü sürümü.
    $clientFeatures = @(
        "IIS-WebServerRole", "IIS-WebServer", "IIS-CommonHttpFeatures", "IIS-HttpErrors",
        "IIS-HttpRedirect", "IIS-ApplicationDevelopment", "IIS-Security", "IIS-RequestFiltering",
        "IIS-NetFxExtensibility45", "IIS-HealthAndDiagnostics", "IIS-HttpLogging",
        "IIS-Performance", "IIS-WebServerManagementTools", "IIS-ManagementConsole",
        "IIS-ManagementScriptingTools", "IIS-StaticContent", "IIS-DefaultDocument",
        "IIS-ASPNET45", "IIS-ISAPIExtensions", "IIS-ISAPIFilter", "IIS-WebSockets"
    )
    try {
        # Her özellik için AYRI DISM çağrısı yapmak (hem tespitte hem kurulumda) çok yavaştı -
        # her çağrının kendi başlatma maliyeti var, 20 özellik x 2 (kontrol+kurulum) = 40 DISM
        # çağrısı dakikalarca sürüyordu. Tek seferlik toplu sorgu + tek seferlik toplu kurulum
        # (-FeatureName virgüllü liste kabul ediyor) çok daha hızlı - Dinosoft'un da muhtemelen
        # yaptığı bu.
        $allFeatures = Get-WindowsOptionalFeature -Online
        $missing = $clientFeatures | Where-Object {
            $name = $_
            $match = $allFeatures | Where-Object { $_.FeatureName -eq $name }
            -not $match -or $match.State -ne "Enabled"
        }
        if ($missing.Count -gt 0) {
            Write-Warn "IIS bileşenleri eksik ($($missing -join ', ')), kuruluyor (bu birkaç dakika sürebilir)..."
            $result = Enable-WindowsOptionalFeature -Online -FeatureName $missing -All -NoRestart -ErrorAction Stop

            # DİKKAT: Enable-WindowsOptionalFeature hata fırlatmadan dönmesi GERÇEKTEN kurulduğu
            # anlamına gelmiyor - bazı durumlarda "RestartNeeded" ile döner ve özellik tam
            # etkinleşmeden yeniden başlatma bekler. Burada TEKRAR sorgulayıp gerçek durumu
            # doğruluyoruz, körü körüne "[OK]" basmıyoruz.
            if ($result.RestartNeeded) {
                Write-Warn "IIS bileşenleri kuruldu ama YENİDEN BAŞLATMA gerekiyor - şimdi Windows'u yeniden başlatıp bu kurulumu tekrar çalıştırın, aksi halde IIS tam çalışmayabilir."
            }

            $stillMissing = Get-WindowsOptionalFeature -Online | Where-Object {
                $missing -contains $_.FeatureName -and $_.State -ne "Enabled"
            }
            if ($stillMissing.Count -gt 0) {
                Write-Fail "Şu bileşenler kurulduktan SONRA bile hâlâ etkin değil: $($stillMissing.FeatureName -join ', ') - Windows'u yeniden başlatıp tekrar deneyin."
            }
            else {
                Write-Ok "IIS kuruldu ve doğrulandı (Windows 10/11)."
            }
        }
        else {
            Write-Ok "IIS zaten kurulu."
        }
    }
    catch {
        Write-Fail "IIS kurulamadı: $($_.Exception.Message)"
        Write-Warn "Denetim Masası > Programlar > Windows Özelliklerini Aç/Kapat'tan 'İnternet Bilgi Hizmetleri'ni elle işaretleyip deneyin."
    }
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
    # ÖNEMLİ: Hem "thank-you" sayfası hem Microsoft'un "permalink" yönlendirme linki denendi,
    # ikisi de PowerShell'in Invoke-WebRequest'inde küçük/geçersiz bir dosya (yönlendirme/hata
    # sayfası) olarak indi - "bu işletim sistemi platformu için geçerli bir uygulama değil"
    # hatası tam bu yüzden çıktı. Doğrusu Microsoft'un CDN'indeki SABİT sürüm dosyasına doğrudan
    # bağlanmak (builds.dotnet.microsoft.com) - yönlendirme yok, doğrudan binary iniyor.
    $hostingBundleUrl = "https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/10.0.0/dotnet-hosting-10.0.0-win.exe"
    $hostingBundlePath = Join-Path $DownloadCacheDir "dotnet-hosting-10.0.0-win.exe"
    try {
        # Önbellekte zaten tam (>=50MB) bir dosya varsa, tekrar indirme - direkt kullan.
        $downloadedSize = if (Test-Path $hostingBundlePath) { (Get-Item $hostingBundlePath).Length } else { 0 }
        if ($downloadedSize -ge 50MB) {
            Write-Ok "Hosting Bundle önbellekte bulundu ($([math]::Round($downloadedSize/1MB, 1)) MB), tekrar indirilmiyor."
        }
        else {
            # Gerçek dosya ~113 MB (doğrulandı). Kurulumda bir kere 214 KB gibi kesik bir dosya indi -
            # muhtemelen antivirüs taraması veya geçici ağ kesintisi. 3 deneme + gerçekçi boyut eşiği
            # (50MB) ile hem geçici kesintilere karşı dayanıklı hem yanlış dosyayı çalıştırmaya karşı
            # güvenli hale getiriyoruz.
            for ($attempt = 1; $attempt -le 3; $attempt++) {
                Invoke-WebRequest -Uri $hostingBundleUrl -OutFile $hostingBundlePath -UseBasicParsing -TimeoutSec 300
                $downloadedSize = (Get-Item $hostingBundlePath).Length
                if ($downloadedSize -ge 50MB) {
                    break
                }
                Write-Warn "İndirme eksik göründü (deneme $attempt/3, $([math]::Round($downloadedSize/1MB, 1)) MB) - tekrar deneniyor..."
                Start-Sleep -Seconds 3
            }
        }

        if ($downloadedSize -lt 50MB) {
            Remove-Item $hostingBundlePath -Force -ErrorAction SilentlyContinue
            throw "İndirilen dosya 3 denemeden sonra hâlâ eksik ($([math]::Round($downloadedSize/1KB, 1)) KB) - muhtemelen gerçek installer değil, bir hata sayfası inmiş olabilir veya bağlantı/antivirüs engelliyor."
        }

        Write-Ok "İndirme tamamlandı ($([math]::Round($downloadedSize/1MB, 1)) MB), kuruluyor..."
        $hostingProcess = Start-Process -FilePath $hostingBundlePath -ArgumentList "/quiet", "/install", "/norestart" -Wait -PassThru
        if ($hostingProcess.ExitCode -ne 0) {
            throw "Hosting Bundle kurulumu çıkış kodu $($hostingProcess.ExitCode) döndürdü."
        }
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
    # ÖNEMLİ: SSEI-Expr.exe (küçük "bootstrap" indirici) kendisi bir kurulum yapmaz, YALNIZCA
    # gerçek kurulum paketini indirir - /SECURITYMODE, /SAPWD gibi parametreleri kendisi
    # ANLAMAZ ve bunları gerçek kuruluma AKTARMAZ (önceki denemede tam bu yüzden sessizce hiçbir
    # şey kurmadan "For more information use /? or /Help." yazıp çıktı, script de bunu hata
    # saymadığı için [OK] diye yanlış rapor verdi). Doğru sıra: 1) bootstrap ile TAM paketi indir,
    # 2) o paketi bir klasöre çıkart, 3) çıkan GERÇEK SETUP.EXE'yi parametrelerle çalıştır -
    # parametreler ancak bu üçüncü adımda etkili olur.
    $sqlExpressUrl = "https://go.microsoft.com/fwlink/?linkid=866658"
    # Sabit dosya/klasör adları yerine her çalıştırmada benzersiz bir alt klasör kullanıyoruz -
    # önceki (başarısız/yarım kalmış) bir denemeden kalan kilitli dosya "başka bir işlem
    # tarafından kullanılıyor" hatasına yol açabiliyordu (tam bu şekilde yaşandı).
    # Çıkartma/kurulum adımı hâlâ HER ÇALIŞTIRMADA benzersiz bir klasör kullanıyor (kilitli
    # dosya sorununu önlemek için), ama İNDİRME kısmı (bootstrap + ~250MB tam paket) kalıcı
    # önbellekte tutuluyor - tekrar denemede yeniden inmiyor, bu kurulumun en yavaş kısmıydı.
    $sqlRunId = [Guid]::NewGuid().ToString("N").Substring(0, 8)
    $sqlWorkRoot = Join-Path $env:TEMP "SahinSoftSql_$sqlRunId"
    New-Item -Path $sqlWorkRoot -ItemType Directory -Force | Out-Null
    $sqlBootstrapPath = Join-Path $DownloadCacheDir "SQLEXPR-SSEI.exe"
    $sqlMediaPath = Join-Path $DownloadCacheDir "Media"
    $sqlExtractPath = Join-Path $sqlWorkRoot "Setup"
    try {
        $corePackage = Get-ChildItem -Path $sqlMediaPath -Filter "*.exe" -ErrorAction SilentlyContinue | Where-Object { $_.Length -ge 200MB } | Select-Object -First 1
        if ($corePackage) {
            Write-Ok "Tam SQL Server kurulum paketi önbellekte bulundu ($($corePackage.Name), $([math]::Round($corePackage.Length/1MB,0)) MB), tekrar indirilmiyor."
        }
        else {
            Invoke-WebRequest -Uri $sqlExpressUrl -OutFile $sqlBootstrapPath -UseBasicParsing
            Write-Ok "Bootstrap indirici indirildi, tam kurulum paketi indiriliyor..."

            New-Item -Path $sqlMediaPath -ItemType Directory -Force | Out-Null
            Start-Process -FilePath $sqlBootstrapPath -ArgumentList "/ACTION=Download", "/MEDIAPATH=$sqlMediaPath", "/MEDIATYPE=Core", "/QUIET" -Wait

            $corePackage = Get-ChildItem -Path $sqlMediaPath -Filter "*.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
            if (-not $corePackage) {
                throw "Tam kurulum paketi indirilemedi ($sqlMediaPath boş)."
            }
            Write-Ok "Tam kurulum paketi indirildi ($($corePackage.Name))."
        }

        Write-Ok "Çıkartılıyor..."
        New-Item -Path $sqlExtractPath -ItemType Directory -Force | Out-Null
        Start-Process -FilePath $corePackage.FullName -ArgumentList "/q", "/x:$sqlExtractPath" -Wait

        $setupExe = Join-Path $sqlExtractPath "SETUP.EXE"
        if (-not (Test-Path $setupExe)) {
            throw "Çıkartılan paketin içinde SETUP.EXE bulunamadı ($sqlExtractPath)."
        }
        Write-Ok "Paket çıkartıldı, sessiz kurulum başlatılıyor (Karma Kimlik Doğrulama, sa şifresi ayarlanıyor)..."

        # Not: /QUIET'in ilk denemede güvenilmez görünmesinin asıl sebebi bu değildi - sorun
        # küçük "bootstrap" indiricisinin (SSEI-Expr.exe) parametreleri gerçek kuruluma
        # AKTARMAMASIYDI (bkz. yukarıdaki indirme/çıkartma adımları - artık GERÇEK SETUP.EXE'yi
        # çalıştırıyoruz) ve bozuk TLS/indirme yüzünden bozuk dosya inmesiydi (yukarıda TLS 1.2
        # zorlandı). İkisi de düzeldiğine göre gerçek SETUP.EXE ile /QUIET güvenilir olmalı -
        # görünür kurulum ekranı (özellikle uzak masaüstü/TeamViewer altında) siyah/boş render
        # oluyordu, profesyonel görünmüyordu. /SECURITYMODE=SQL: Karma Kimlik Doğrulama (sa
        # hesabı aktif olur). /SAPWD: sa şifresi - appsettings.json bağlantı dizesi (5. adım)
        # aynı şifreyle sa kullanıcısına bağlanır.
        $setupProcess = Start-Process -FilePath $setupExe -ArgumentList "/ACTION=Install", "/IACCEPTSQLSERVERLICENSETERMS", "/QUIET", "/INSTANCENAME=MSSQLSERVER", "/SECURITYMODE=SQL", "/SAPWD=$SqlSaPassword" -Wait -PassThru
        if ($setupProcess.ExitCode -ne 0) {
            throw "SETUP.EXE çıkış kodu $($setupProcess.ExitCode) döndürdü - kurulum başarısız olmuş olabilir. Log: %ProgramFiles%\Microsoft SQL Server\1[0-9][0-9]\Setup Bootstrap\Log"
        }
        Write-Ok "SQL Server Express kurulumu tamamlandı (sa / $SqlSaPassword)."
    }
    catch {
        Write-Fail "SQL Server Express otomatik kurulamadı: $($_.Exception.Message)"
        Write-Warn "Lütfen SQL Server'ı elle kurun (SQL Server Express yeterlidir, Karma Kimlik Doğrulama + sa şifresi $SqlSaPassword ile), sonra bu betiği tekrar çalıştırın."
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
# 5) Bağlantı dizesini bu makinenin SQL Server'ına ayarla
# ---------------------------------------------------------------------------
Write-Step "appsettings.json bağlantı dizesi ayarlanıyor..."
$appSettingsPath = Join-Path $SitePath "appsettings.json"
if (Test-Path $appSettingsPath) {
    try {
        if ($RemoteSqlServer -ne "") {
            if ($SqlUser -ne "") {
                $newConnectionString = "Server=$RemoteSqlServer;Database=SahinSoftDb;User Id=$SqlUser;Password=$SqlPassword;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true"
            }
            else {
                $newConnectionString = "Server=$RemoteSqlServer;Database=SahinSoftDb;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true"
            }
            Write-Ok "Bağlantı dizesi belirtilen sunucuya ($RemoteSqlServer) ayarlanacak."
        }
        else {
            # sa / $SqlSaPassword - yukarıdaki 3. adımda SQL Server Express bu şifreyle kurulmuştu.
            $newConnectionString = "Server=localhost;Database=SahinSoftDb;User Id=sa;Password=$SqlSaPassword;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true"
            Write-Ok "Bağlantı dizesi bu makinedeki (localhost) SQL Server'a sa hesabıyla ayarlanacak."
        }

        $json = Get-Content -Path $appSettingsPath -Raw | ConvertFrom-Json
        if (-not $json.ConnectionStrings) {
            $json | Add-Member -MemberType NoteProperty -Name "ConnectionStrings" -Value ([PSCustomObject]@{})
        }
        $json.ConnectionStrings.DefaultConnection = $newConnectionString
        $json | ConvertTo-Json -Depth 10 | Set-Content -Path $appSettingsPath -Encoding UTF8
        Write-Ok "appsettings.json güncellendi."
    }
    catch {
        Write-Fail "appsettings.json güncellenirken hata: $($_.Exception.Message)"
        Write-Warn "Bağlantı dizesini SahinSoftVeritabaniAyarlari.exe (ConfigTool) ile elle ayarlayabilirsiniz."
    }
}
else {
    Write-Warn "appsettings.json bulunamadı ($appSettingsPath). Bu adım atlandı - uygulama dosyaları yayılmamış olabilir."
}

# ---------------------------------------------------------------------------
# 6) IIS Uygulama Havuzu ve Site
# ---------------------------------------------------------------------------
Write-Step "IIS Uygulama Havuzu ve Site kontrol ediliyor..."
try {
    # IIS özellikleri bu çalıştırmada YENİ etkinleştirilmişse (1. adım), WAS/W3SVC servisleri
    # ve IIS'in yönetim RPC/COM katmanı henüz tam hazır olmayabilir - "RPC sunucusu
    # kullanılamıyor (0x800706BA)" hatası tam bunu yaşadık. Devam etmeden önce servislerin
    # çalıştığından emin oluyoruz ve WebAdministration provider'ının hazır olması için kısa
    # bir bekleme + yeniden deneme uyguluyoruz.
    foreach ($serviceName in @("WAS", "W3SVC")) {
        $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
        if ($service -and $service.Status -ne "Running") {
            Start-Service -Name $serviceName -ErrorAction SilentlyContinue
        }
    }

    function Invoke-WithRetry([scriptblock]$Action, [string]$What) {
        $lastError = $null
        for ($attempt = 1; $attempt -le 5; $attempt++) {
            try {
                & $Action
                return
            }
            catch {
                $lastError = $_
                Write-Warn "$What başarısız (deneme $attempt/5): $($_.Exception.Message) - 3 sn sonra tekrar denenecek."
                Start-Sleep -Seconds 3
            }
        }
        throw $lastError
    }

    if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
        Invoke-WithRetry -What "Uygulama havuzu oluşturma" -Action {
            New-WebAppPool -Name $AppPoolName | Out-Null
            Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
        }
        Write-Ok "Uygulama havuzu '$AppPoolName' oluşturuldu (No Managed Code)."
    }
    else {
        Write-Ok "Uygulama havuzu '$AppPoolName' zaten mevcut."
    }

    if (-not (Test-Path "IIS:\Sites\$SiteName")) {
        Invoke-WithRetry -What "Site oluşturma" -Action {
            New-Website -Name $SiteName -PhysicalPath $SitePath -ApplicationPool $AppPoolName -Port $SitePort | Out-Null
        }
        Write-Ok "Site '$SiteName' oluşturuldu (port $SitePort)."
    }
    else {
        Write-Ok "Site '$SiteName' zaten mevcut, ayarlar korunuyor."
    }

    Invoke-WithRetry -What "Uygulama havuzunu başlatma" -Action { Start-WebAppPool -Name $AppPoolName }
    Invoke-WithRetry -What "Siteyi başlatma" -Action { Start-Website -Name $SiteName }
    Write-Ok "Uygulama havuzu ve site başlatıldı."
}
catch {
    Write-Fail "IIS site/havuz ayarlanırken hata: $($_.Exception.Message)"
    Write-Warn "Bu adımı IIS Yöneticisi (inetmgr) üzerinden elle yapmanız gerekebilir."
}

# ---------------------------------------------------------------------------
# 7) Veritabanı migration script'i
# ---------------------------------------------------------------------------
# Bu adım sqlcmd ile yapılıyordu - sqlcmd'nin PATH'te bulunamaması, QUOTED_IDENTIFIER ayarı,
# ve KENDİ çıktısının yakalanamaması (migration çalışırken pencerede "donmuş" gibi görünmesi)
# gibi art arda sorunlar çıkardı. SahinSoftDbKur.exe (aynı payload'ın içinde gömülü) bunların
# hepsini çözüyor - sqlcmd'ye hiç ihtiyaç duymadan doğrudan Microsoft.Data.SqlClient ile
# bağlanıp veritabanını temizleyip/kurup migration'ı uyguluyor, kanıtlanmış şekilde güvenilir.
Write-Step "Veritabanı migration script'i uygulanıyor..."

$dbKurExe = Join-Path $ScriptDir "SahinSoftDbKur.exe"
if (Test-Path $dbKurExe) {
    $migrationTargetServer = if ($RemoteSqlServer -ne "") { $RemoteSqlServer } else { "localhost" }
    $dbKurUser = if ($RemoteSqlServer -ne "" -and $SqlUser -ne "") { $SqlUser } else { "sa" }
    $dbKurPassword = if ($RemoteSqlServer -ne "" -and $SqlUser -ne "") { $SqlPassword } else { $SqlSaPassword }

    $dbKurProcess = Start-Process -FilePath $dbKurExe -ArgumentList $migrationTargetServer, $dbKurUser, $dbKurPassword -Wait -PassThru -WindowStyle Hidden -RedirectStandardOutput (Join-Path $ScriptDir "dbkur-cikti.log")
    if (Test-Path (Join-Path $ScriptDir "dbkur-cikti.log")) {
        Get-Content (Join-Path $ScriptDir "dbkur-cikti.log") -Encoding UTF8 | ForEach-Object { Write-Log "    $_" }
    }

    if ($dbKurProcess.ExitCode -eq 0) {
        Write-Ok "Veritabanı başarıyla kuruldu."
    }
    else {
        Write-Fail "Veritabanı kurulum aracı $($dbKurProcess.ExitCode) çıkış koduyla sona erdi - yukarıdaki loga bakın."
    }
}
else {
    Write-Warn "SahinSoftDbKur.exe bulunamadı ($dbKurExe). Bu adım atlandı."
}

# ---------------------------------------------------------------------------
# 8) IIS uygulama havuzunu yeniden başlat
# ---------------------------------------------------------------------------
# ÖNEMLİ: Uygulama havuzu/site 6. adımda, veritabanı henüz oluşturulmadan/migration
# uygulanmadan ÖNCE başlatılıyor. In-process hosting modeli w3wp içindeki .NET uygulamasını
# ilk HTTP isteğinde başlatıyor - eğer o ilk istek (Edip'in kendi testi, Windows'un/AV'nin
# arka plan taraması, IIS'in "Application Initialization" özelliği vb.) veritabanı henüz hazır
# değilken gelirse, w3wp süreci başarısız/bozuk bir durumda kalıp kendiliğinden düzelmiyor -
# tam olarak "kurulum bitti ama sayfa açılmadı, manuel yeniden başlatınca açıldı" şikayetinin
# sebebi bu. Çözüm: her şey (dosyalar + DB + migration) tamamlandıktan SONRA havuzu STOP/START
# ile tam sıfırlayıp Website'i yeniden başlatmak - böylece ilk gerçek istek her zaman hazır
# bir veritabanına düşer.
Write-Step "IIS uygulama havuzu veritabanı hazır olduktan sonra yeniden başlatılıyor..."
try {
    Stop-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Start-WebAppPool -Name $AppPoolName
    Start-Website -Name $SiteName -ErrorAction SilentlyContinue
    Write-Ok "Uygulama havuzu yeniden başlatıldı, sistem istek almaya hazır."
}
catch {
    Write-Warn "Uygulama havuzu yeniden başlatılırken hata: $($_.Exception.Message) - IIS Yöneticisi'nden '$AppPoolName' havuzunu elle Recycle edin."
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
# NOT: Burada eskiden "Read-Host 'Kapatmak için Enter'a basın'" vardı - bu script artık HER ZAMAN
# SahinSoft.exe (WinForms kurulum sihirbazı) tarafından, konsol girişi olmadan (RedirectStandardOutput,
# stdin yok) çalıştırılıyor. Read-Host orada sonsuza kadar bekliyordu, PowerShell süreci hiç
# kapanmıyordu, bu yüzden MainForm.cs'teki process.Exited olayı hiç tetiklenmiyor, "Kapat" butonu
# (başlangıçta Enabled=false) kalıcı olarak devre dışı kalıyordu - tıklanamayan/düz metin gibi
# görünen buton şikayetinin gerçek sebebi buydu.
