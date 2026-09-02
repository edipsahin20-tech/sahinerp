(function () {
    'use strict';

    var root = document.getElementById('pos-root');
    if (!root) return;

    var checkId = parseInt(root.getAttribute('data-check-id'), 10);
    var closeUrl = root.getAttribute('data-close-url');
    var payableTotal = parseFloat(root.getAttribute('data-payable-total')) || 0;
    var financialAccounts = JSON.parse(document.getElementById('pos-financial-accounts-data').textContent || '[]');

    var METHOD_LABELS = { 1: 'Nakit', 2: 'Kredi Kartı', 3: 'Yemek Kartı' };

    var openBtn = document.getElementById('open-close-payment-btn');
    if (!openBtn) return; // PayableTotal = 0, buton yok.

    var linesContainer = document.getElementById('close-payment-lines');
    var totalEl = document.getElementById('pay-total');
    var paidEl = document.getElementById('pay-paid');
    var remainingEl = document.getElementById('pay-remaining');
    var entryAmountEl = document.getElementById('pay-entry-amount');
    var partialBadge = document.getElementById('pay-partial-badge');
    var errorEl = document.getElementById('close-payment-error');
    var confirmBtn = document.getElementById('confirm-close-payment-btn');

    var paymentLines = [];
    var lineSeq = 0;
    var entryDigits = '';

    function getCsrfToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function money(v) {
        return v.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ₺';
    }

    function paidTotal() {
        return paymentLines.reduce(function (sum, l) { return sum + l.amount; }, 0);
    }

    function remaining() {
        return Math.round((payableTotal - paidTotal()) * 100) / 100;
    }

    function entryValue() {
        var normalized = entryDigits.replace(',', '.');
        return parseFloat(normalized) || 0;
    }

    function renderEntry() {
        entryAmountEl.textContent = entryDigits.length > 0 ? money(entryValue()) : money(0);
    }

    function setEntryFromNumber(n) {
        entryDigits = String(Math.round(n * 100) / 100).replace('.', ',');
        renderEntry();
    }

    function renderLines() {
        linesContainer.innerHTML = '';
        paymentLines.forEach(function (line) {
            var row = document.createElement('div');
            row.className = 'pay-line-row';
            var account = financialAccounts.find(function (a) { return a.financialAccountId === line.financialAccountId; });
            var label = document.createElement('span');
            label.innerHTML = '<b>' + METHOD_LABELS[line.method] + '</b>' +
                (account ? ' <span style="color:#8793a3">(' + account.name + ')</span>' : '');
            var amountWrap = document.createElement('span');
            amountWrap.style.display = 'flex';
            amountWrap.style.alignItems = 'center';
            amountWrap.style.gap = '10px';
            var amountStrong = document.createElement('b');
            amountStrong.textContent = money(line.amount);
            var removeBtn = document.createElement('button');
            removeBtn.type = 'button';
            removeBtn.textContent = '✕';
            removeBtn.addEventListener('click', function () {
                paymentLines = paymentLines.filter(function (x) { return x.lineId !== line.lineId; });
                refresh();
            });
            amountWrap.appendChild(amountStrong);
            amountWrap.appendChild(removeBtn);
            row.appendChild(label);
            row.appendChild(amountWrap);
            linesContainer.appendChild(row);
        });
        partialBadge.style.display = paymentLines.length > 0 ? '' : 'none';
    }

    function refresh() {
        renderLines();
        paidEl.textContent = money(paidTotal());
        var rem = remaining();
        remainingEl.textContent = money(Math.max(rem, 0));
        remainingEl.className = rem <= 0 ? 'text-success' : 'text-danger';
        confirmBtn.disabled = rem !== 0 || paymentLines.length === 0;
        if (entryDigits.length === 0 || entryValue() === 0) {
            setEntryFromNumber(Math.max(rem, 0));
        }
    }

    openBtn.addEventListener('click', function () {
        errorEl.style.display = 'none';
        totalEl.textContent = money(payableTotal);
        paymentLines = [];
        lineSeq = 0;
        entryDigits = '';
        setEntryFromNumber(payableTotal);
        refresh();
        new bootstrap.Modal(document.getElementById('closePaymentModal')).show();
    });

    document.querySelectorAll('.pay-numpad [data-num]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var digit = btn.getAttribute('data-num');
            if (digit === ',' && entryDigits.indexOf(',') !== -1) { return; }
            if (entryDigits === '0' && digit !== ',') { entryDigits = ''; }
            entryDigits += digit;
            renderEntry();
        });
    });
    document.getElementById('pay-num-clear').addEventListener('click', function () {
        entryDigits = '';
        renderEntry();
    });

    document.querySelectorAll('.pay-split-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var n = parseInt(btn.getAttribute('data-split'), 10);
            var rem = Math.max(remaining(), 0);
            setEntryFromNumber(Math.round((rem / n) * 100) / 100);
        });
    });

    document.querySelectorAll('.pay-cash-quick').forEach(function (btn) {
        btn.addEventListener('click', function () {
            setEntryFromNumber(parseFloat(btn.getAttribute('data-cash')));
        });
    });

    document.querySelectorAll('.pay-method-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var amount = entryValue();
            if (amount <= 0) { return; }
            paymentLines.push({
                lineId: ++lineSeq,
                method: parseInt(btn.getAttribute('data-method'), 10),
                financialAccountId: financialAccounts.length > 0 ? financialAccounts[0].financialAccountId : null,
                amount: amount
            });
            entryDigits = '';
            refresh();
        });
    });

    // Yazar kasa entegrasyonu (bkz. SettingsController Ayarlar > Stok Parametreleri) açıkken ve
    // satış fatura kesilmeden (walk-in, customerId=null - bu ekranda zaten hep null) kapatılıyorsa
    // önce fiziksel cihaza gönderilir; cihaz başarılı dönerse fiş/Z no'yla birlikte closeUrl'e
    // gidilir. Kapalıyken ya da fatura kesilen satışta (customerId varsa) hiç çağrılmaz - bugünkü
    // davranış aynen korunur.
    var fiscalEnabled = root.getAttribute('data-fiscal-enabled') === 'true';
    var fiscalAgentUrl = root.getAttribute('data-fiscal-agent-url');

    function sendToFiscalDevice(customerId, onDone, onError) {
        if (!fiscalEnabled || customerId) {
            onDone(null);
            return;
        }

        var lines = JSON.parse((document.getElementById('pos-fiscal-lines-data') || {}).textContent || '[]');
        var fiscalPayload = {
            cashierName: root.getAttribute('data-cashier-name') || '',
            referenceCheckNumber: root.getAttribute('data-check-id'),
            items: lines.map(function (l) {
                return { name: l.name, quantity: l.quantity, unitPrice: l.unitPrice, discountAmount: l.discountAmount, section: 1 };
            }),
            payments: paymentLines.map(function (l) {
                return { method: l.method, amount: l.amount };
            })
        };

        fetch(fiscalAgentUrl.replace(/\/$/, '') + '/sale/process', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(fiscalPayload)
        })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (!data.success) {
                    onError('Yazar kasa: ' + (data.errorMessage || 'Satış cihaza gönderilemedi.'));
                    return;
                }
                onDone({
                    fiscalReceiptNumber: data.receiptNo != null ? String(data.receiptNo) : null,
                    fiscalZNo: data.zNo != null ? String(data.zNo) : null,
                    fiscalDeviceSerialNumber: data.deviceSerialNumber || null
                });
            })
            .catch(function () {
                onError('Yazar kasaya bağlanılamadı (' + fiscalAgentUrl + '). Cihazın/agent\'ın çalıştığından emin olun.');
            });
    }

    confirmBtn.addEventListener('click', function () {
        confirmBtn.disabled = true;
        confirmBtn.textContent = 'Kapatılıyor...';
        errorEl.style.display = 'none';

        var customerId = null;

        sendToFiscalDevice(customerId, function (fiscal) {
            var payload = {
                checkId: checkId,
                submissionKey: document.getElementById('pos-close-submission-key').value,
                customerId: customerId,
                payments: paymentLines.map(function (l) {
                    return { method: l.method, financialAccountId: l.financialAccountId, amount: l.amount };
                }),
                fiscalReceiptNumber: fiscal ? fiscal.fiscalReceiptNumber : null,
                fiscalZNo: fiscal ? fiscal.fiscalZNo : null,
                fiscalDeviceSerialNumber: fiscal ? fiscal.fiscalDeviceSerialNumber : null
            };

            fetch(closeUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': getCsrfToken() },
                body: JSON.stringify(payload)
            })
                .then(function (res) { return res.json().then(function (data) { return { ok: res.ok, data: data }; }); })
                .then(function (result) {
                    if (!result.ok) {
                        errorEl.textContent = result.data.error || 'Adisyon kapatılamadı.';
                        errorEl.style.display = 'block';
                        confirmBtn.disabled = false;
                        confirmBtn.textContent = 'Kapat / Öde';
                        return;
                    }
                    window.location.href = root.getAttribute('data-back-url');
                })
                .catch(function () {
                    errorEl.textContent = 'Bağlantı hatası oluştu.';
                    errorEl.style.display = 'block';
                    confirmBtn.disabled = false;
                    confirmBtn.textContent = 'Kapat / Öde';
                });
        }, function (fiscalErrorMessage) {
            errorEl.textContent = fiscalErrorMessage;
            errorEl.style.display = 'block';
            confirmBtn.disabled = false;
            confirmBtn.textContent = 'Kapat / Öde';
        });
    });
})();
