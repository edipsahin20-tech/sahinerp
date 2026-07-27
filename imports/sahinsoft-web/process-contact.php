<?php
/**
 * ŞahinSoft İletişim Formu - Sunucu Taraflı Gönderim Script'i
 * ---------------------------------------------------------
 * Bu dosya iletisim.html'deki formdan gelen POST verisini alır,
 * doğrular ve edip@sahinbilisim.com.tr adresine e-posta olarak gönderir.
 *
 * GEREKSİNİM: Hosting'inizde PHP çalışıyor olmalı (çoğu paylaşımlı
 * hosting / cPanel bunu destekler). Statik dosya barındıran servisler
 * (örn. sadece HTML/CSS/JS sunan bazı ücretsiz hosting'ler) bu dosyayı
 * ÇALIŞTIRAMAZ, PHP desteği olan bir hosting gerekir.
 */

header('Content-Type: application/json; charset=utf-8');

// Sadece POST isteklerine izin ver
if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode(['success' => false, 'message' => 'Geçersiz istek yöntemi.']);
    exit;
}

// --- Basit spam koruması (honeypot) ---
// Formda görünmeyen "website" adlı bir alan var; botlar genelde bunu da doldurur.
if (!empty($_POST['website'])) {
    // Bot olduğunu düşün, sessizce başarılı gibi davran (botu bilgilendirme)
    echo json_encode(['success' => true, 'message' => 'Mesajınız alındı.']);
    exit;
}

// --- CSRF koruması (double-submit cookie yöntemi) ---
// iletisim.html sayfası PHP ile işlenmediği için (statik HTML), oturum tabanlı
// klasik CSRF token yerine bu yöntem kullanılıyor: sayfa açılırken JS bir rastgele
// token üretip hem çerez hem gizli form alanına yazıyor; burada ikisi karşılaştırılıyor.
// Başka bir siteden (CSRF saldırısı) gelen istek bu çerezi taşıyamaz.
$csrfCookie = $_COOKIE['csrf_token'] ?? '';
$csrfPost   = $_POST['csrf_token'] ?? '';
if ($csrfCookie === '' || $csrfPost === '' || !hash_equals($csrfCookie, $csrfPost)) {
    http_response_code(403);
    echo json_encode(['success' => false, 'message' => 'Güvenlik doğrulaması başarısız oldu. Lütfen sayfayı yenileyip tekrar deneyin.']);
    exit;
}

// --- IP bazlı hız sınırlama (spam/kötüye kullanım önleme) ---
// Aynı IP'den kısa sürede çok fazla gönderim varsa reddedilir.
// Dosya tabanlı basit bir sayaç kullanılır (veritabanı gerektirmez).
function check_rate_limit($ip, $maxRequests = 5, $windowSeconds = 600) {
    $rateDir = __DIR__ . '/data';
    if (!is_dir($rateDir)) {
        @mkdir($rateDir, 0755, true);
    }
    $safeIp = preg_replace('/[^a-zA-Z0-9_.:]/', '_', $ip);
    $file = $rateDir . '/ratelimit_' . $safeIp . '.json';

    $now = time();
    $timestamps = [];

    $fp = @fopen($file, 'c+');
    if (!$fp) {
        // Dosya sistemi yazılabilir değilse hız sınırlamasını atla (formu engelleme)
        return true;
    }
    flock($fp, LOCK_EX);
    $contents = stream_get_contents($fp);
    if ($contents) {
        $decoded = json_decode($contents, true);
        if (is_array($decoded)) {
            $timestamps = $decoded;
        }
    }
    // Pencere dışındaki eski kayıtları temizle
    $timestamps = array_values(array_filter($timestamps, function ($t) use ($now, $windowSeconds) {
        return ($now - $t) < $windowSeconds;
    }));

    $allowed = count($timestamps) < $maxRequests;
    if ($allowed) {
        $timestamps[] = $now;
    }

    ftruncate($fp, 0);
    rewind($fp);
    fwrite($fp, json_encode($timestamps));
    fflush($fp);
    flock($fp, LOCK_UN);
    fclose($fp);

    return $allowed;
}

$clientIp = $_SERVER['REMOTE_ADDR'] ?? 'unknown';
if (!check_rate_limit($clientIp, 5, 600)) {
    http_response_code(429);
    echo json_encode([
        'success' => false,
        'message' => 'Çok fazla deneme yaptınız. Lütfen birkaç dakika sonra tekrar deneyin ya da bizi doğrudan arayın: 0533 278 23 79'
    ]);
    exit;
}

