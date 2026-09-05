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

    function close() {
        okBtn.onclick = null;
        cancelBtn.onclick = null;
        inputEl.onkeydown = null;
        modal.hide();
    }

    window.posAlert = function (message, onClose) {
        open({ message: message, showCancel: false });
        okBtn.onclick = function () { close(); if (onClose) onClose(); };
    };

    window.posConfirm = function (message, onConfirm, onCancel, opts) {
        open({ message: message, okText: (opts && opts.okText) || 'Evet', danger: opts && opts.danger });
        okBtn.onclick = function () { close(); if (onConfirm) onConfirm(); };
        cancelBtn.onclick = function () { close(); if (onCancel) onCancel(); };
    };

    window.posPrompt = function (message, defaultValue, onSubmit, onCancel) {
        open({ message: message, defaultValue: defaultValue, isPrompt: true });
        function submit() { var v = inputEl.value; close(); if (onSubmit) onSubmit(v); }
        okBtn.onclick = submit;
        inputEl.onkeydown = function (e) { if (e.key === 'Enter') { e.preventDefault(); submit(); } };
        cancelBtn.onclick = function () { close(); if (onCancel) onCancel(); };
    };
})();
