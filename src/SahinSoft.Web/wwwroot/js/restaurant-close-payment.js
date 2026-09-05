(function () {
    'use strict';

    var root = document.getElementById('pos-root');
    if (!root) return;

    var checkId = parseInt(root.getAttribute('data-check-id'), 10);
    var closeUrl = root.getAttribute('data-close-url');
    var payableTotal = parseFloat(root.getAttribute('data-payable-total')) || 0;
    var financialAccounts = JSON.parse(document.getElementById('pos-financial-accounts-data').textContent || '[]');
    // Kasa Tanımları (madde 27, 2026-09-05) - kasiyerin şubesine bağlı kasadan çözülen, ödeme
    // yöntemine göre GERÇEK hedef hesap. Öncesinde HER ödeme yöntemi (Nakit/Kredi Kartı/Yemek
    // Çeki) sabit olarak financialAccounts[0]'a (alfabetik ilk hesap) gidiyordu - hangi şubenin
    // hangi kasası kullanıldığı hiç izlenmiyor, Nakit ve Kredi Kartı aynı hesaba karışıyordu.
    var cashRegisterData = JSON.parse(document.getElementById('pos-cash-register-data') ? document.getElementById('pos-cash-register-data').textContent || '{}' : '{}');
    function resolveFinancialAccountId(method) {
        var byRegister = method === 1 ? cashRegisterData.cash
            : method === 2 ? cashRegisterData.creditCard
            : method === 3 ? (cashRegisterData.mealCard || cashRegisterData.creditCard)
            : null;
        if (byRegister) { return byRegister; }
        // Kasa tanımlı değilse (henüz yapılandırılmamış şube) eski davranışa geri düş.
        return financialAccounts.length > 0 ? financialAccounts[0].financialAccountId : null;
    }

    // Kısmi ödeme (madde 4-8) - sunucuda kalıcı, muhasebeye HİÇ dokunmayan RestaurantCheckPendingPayment
    // kayıtları (bkz. RestaurantPostingService). Adisyona Dön modalı kapatır ama bu diziyi
    // SIFIRLAMAZ - sayfa yeniden yüklense bile sunucudan aynen geri gelir.
    var addPendingPaymentUrl = root.getAttribute('data-add-pending-payment-url');
    var removePendingPaymentUrl = root.getAttribute('data-remove-pending-payment-url');
    var cancelPendingPaymentsUrl = root.getAttribute('data-cancel-pending-payments-url');
    var receiptUrlTemplate = root.getAttribute('data-receipt-url-template');
    var requireReceiptPromptAfterQuickPay = root.getAttribute('data-require-receipt-prompt-after-quickpay') === 'true';
    var selfSaleUrl = root.getAttribute('data-self-sale-url');
    var mainPaidTotalEl = document.getElementById('main-paid-total');
    var mainRemainingTotalEl = document.getElementById('main-remaining-total');
    var payChangeWrap = document.getElementById('pay-change-wrap');
    var payChangeEl = document.getElementById('pay-change');

    var METHOD_LABELS = { 1: 'Nakit', 2: 'Kredi Kartı', 3: 'Yemek Çeki', 4: 'Ödenmez', 5: 'Açık Hesap' };
    var OPEN_ACCOUNT_METHOD = 5;

    // Açık Hesap (madde 13) - Cari Ekle (restaurant-pos.js) ile bağlanmış müşteri ZORUNLUDUR.
    function attachedCustomerId() {
        var input = document.getElementById('AttachedCustomerId');
        var v = input && input.value ? parseInt(input.value, 10) : NaN;
        return isNaN(v) ? null : v;
    }

    // Self satış ve masa satışta AYNI tek tetikleyici - "Kapat/Öde" ayrı bir buton olarak
    // kaldırıldı (Edip, 2026-09-03: "kapat ve ödeme mantığı kalksın"), her ikisi de sepetin
    // altındaki "Ödemeyi Al"ı kullanır (bkz. Check.cshtml #self-pay-btn).
    var openBtn = document.getElementById('self-pay-btn');
    if (!openBtn) return; // PayableTotal = 0, buton disabled ama DOM'da var - yine de devam eder.

    var linesContainer = document.getElementById('close-payment-lines');
    var totalEl = document.getElementById('pay-total');
    var paidEl = document.getElementById('pay-paid');
    var remainingEl = document.getElementById('pay-remaining');
    var entryAmountEl = document.getElementById('pay-entry-amount');
    var partialBadge = document.getElementById('pay-partial-badge');
    var errorEl = document.getElementById('close-payment-error');
    var confirmBtn = document.getElementById('confirm-close-payment-btn');

    var lineSeq = 0;
    var entryDigits = '';
    var initialPending = JSON.parse((document.getElementById('pos-pending-payments-data') || {}).textContent || '[]');
    var paymentLines = initialPending.map(function (p) {
        return {
            lineId: ++lineSeq,
            pendingPaymentId: p.pendingPaymentId,
            method: p.method,
            financialAccountId: p.financialAccountId,
            amount: p.amount
        };
    });

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

    // Para Üstü (madde 4) - kasiyerin girdiği tutar kalan bakiyeyi aşarsa fark burada gösterilir.
    // Sadece BİLGİLENDİRME amaçlıdır: bir ödeme yöntemi tuşuna basıldığında kayda giren tutar
    // kalanla SINIRLANIR (bkz. pay-method-btn handler) - CloseCheckAsync'in "ödeme toplamı ==
    // adisyon tutarı" katı eşitlik kontrolü BOZULMAZ, gerçek fazla tahsilat asla kaydedilmez.
    function renderEntry() {
        entryAmountEl.textContent = entryDigits.length > 0 ? money(entryValue()) : money(0);
        var rem = Math.max(remaining(), 0);
        var typed = entryValue();
        if (rem > 0 && typed > rem) {
            payChangeWrap.style.display = '';
            payChangeEl.textContent = money(typed - rem);
        } else {
            payChangeWrap.style.display = 'none';
        }
    }

    function setEntryFromNumber(n) {
        entryDigits = String(Math.round(n * 100) / 100).replace('.', ',');
        renderEntry();
    }

    // Ödenen/Kalan (madde 2, 2026-09-05 teknik doküman) - alan HER ZAMAN görünür, ödeme
    // yapılmamışsa bile Ödenen 0,00/Kalan Toplam kadar gösterilir.
    function updateMainScreenSummary() {
        var paid = paidTotal();
        if (mainPaidTotalEl) mainPaidTotalEl.textContent = money(paid);
        if (mainRemainingTotalEl) mainRemainingTotalEl.textContent = money(Math.max(remaining(), 0));
    }

    function removePendingPaymentLine(line) {
        var afterRemoval = function () {
            paymentLines = paymentLines.filter(function (x) { return x.lineId !== line.lineId; });
            refresh();
        };
        if (!line.pendingPaymentId) {
            // Sunucuya henüz hiç yazılmadı (yarış durumu) - sadece yerelden kaldır.
            afterRemoval();
            return;
        }
        fetch(removePendingPaymentUrl + '?pendingPaymentId=' + line.pendingPaymentId, {
            method: 'POST',
            headers: { 'X-CSRF-TOKEN': getCsrfToken() }
        }).then(afterRemoval).catch(afterRemoval);
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
            removeBtn.addEventListener('click', function () { removePendingPaymentLine(line); });
            amountWrap.appendChild(amountStrong);
            amountWrap.appendChild(removeBtn);
            row.appendChild(label);
            row.appendChild(amountWrap);
            linesContainer.appendChild(row);
        });
        partialBadge.style.display = paymentLines.length > 0 ? '' : 'none';
    }

    // skipAutoFill: pencere YENİ açıldığında (openPaymentModal) Alınacak Tutar bilinçli olarak
    // 0'da bırakılır (2026-09-05 teknik doküman madde 3) - bu fonksiyonun normalde boş/sıfır
    // girişi kalanla YENİDEN doldurma davranışı (ör. bir ödeme satırı silindiğinde) o durumda
    // devre dışı bırakılır.
    function refresh(skipAutoFill) {
        renderLines();
        paidEl.textContent = money(paidTotal());
        var rem = remaining();
        remainingEl.textContent = money(Math.max(rem, 0));
        remainingEl.className = rem <= 0 ? 'text-success' : 'text-danger';
        confirmBtn.disabled = rem !== 0 || paymentLines.length === 0;
        if (!skipAutoFill && (entryDigits.length === 0 || entryValue() === 0)) {
            setEntryFromNumber(Math.max(rem, 0));
        } else {
            renderEntry();
        }
        updateMainScreenSummary();
    }

    // Sayfa ilk yüklendiğinde zaten kısmi ödemesi olan bir adisyon açılmışsa (madde 8: "ana
    // ekranda da görünür") - modal hiç açılmadan bile Ödenen/Kalan görünsün.
    updateMainScreenSummary();

    function openPaymentModal() {
        errorEl.style.display = 'none';
        totalEl.textContent = money(payableTotal);
        // paymentLines KASITLI OLARAK sıfırlanmıyor (madde 8: "Adisyona Dön kısmi ödemeleri
        // korur") - sayfa ilk yüklendiğinde sunucudan gelen haliyle veya bu oturumda eklenmiş
        // haliyle aynen devam eder. Alınacak Tutar ise (2026-09-05 teknik doküman madde 3)
        // pencere ilk açıldığında HER ZAMAN 0,00 ₺'den başlar - toplam/kalanı otomatik doldurmaz,
        // kullanıcı tuşlar ya da bölme butonlarını kullanır.
        entryDigits = '';
        setEntryFromNumber(0);
        refresh(true);
        new bootstrap.Modal(document.getElementById('closePaymentModal')).show();
    }

    openBtn.addEventListener('click', openPaymentModal);

    // Self Satış hızlı ödeme kısayolları (MASTER tasarım, Edip 2026-09-03) - ürün panelinin
    // altındaki Nakit/Kredi Kartı/Yemek Çeki butonları restaurant-pos.js'ten burayı çağırır.
    // Madde 3 (Edip, 2026-09-04) - Nakit/Kredi Kartı/Yemek Çeki tuşu artık modalı AÇIP kasiyerin
    // "Siparişi Tamamla"ya basmasını BEKLEMİYOR: tam tutarı anında TEK yöntemle kapatıyor, hiçbir
    // onay istemiyor. "Satış sonrası tahsilat fişi sorulsun mu?" (InventorySettings) açıksa
    // kapanışın ardından küçük bir Fiş Yazdır | Kapat diyaloğu gösterilir; ikisi de sonunda boş
    // Self Satış ekranına döner. Kapanış başarısız olursa (ör. tutar uyuşmazlığı) dökme yol olarak
    // normal ödeme modalı bu satırla ön dolu açılır, kasiyer elle düzeltebilir.
    window.RestaurantQuickPay = function (method) {
        if (payableTotal <= 0) return;
        var quickMethod = parseInt(method, 10);
        var quickLine = {
            method: quickMethod,
            financialAccountId: resolveFinancialAccountId(quickMethod),
            amount: payableTotal
        };
        submitClosePayment([quickLine], function (retailSale) {
            if (requireReceiptPromptAfterQuickPay) {
                showQuickPayReceiptPrompt(retailSale);
            } else {
                window.location.href = selfSaleUrl;
            }
        }, function (errorMessage) {
            openPaymentModal();
            paymentLines = [{ lineId: ++lineSeq, method: quickLine.method, financialAccountId: quickLine.financialAccountId, amount: quickLine.amount }];
            entryDigits = '';
            refresh();
            errorEl.textContent = errorMessage;
            errorEl.style.display = 'block';
        });
    };

    function showQuickPayReceiptPrompt(retailSale) {
        var modalEl = document.getElementById('quickPayReceiptModal');
        var modal = new bootstrap.Modal(modalEl);
        var receiptUrl = receiptUrlTemplate.replace('-1', String(retailSale.retailSaleId));
        document.getElementById('quickpay-print-btn').onclick = function () {
            window.open(receiptUrl, '_blank');
            window.location.href = selfSaleUrl;
        };
        document.getElementById('quickpay-close-btn').onclick = function () {
            window.location.href = selfSaleUrl;
        };
        modal.show();
    }

    var quickPayParam = new URLSearchParams(window.location.search).get('quickpay');
    if (quickPayParam) {
        window.RestaurantQuickPay(quickPayParam);
        var cleanUrl = new URL(window.location.href);
        cleanUrl.searchParams.delete('quickpay');
        window.history.replaceState({}, '', cleanUrl.toString());
    }

    // Mutfağa göndermeden "Ödemeyi Al" akışının devamı - restaurant-pos.js sepeti gönderip
    // ?openPayment=1 ile sayfayı yeniler, bu sayfa yüklemesindeki payableTotal artık güncel
    // (yeni eklenenler dahil) olduğu için modal doğru tutarla açılabilir.
    var openPaymentParam = new URLSearchParams(window.location.search).get('openPayment');
    if (openPaymentParam) {
        openPaymentModal();
        var cleanUrl2 = new URL(window.location.href);
        cleanUrl2.searchParams.delete('openPayment');
        window.history.replaceState({}, '', cleanUrl2.toString());
    }

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
            var typed = entryValue();
            if (typed <= 0) { return; }
            // Girilen tutar kalanı aşarsa (ör. müşteri 50 TL verdi, kalan 47,50 TL) sadece
            // KALAN kadarı kayda geçer - fazlası Para Üstü'dür, asla ödeme olarak yazılmaz
            // (madde 4 - CloseCheckAsync'in "ödeme toplamı == adisyon tutarı" katı eşitliği
            // bu sayede hiç bozulmaz, fazla tahsilat asla muhasebeye girmez).
            var rem = Math.max(remaining(), 0);
            var amount = Math.min(typed, rem);
            if (amount <= 0) { return; }
            var method = parseInt(btn.getAttribute('data-method'), 10);
            var financialAccountId = resolveFinancialAccountId(method);
            var collectionCariId = btn.getAttribute('data-collection-cari-id');

            function addPending() {
                btn.disabled = true;
                fetch(addPendingPaymentUrl, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': getCsrfToken() },
                    body: JSON.stringify({ checkId: checkId, method: method, financialAccountId: financialAccountId, amount: amount })
                })
                    .then(function (res) { return res.json().then(function (data) { return { ok: res.ok, data: data }; }); })
                    .then(function (result) {
                        btn.disabled = false;
                        if (!result.ok) {
                            errorEl.textContent = result.data.error || 'Ödeme kaydedilemedi.';
                            errorEl.style.display = 'block';
                            return;
                        }
                        paymentLines.push({
                            lineId: ++lineSeq,
                            pendingPaymentId: result.data.pendingPaymentId,
                            method: method,
                            financialAccountId: financialAccountId,
                            amount: amount
                        });
                        entryDigits = '';
                        refresh();
                    })
                    .catch(function () {
                        btn.disabled = false;
                        errorEl.textContent = 'Bağlantı hatası oluştu.';
                        errorEl.style.display = 'block';
                    });
            }

            // Tahsilat Carisi butonu (madde 14) - Açık Hesap ile AYNI mekanizma, sadece cari
            // seçimini "Cari Ekle" yerine bu buton kendisi yapar (henüz bağlı değilse/farklıysa).
            if (collectionCariId) {
                if (String(attachedCustomerId()) === collectionCariId) {
                    addPending();
                    return;
                }
                btn.disabled = true;
                fetch('/Restaurant/AttachCustomer', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-CSRF-TOKEN': getCsrfToken() },
                    body: 'checkId=' + encodeURIComponent(checkId) + '&customerId=' + encodeURIComponent(collectionCariId)
                })
                    .then(function (res) { return res.json(); })
                    .then(function (data) {
                        btn.disabled = false;
                        if (!data.success) {
                            errorEl.textContent = data.message || 'Cari eklenemedi.';
                            errorEl.style.display = 'block';
                            return;
                        }
                        document.getElementById('AttachedCustomerId').value = collectionCariId;
                        var label = document.getElementById('attach-customer-label');
                        if (label) label.textContent = btn.textContent.trim();
                        addPending();
                    })
                    .catch(function () {
                        btn.disabled = false;
                        errorEl.textContent = 'Bağlantı hatası oluştu.';
                        errorEl.style.display = 'block';
                    });
                return;
            }

            if (method === OPEN_ACCOUNT_METHOD && attachedCustomerId() === null) {
                errorEl.textContent = 'Cari seçmelisiniz.';
                errorEl.style.display = 'block';
                return;
            }

            addPending();
        });
    });

    // "Ödeme İptal" (madde 7) - Adisyona Dön'den FARKLI: kısmi ödemeleri sıfırlar, kullanıcı
    // ödeme ekranında KALIR (modal kapanmaz).
    document.getElementById('cancel-pending-payments-btn').addEventListener('click', function () {
        if (paymentLines.length === 0) { return; }
        window.posConfirm('Alınan tüm kısmi ödemeler iptal edilecek. Devam edilsin mi?', function () {
            fetch(cancelPendingPaymentsUrl + '?checkId=' + checkId, {
                method: 'POST',
                headers: { 'X-CSRF-TOKEN': getCsrfToken() }
            })
                .then(function () {
                    paymentLines = [];
                    entryDigits = '';
                    refresh();
                })
                .catch(function () {
                    errorEl.textContent = 'Ödemeler iptal edilirken bağlantı hatası oluştu.';
                    errorEl.style.display = 'block';
                });
        }, null, { danger: true });
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

    // Ortak kapanış gönderimi - hem "Siparişi Tamamla" (kısmi ödemelerin TAMAMI) hem de Madde 3
    // hızlı ödeme (TEK tam tutar satırı) burayı kullanır. linesForSubmit fiskal cihaza da AYNEN
    // gönderilir (paymentLines'ı GEÇİCİ olarak bu satırlara çevirip fiskal fonksiyonun mevcut
    // "paymentLines" okumasını bozmadan çalıştırıyoruz).
    function submitClosePayment(linesForSubmit, onSuccess, onError) {
        var customerId = null;
        var previousPaymentLines = paymentLines;
        paymentLines = linesForSubmit;

        sendToFiscalDevice(customerId, function (fiscal) {
            var payload = {
                checkId: checkId,
                submissionKey: document.getElementById('pos-close-submission-key').value,
                customerId: customerId,
                payments: linesForSubmit.map(function (l) {
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
                        paymentLines = previousPaymentLines;
                        onError(result.data.error || 'Adisyon kapatılamadı.');
                        return;
                    }
                    onSuccess(result.data);
                })
                .catch(function () {
                    paymentLines = previousPaymentLines;
                    onError('Bağlantı hatası oluştu.');
                });
        }, function (fiscalErrorMessage) {
            paymentLines = previousPaymentLines;
            onError(fiscalErrorMessage);
        });
    }

    confirmBtn.addEventListener('click', function () {
        confirmBtn.disabled = true;
        confirmBtn.textContent = 'Tamamlanıyor...';
        errorEl.style.display = 'none';

        submitClosePayment(paymentLines.slice(), function () {
            // Ödeme tamamlandıktan sonra masa/self/paket fark etmeksizin HER ZAMAN Self Satış
            // ekranında kalınır (Edip, 2026-09-03: "masa ödemesini de alsa self ödeme de alsa
            // pakette de ödeme alsa self satış ekranında kalsın hep").
            window.location.href = selfSaleUrl;
        }, function (errorMessage) {
            errorEl.textContent = errorMessage;
            errorEl.style.display = 'block';
            confirmBtn.disabled = false;
            confirmBtn.textContent = 'Siparişi Tamamla';
        });
    });
})();
