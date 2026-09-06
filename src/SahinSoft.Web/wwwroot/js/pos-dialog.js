// ŞahinSoft masaüstü-hissi onay/uyarı/giriş penceresi (Edip, 2026-09-05: "web tarayıcı gibi
// onay istemesin, masaüstü program gibi küçük onay alanı açsın") - tarayıcının native
// window.confirm/alert/prompt'u yerine AYNI ŞahinSoft görsel diliyle (Bootstrap modal) bir
// pencere gösterir. window.posAlert/posConfirm/posPrompt olarak dışa açılır, tüm restoran POS
// dosyaları bunları kullanır - native window.confirm/alert/prompt'a bir daha DÖNÜLMEZ.
(function () {
    'use strict';

    var modalEl = document.getElementById('posDialogModal');
    if (!modalEl) return;

    var modal = new bootstrap.Modal(modalEl, { backdrop: 'static', keyboard: false });
    var messageEl = document.getElementById('posDialogMessage');
    var inputEl = document.getElementById('posDialogInput');
    var cancelBtn = document.getElementById('posDialogCancelBtn');
    var okBtn = document.getElementById('posDialogOkBtn');

    function open(opts) {
        messageEl.textContent = opts.message || '';
        inputEl.style.display = opts.isPrompt ? '' : 'none';
        inputEl.value = opts.defaultValue == null ? '' : opts.defaultValue;
        cancelBtn.style.display = opts.showCancel === false ? 'none' : '';
        okBtn.textContent = opts.okText || 'Tamam';
        cancelBtn.textContent = opts.cancelText || 'Vazgeç';
        okBtn.className = 'btn ' + (opts.danger ? 'btn-danger' : 'btn-erp-primary');
        modal.show();
        if (opts.isPrompt) {
            setTimeout(function () { inputEl.focus(); inputEl.select(); }, 250);
        }
    }

    // GERÇEK HATA (2026-09-05, Edip'in bildirimiyle bulundu: "TÜM SİPARİŞİ SİL DEDİĞİMDE
    // SİLMİYOR") - bir dialog'un onConfirm/onSubmit'i İÇİNDEN hemen bir YENİ dialog açıldığında
    // (ör. "Sipariş Sil" onayından sonra gerekçe sorulması) modal.hide() henüz bitmeden AYNI
    // modal örneğinde modal.show() çağrılıyordu - Bootstrap'ın hide geçişi sonradan tamamlanıp
    // modalı GÖRÜNMEZ bırakıyordu, içerik (mesaj/girdi) güncellenmiş olsa bile. Artık close()
    // asıl callback'i modal TAMAMEN kapandıktan SONRA (hidden.bs.modal) çalıştırıyor - zincirleme
    // bir sonraki open() her zaman temiz, tam kapanmış bir modal üzerinde çalışır.
    function close(then) {
        okBtn.onclick = null;
        cancelBtn.onclick = null;
        inputEl.onkeydown = null;
        if (then) {
            // GERÇEK HATA (2026-09-06, kapsamlı kabul testinde bulundu) - dokunmatik ekranda
            // hızlı/üst üste dokunma (ör. "Evet"e art arda iki kez basmak) modal henüz AÇILMA
            // geçişini bitirmeden close()'u tetikleyebiliyordu; Bootstrap'ın hide() metodu bu
            // durumda kendi iç "_isTransitioning" korumasıyla SESSİZCE hiçbir şey yapmıyor,
            // "hidden.bs.modal" da hiç ateşlenmiyor - callback (ör. Sipariş Sil'in gerçek
            // gönderimi) SONSUZA DEK bekliyor, modal görünürde açık kalıyor ve okBtn.onclick zaten
            // null olduğundan tekrar tıklamak da işe yaramıyordu. Güvenlik ağı: "hidden.bs.modal"
            // makul bir sürede gelmezse callback yine de çalıştırılır - aynı callback'in iki kez
            // çalışmaması "fired" bayrağıyla garanti edilir.
            var fired = false;
            function runOnce() {
                if (fired) return;
                fired = true;
                then();
            }
            modalEl.addEventListener('hidden.bs.modal', function handler() {
                modalEl.removeEventListener('hidden.bs.modal', handler);
                runOnce();
            });
            setTimeout(function () {
                runOnce();
                // Bootstrap'ın kendi hide() çağrısı sessizce hiçbir şey yapmadıysa modal
                // görünürde hâlâ açık kalır - callback yukarıda zaten çalıştı, ama kullanıcı
                // kafası karışmasın diye modalı burada da kapatmayı deneriz. Önce normal hide()
                // (bu noktada geçiş durumu kesinlikle bitmiştir, normal çalışması beklenir);
                // o da sessiz kalırsa (gözlemlenen bir durum - iç Bootstrap state'i beklenenden
                // farklı kalabiliyor) son çare olarak DOM'u doğrudan kapalı hale getiririz.
                if (modalEl.classList.contains('show')) {
                    modal.hide();
                    setTimeout(function () {
                        if (modalEl.classList.contains('show')) {
                            modalEl.classList.remove('show');
                            modalEl.style.display = 'none';
                            modalEl.setAttribute('aria-hidden', 'true');
                            modalEl.removeAttribute('aria-modal');
                            modalEl.removeAttribute('role');
                            document.body.classList.remove('modal-open');
                            document.body.style.removeProperty('overflow');
                            document.body.style.removeProperty('padding-right');
                            document.querySelectorAll('.modal-backdrop').forEach(function (el) { el.remove(); });
                        }
                    }, 200);
                }
            }, 500);
        }
        modal.hide();
    }

    window.posAlert = function (message, onClose) {
        open({ message: message, showCancel: false });
        okBtn.onclick = function () { close(onClose); };
    };

    window.posConfirm = function (message, onConfirm, onCancel, opts) {
        open({ message: message, okText: (opts && opts.okText) || 'Evet', danger: opts && opts.danger });
        okBtn.onclick = function () { close(onConfirm); };
        cancelBtn.onclick = function () { close(onCancel); };
    };

    window.posPrompt = function (message, defaultValue, onSubmit, onCancel) {
        open({ message: message, defaultValue: defaultValue, isPrompt: true });
        function submit() { var v = inputEl.value; close(function () { if (onSubmit) onSubmit(v); }); }
        okBtn.onclick = submit;
        inputEl.onkeydown = function (e) { if (e.key === 'Enter') { e.preventDefault(); submit(); } };
        cancelBtn.onclick = function () { close(onCancel); };
    };
})();
