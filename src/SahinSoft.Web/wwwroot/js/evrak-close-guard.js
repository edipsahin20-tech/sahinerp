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

        closeBtn.addEventListener('click', function () {
            if (isDirty()) {
                alert('Kaydedilmemiş değişiklikleriniz var. Lütfen önce "Sakla" ile kaydedin.');
                return;
            }
            var closeUrl = closeBtn.getAttribute('data-close-url');
            window.location.href = closeUrl || document.referrer || '/';
        });
    });
})();
