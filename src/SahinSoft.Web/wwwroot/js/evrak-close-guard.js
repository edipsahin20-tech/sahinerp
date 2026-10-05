// Kırmızı "Kapat" butonu - dirty-check (Edip, 2026-09-28: "ekranda hiç bir veri yoksa sistemden
// çıksın yada ekrana bir veri getirdi değişilik yapmadı kapat çalıssın ama ekrana veriyi getirdi
// bi değişilik yaptı ama kaydet demeden kapat dedi önce o zaman uyarı versin önce kayıt edin diye
// eğer yaptıgı değişikliği kayıt ettikten sonra kapat butonu aktif olsun").
//
// _EvrakToolbar.cshtml TEK partial olduğu için bu script tüm Form.cshtml ekranlarında (Cari,
// Ürün, Fatura, vb. - #evrak-form + #evrak-close-btn ikilisi neredeyse varsa) otomatik çalışır.
// Kaydetme normal MVC post-redirect akışıyla zaten YENİ bir sayfa yüklemesi olduğundan (Sakla ->
// sunucu tarafı redirect), kayıttan sonra "temiz" durum otomatik oluşur - ayrı bir "kaydedildi"
// bayrağı TUTULMASINA gerek yok.
(function () {
    'use strict';

    // Evet/Hayır onay penceresi (tarayıcı confirm() yerine; ŞahinSoft görünümünde).
    function evrakConfirm(message, onYes) {
        var overlay = document.createElement('div');
        overlay.style.cssText = 'position:fixed;inset:0;background:rgba(11,21,36,.55);z-index:99999;display:flex;align-items:center;justify-content:center;';
        overlay.innerHTML = '<div style="background:#fff;border-radius:16px;padding:24px 26px;width:min(420px,92vw);box-shadow:0 20px 60px rgba(0,0,0,.35);font-family:Inter,\'Segoe UI\',system-ui,sans-serif;">' +
            '<div style="font-weight:800;font-size:15px;color:#132238;margin-bottom:8px;">Onay</div>' +
            '<div class="evrak-confirm-msg" style="font-size:14px;color:#334155;line-height:1.5;margin-bottom:18px;"></div>' +
            '<div style="display:flex;gap:10px;justify-content:flex-end;">' +
            '<button type="button" data-no style="padding:9px 18px;border:1px solid #cbd5e1;border-radius:10px;background:#fff;font-weight:700;cursor:pointer;">Hayır</button>' +
            '<button type="button" data-yes style="padding:9px 18px;border:0;border-radius:10px;background:linear-gradient(90deg,#f0cf73,#e9bd46);font-weight:800;cursor:pointer;">Evet</button>' +
            '</div></div>';
        overlay.querySelector('.evrak-confirm-msg').textContent = message;
        function close() { overlay.remove(); }
        overlay.querySelector('[data-no]').addEventListener('click', close);
        overlay.querySelector('[data-yes]').addEventListener('click', function () { close(); onYes(); });
        overlay.addEventListener('click', function (e) { if (e.target === overlay) { close(); } });
        document.body.appendChild(overlay);
        overlay.querySelector('[data-no]').focus();
    }
    window.evrakConfirm = evrakConfirm;

    // Kalıcı "Sil" formu: Evet/Hayır onayından sonra gönderilir.
    document.addEventListener('submit', function (e) {
        var f = e.target;
        if (f && f.getAttribute && f.getAttribute('data-evrak-confirm') && !f.__confirmed) {
            e.preventDefault();
            evrakConfirm(f.getAttribute('data-evrak-confirm'), function () { f.__confirmed = true; f.submit(); });
        }
    }, true);

    function serializeForm(form) {
        var entries = [];
        new FormData(form).forEach(function (value, key) {
            entries.push(key + '=' + value);
        });
        entries.sort();
        return entries.join('&');
    }

    document.addEventListener('DOMContentLoaded', function () {
        var form = document.getElementById('evrak-form');
        var closeBtn = document.getElementById('evrak-close-btn');
        if (!form || !closeBtn) {
            return;
        }

        var baseline = serializeForm(form);
        var isDirty = function () {
            return serializeForm(form) !== baseline;
        };

        var refreshDirtyState = function () {
            closeBtn.classList.toggle('is-dirty', isDirty());
        };
        form.addEventListener('input', refreshDirtyState);
        form.addEventListener('change', refreshDirtyState);

        var goClose = function () {
            var closeUrl = closeBtn.getAttribute('data-close-url');
            window.location.href = closeUrl || document.referrer || '/';
        };
        closeBtn.addEventListener('click', function () {
            if (isDirty()) {
                evrakConfirm('Ekranda kaydedilmemiş işlem var. Kaydetmeden çıkılsın mı?', goClose);
                return;
            }
            goClose();
        });

        // Vazgeç: onaydan sonra ekran temizlenir (sayfa yeniden yüklenir), hiçbir şey kaydedilmez.
        var cancelBtn = document.getElementById('evrak-cancel-btn');
        if (cancelBtn) {
            cancelBtn.addEventListener('click', function () {
                // Ekranda hiçbir değişiklik yoksa (ör. eski evrağı Düzenle ile açıp dokunmadan Vazgeç) ekran doğrudan kapanır.
                if (!isDirty()) {
                    goClose();
                    return;
                }
                evrakConfirm(cancelBtn.getAttribute('data-cancel-confirm') || 'İşlem iptal edilsin mi?', function () {
                    window.location.assign(window.location.pathname + window.location.search);
                });
            });
        }
    });
})();