function clean($value) {
    $value = trim($value ?? '');
    $value = strip_tags($value);
    return htmlspecialchars($value, ENT_QUOTES, 'UTF-8');
}

// Mail başlıklarına (Subject/From/Reply-To) enjeksiyonu önlemek için:
// bu alanlarda satır sonu (\r, \n) veya başlık enjeksiyonunda kullanılan
// "Bcc:"/"Content-Type:" gibi dizeler asla kabul edilmez - tek satıra indirilir.
function sanitize_header_field($value) {
    $value = str_replace(["\r", "\n", "%0a", "%0d", "%0A", "%0D"], '', $value);
    return trim($value);
}

$name    = mb_substr(sanitize_header_field(clean($_POST['name'] ?? '')), 0, 100);
$phone   = mb_substr(sanitize_header_field(clean($_POST['phone'] ?? '')), 0, 30);
$email   = mb_substr(sanitize_header_field(clean($_POST['email'] ?? '')), 0, 150);
$sector  = mb_substr(sanitize_header_field(clean($_POST['sector'] ?? '')), 0, 100);
$subject = mb_substr(sanitize_header_field(clean($_POST['subject'] ?? '')), 0, 150) ?: 'Web sitesi iletişim formu';
$message = mb_substr(clean($_POST['message'] ?? ''), 0, 5000); // mesaj gövdesi, çok satırlı olabilir - başlık değil

// --- Zorunlu alan kontrolü ---
$errors = [];
if ($name === '')    $errors[] = 'Ad Soyad';
if ($phone === '')   $errors[] = 'Telefon';
if ($message === '') $errors[] = 'Mesaj';

if (!empty($errors)) {
    http_response_code(400);
    echo json_encode([
        'success' => false,
        'message' => 'Lütfen şu alanları doldurun: ' . implode(', ', $errors)
    ]);
    exit;
}

// --- E-posta adresi formatı geçerliyse doğrula (boş bırakılabilir) ---
if ($email !== '' && !filter_var($email, FILTER_VALIDATE_EMAIL)) {
    http_response_code(400);
    echo json_encode(['success' => false, 'message' => 'Geçerli bir e-posta adresi girin.']);
    exit;
}

// --- Alıcı ve gönderim ayarları ---
$to = 'edip@sahinbilisim.com.tr';
$mail_subject = '[Web Formu] ' . $subject;

$body  = "Yeni bir iletişim formu mesajı alındı:\n\n";
$body .= "Ad Soyad     : $name\n";
$body .= "Telefon      : $phone\n";
$body .= "E-posta      : " . ($email ?: '-') . "\n";
$body .= "İlgi Alanı   : " . ($sector ?: '-') . "\n";
$body .= "Konu         : $subject\n";
$body .= "Gönderim IP  : " . ($_SERVER['REMOTE_ADDR'] ?? '-') . "\n";
$body .= "Tarih        : " . date('d.m.Y H:i:s') . "\n";
$body .= "\nMesaj:\n$message\n";

// Not: "From" alanı olarak sizin kendi domain adresinizi kullanmak,
// mail sunucularının bu e-postayı spam'e atmasını engellemeye yardımcı olur.
// Müşterinin e-postasına doğrudan "From" olarak yazmak (özellikle Gmail/Yahoo
// hedeflerinde) SPF/DKIM uyumsuzluğu yüzünden spam'e düşme riskini artırır.
$headers  = "From: ŞahinSoft Web Sitesi <edip@sahinbilisim.com.tr>\r\n";
if ($email !== '') {
    $headers .= "Reply-To: $email\r\n";
}
$headers .= "MIME-Version: 1.0\r\n";
$headers .= "Content-Type: text/plain; charset=UTF-8\r\n";

$sent = @mail($to, $mail_subject, $body, $headers);

if ($sent) {
    echo json_encode(['success' => true, 'message' => 'Mesajınız başarıyla gönderildi. En kısa sürede sizinle iletişime geçeceğiz.']);
} else {
    http_response_code(500);
    echo json_encode([
        'success' => false,
        'message' => 'Mesajınız gönderilirken bir sorun oluştu. Lütfen bizi doğrudan arayın: 0533 278 23 79'
    ]);
}
