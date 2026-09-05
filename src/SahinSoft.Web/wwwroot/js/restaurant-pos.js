(function () {
    'use strict';

    var root = document.getElementById('pos-root');
    if (!root) return;

    // "Şifre sorulsun mu?" (madde 21) - ortak ikinci yetkili PIN onayı yardımcısı. requiresApproval
    // false ise onReady(null) hemen çağrılır (modal hiç açılmaz). true ise approverPinModal
    // açılır, Onayla'ya basınca onReady(pin) çağrılır. window'a bağlı - Check.cshtml'in kendi
    // inline script'i de (bu IIFE dışında) kullanabiliyor.
    window.requestApproverPin = function (requiresApproval, onReady) {
        if (!requiresApproval) { onReady(null); return; }
        var modalEl = document.getElementById('approverPinModal');
        var input = document.getElementById('approverPinInput');
        var confirmBtn = document.getElementById('approverPinConfirmBtn');
        if (!modalEl || !input || !confirmBtn) { onReady(null); return; }
        input.value = '';
        var modal = new bootstrap.Modal(modalEl);
        confirmBtn.onclick = function () {
            var pin = input.value;
            if (!pin) { window.posAlert('PIN girilmelidir.'); return; }
            modal.hide();
            onReady(pin);
        };
        modal.show();
        setTimeout(function () { input.focus(); }, 300);
    };

    var checkId = parseInt(root.getAttribute('data-check-id'), 10);
    var checkNumber = root.getAttribute('data-check-number');
    var tableLabel = root.getAttribute('data-table-label');
    var isSelfSale = root.getAttribute('data-is-self-sale') === 'true';
    // Zaten gönderilmiş/kalıcı satırların tutarı (bkz. Check.cshtml #cart-sent-lines) - "Toplam"
    // sadece bekleyen sepeti değil BUNU DA içermeli, aksi halde mutfağa gönderdikten sonra
    // Toplam sıfıra düşüyormuş gibi görünür (Edip, 2026-09-03: "mutfağa gönderdikten sonra tutar
    // kısmı 0 geliyor").
    var sentLinesTotal = parseFloat(root.getAttribute('data-payable-total')) || 0;
    var sendUrl = root.getAttribute('data-send-url');
    var catalog = JSON.parse(document.getElementById('pos-catalog-data').textContent || '[]');
    var cart = [];
    var cartSeq = 0;

    function getCsrfToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function money(v) {
        return v.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ₺';
    }

    function findCatalogProduct(productId) {
        for (var i = 0; i < catalog.length; i++) {
            for (var j = 0; j < catalog[i].products.length; j++) {
                if (catalog[i].products[j].productId === productId) return catalog[i].products[j];
            }
        }
        return null;
    }

    // --- Fişi Beklet / Bekleyen Fişler (2026-09-05 teknik doküman madde 8) - GERÇEK bir veri
    // tabanı durumu (RestaurantCheck.HeldAtUtc), localStorage YOK. Böylece hangi terminalden/
    // kasiyerden bakılırsa bakılsın aynı liste görünür, ve bir tarayıcı verisi silindiğinde
    // "bekletiyor gibi görünüp listede görünmeme" hatası oluşmaz (önceki mimarinin gerçek kökeni).
    // Geri çağırma SADECE Bekleyen Fişler'deki bir karta tıklanınca (Recall POST) olur - sıradan
    // bir Self Satış girişinde ASLA otomatik gelmez (HeldAtUtc dolu check'ler
    // RestaurantSelfSaleController.Index'in "benim açık check'im" sorgusundan HARİÇ tutulur).

    // --- Kategori sekmeleri + ürün ızgarası + arama ---
    var tabsEl = document.getElementById('category-tabs');
    var gridEl = document.getElementById('product-grid');
    var searchEl = document.getElementById('product-search');
    var activeCategory = null;

    function renderCategories() {
        tabsEl.innerHTML = '';
        catalog.forEach(function (cat, idx) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'category-tab' + (idx === 0 ? ' active' : '');
            // Kategori adının yanında ürün sayısı - Edip'in kendi eski POS ekranındaki gibi
            // (2026-09-03).
            btn.innerHTML = escapeHtml(cat.categoryName) + ' <span class="category-tab-count">' + cat.products.length + '</span>';
            // Kategori Tanımla'da seçilen renk burada aynen kullanılır (Edip, 2026-09-03).
            if (cat.color) btn.style.setProperty('--cat-tab-color', cat.color);
            btn.addEventListener('click', function () {
                tabsEl.querySelectorAll('.category-tab').forEach(function (b) { b.classList.remove('active'); });
                btn.classList.add('active');
                if (searchEl) searchEl.value = '';
                renderProducts(cat);
            });
            tabsEl.appendChild(btn);
        });
        if (catalog.length > 0) renderProducts(catalog[0]);
    }

    function renderProducts(category) {
        activeCategory = category;
        renderProductList(category.products);
    }

    function renderProductList(products) {
        gridEl.innerHTML = '';
        products.forEach(function (p) {
            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'product-tile';
            // Stok Tanıtım Kartı'nda fotoğraf yüklenmişse kartta gösterilir, yoksa nötr bir
            // simge ile yer tutucu gösterilir (Edip, 2026-09-03: regeditpos referansı).
            var photoHtml = p.imagePath
                ? '<span class="product-photo"><img src="' + escapeHtml(p.imagePath) + '" alt="" loading="lazy" /></span>'
                : '<span class="product-photo product-photo-empty"><i class="fa-solid fa-utensils"></i></span>';
            btn.innerHTML = photoHtml +
                '<span class="product-info">' +
                '<span class="product-name">' + escapeHtml(p.name) + '</span>' +
                '<span class="product-price">' + money(p.salePrice) + '</span>' +
                '</span>';
            btn.addEventListener('click', function () { addToCart(p); });
            gridEl.appendChild(btn);
        });
        if (products.length === 0) {
            gridEl.innerHTML = '<p class="text-secondary small p-2">Sonuç bulunamadı.</p>';
        }
    }

    // Ürün ara / barkod okut kutusu - kısayol grid'i sadece kategori altına tanımlı ürünleri
    // gösterir, ama ARAMA tüm aktif stoktan sunucuya sorulur (Edip, 2026-09-03: "stok ve ürün
    // arama sol tarafta kategoride değil stok listesinde arasın veritabanında stok" / "stoklarda
    // arama yapsın") - kısayol olarak tanımlanmamış bir ürün de böylece bulunup satılabilir.
    var searchProductsUrl = root.getAttribute('data-search-products-url');
    var searchDebounceTimer = null;
    var lastSearchResults = [];

    function runSearch(term, onDone) {
        fetch(searchProductsUrl + '?term=' + encodeURIComponent(term))
            .then(function (r) { return r.json(); })
            .then(function (results) {
                lastSearchResults = results;
                if (onDone) onDone(results);
            })
            .catch(function () { /* bağlantı hatası - sessizce yok say, kısayol grid'i kalır */ });
    }

    if (searchEl) {
        searchEl.addEventListener('input', function () {
            var term = searchEl.value.trim();
            if (!term) {
                tabsEl.style.display = '';
                lastSearchResults = [];
                if (activeCategory) renderProducts(activeCategory);
                return;
            }
            tabsEl.style.display = 'none';
            if (searchDebounceTimer) clearTimeout(searchDebounceTimer);
            searchDebounceTimer = setTimeout(function () {
                runSearch(term, function (results) { renderProductList(results); });
            }, 250);
        });

        // Barkod okuyucu Enter ile gönderir - tam barkod eşleşmesi varsa direkt sepete eklenir
        // (bekleyen numpad miktarıyla), yoksa arama TEK sonuca düştüyse o eklenir - isimle yazıp
        // Enter'a basmak da aynı şekilde çalışsın diye (Edip, 2026-09-03: "tek satırda barkod ve
        // isimden satış yapabilsin"). Debounce'u beklemeden ANINDA sunucudan taze sonuç alır ki
        // hızlı okutan bir barkod tabancası kaçırılmasın.
        searchEl.addEventListener('keydown', function (e) {
            if (e.key !== 'Enter') return;
            var raw = searchEl.value.trim();
            if (!raw) return;
            if (searchDebounceTimer) clearTimeout(searchDebounceTimer);
            runSearch(raw, function (results) {
                var byBarcode = results.find(function (p) { return p.barcodes && p.barcodes.indexOf(raw) !== -1; });
                var target = byBarcode || (results.length === 1 ? results[0] : null);
                if (target) {
                    addToCart(target);
                    searchEl.value = '';
                    tabsEl.style.display = '';
                    lastSearchResults = [];
                    if (activeCategory) renderProducts(activeCategory);
                } else {
                    renderProductList(results);
                }
            });
        });
    }

    function escapeHtml(s) {
        var d = document.createElement('div');
        d.textContent = s;
        return d.innerHTML;
    }

    // --- Sepet ---
    // Satır işlem çubuğu SABİT (bkz. Check.cshtml #cart-line-toolbar) - satıra tıklayınca
    // seçilir, alttaki tek çubuk (miktar +/-, Not, İndirim, İkram, Sil) o satıra uygulanır.
    var selectedCartId = null;

    function selectedLine() {
        return cart.find(function (x) { return x.cartId === selectedCartId; }) || null;
    }

    // Sol numpad'ın bekleyen miktarı (Edip, 2026-09-03: "numaralara tıkladı örnek 5 tıkladı
    // çorbaya tıkladı ekrana 5 tane çorba attı" / "P porsiyon demek 1.5 duble 0.5 seçebilir").
    // Ürün eklendikten sonra 1'e sıfırlanır - bkz. numpad wiring aşağıda.
    var pendingQuantity = 1;

    function setPendingQuantity(v) {
        pendingQuantity = v;
        var el = document.getElementById('numpad-pending');
        if (el) el.textContent = (v % 1 === 0 ? v.toString() : v.toString());
    }

    function finishAddToCart(product, portionId, portionName, unitPrice) {
        cart.push({
            cartId: ++cartSeq,
            productId: product.productId,
            productPortionId: portionId,
            name: product.name,
            portionName: portionName,
            unitPrice: unitPrice,
            taxRate: product.taxRate,
            unit: product.unit || 'Adet',
            quantity: pendingQuantity,
            discountAmount: 0,
            isComplimentary: false,
            kitchenNote: null,
            hasKitchenStation: product.hasKitchenStation
        });
        selectedCartId = cartSeq;
        setPendingQuantity(1);
        renderCart();
    }

    function addToCart(product) {
        if (!product.portions || product.portions.length === 0) {
            finishAddToCart(product, null, null, product.salePrice);
            return;
        }
        var names = product.portions.map(function (x, i) { return (i + 1) + ') ' + x.name; }).join('\n');
        window.posPrompt('Porsiyon seçin:\n' + names + '\n\n(Numara girin, boş bırakırsanız varsayılan porsiyon kullanılır)', '', function (choice) {
            var selected = null;
            if (choice && !isNaN(parseInt(choice, 10))) {
                selected = product.portions[parseInt(choice, 10) - 1];
            } else {
                selected = product.portions.find(function (x) { return x.isDefault; }) || product.portions[0];
            }
            var portionId = null, portionName = null, unitPrice = product.salePrice;
            if (selected) {
                portionId = selected.portionId;
                portionName = selected.name;
                unitPrice = selected.priceOverride != null ? selected.priceOverride : product.salePrice;
            }
            finishAddToCart(product, portionId, portionName, unitPrice);
        }, function () {
            // Vazgeç - varsayılan porsiyonla ekle (eski native prompt'ta "İptal" da boş string
            // gibi davranıp varsayılanı kullanırdı, aynı davranış korunuyor).
            var selected = product.portions.find(function (x) { return x.isDefault; }) || product.portions[0];
            var unitPrice = selected && selected.priceOverride != null ? selected.priceOverride : product.salePrice;
            finishAddToCart(product, selected ? selected.portionId : null, selected ? selected.name : null, unitPrice);
        });
    }

    // Sunucudaki RestaurantController.ComputeCheckRunningTotal ile AYNI formül. Ürün fiyatı
    // (Product.SalePrice / ProductPortion.PriceOverride) sistemde zaten KDV DAHİL tutulur — bkz.
    // stok kartı/fatura fiyat politikası — bu yüzden burada KDV bir daha eklenmez. Ekranda "ekstra
    // KDV hesabı" yapılmaması kasıtlıdır; KDV yalnızca adisyon kapanışında (Faz 3) tutardan geriye
    // doğru ayrıştırılır.
    function lineTotal(line) {
        var gross = line.quantity * line.unitPrice;
        var discount = line.isComplimentary ? gross : Math.min(line.discountAmount, gross);
        return Math.max(0, gross - discount);
    }

    // Sunucudaki RestaurantPricingCalculator.ExtractTax ile AYNI merkezi kural — ekranda
    // gösterilmez (KDV her yerde dahil tutar olarak kalır), yalnızca Faz 3'te adisyon
    // kapanışında/fiş üretiminde matrah+KDV'yi tutardan geriye doğru ayrıştırmak için kullanılır.
    // Matrah yuvarlanır, KDV tutarı kalan olarak hesaplanır (toplam her zaman birebir eşleşir).
    function extractTax(kdvDahilTutar, kdvOrani) {
        var matrah = Math.round((kdvDahilTutar / (1 + kdvOrani / 100)) * 100) / 100;
        var kdvTutari = Math.round((kdvDahilTutar - matrah) * 100) / 100;
        return { matrah: matrah, kdvTutari: kdvTutari };
    }

    function renderCart() {
        var linesEl = document.getElementById('cart-lines');
        var totalEl = document.getElementById('cart-total');
        var sendBtn = document.getElementById('send-kitchen-btn');

        if (!cart.some(function (x) { return x.cartId === selectedCartId; })) selectedCartId = null;

        var payBtn = document.getElementById('self-pay-btn');

        // Ara Toplam/Kalan Tutar (Edip, 2026-09-05: "Ara Toplam yapmıyor şu an") - bu iki alan
        // sayfa yüklenişinde sunucudan gelen (SADECE gönderilmiş satırları sayan) DONMUŞ değerle
        // basılıyordu, sepete yeni ürün eklendiğinde hiç güncellenmiyordu - eskiden sadece şimdi
        // gizli olan "Toplam" alanı canlı güncelleniyordu. window.RestaurantCartSubtotal'ı burada
        // güncelleyip restaurant-close-payment.js'teki paylaşılan yenileme fonksiyonunu çağırarak
        // İKİSİ de (ve Kalan Tutar'ın ödenen/indirim düşülmüş hali) senkron kalır.
        function syncRunningTotals(subtotal) {
            totalEl.textContent = money(subtotal);
            window.RestaurantCartSubtotal = subtotal;
            if (window.RestaurantRefreshRunningTotals) window.RestaurantRefreshRunningTotals();
        }

        if (cart.length === 0) {
            linesEl.innerHTML = '<p class="text-secondary small p-2">Ürün eklemek için soldan seçim yapın.</p>';
            syncRunningTotals(sentLinesTotal);
            sendBtn.disabled = true;
            if (payBtn) payBtn.disabled = sentLinesTotal <= 0;
            updateLineToolbar();
            return;
        }

        linesEl.innerHTML = '';
        var total = 0;
        // Sıra numarası (Edip, 2026-09-05: "1,2,3 devam etsin kaç ürün varsa") - gönderilmiş
        // satırların DEVAMI olarak numaralanır, Check.cshtml'deki @@for ile AYNI tek sayaç
        // mantığı (gönderilmiş satırlar 1..N ise bekleyen sepet N+1'den başlar).
        var sentLineCount = document.querySelectorAll('.cart-line.sent').length;
        cart.forEach(function (line, idx) {
            total += lineTotal(line);
            var div = document.createElement('div');
            div.className = 'cart-line' + (line.cartId === selectedCartId ? ' selected' : '');
            var badges = '';
            if (line.isComplimentary) badges += ' <span class="badge text-bg-info-subtle text-info-emphasis">İKRAM</span>';
            else if (line.discountAmount > 0) badges += ' <span class="badge text-bg-warning-subtle text-warning-emphasis">İndirim ' + money(line.discountAmount) + '</span>';
            if (!line.hasKitchenStation) badges += ' <span class="badge text-bg-secondary-subtle" title="Mutfak istasyonu tanımlı değil">İstasyonsuz</span>';

            // Sabit şablon: # | Ürün Adı | Miktar | Birim | KDV | Fiyat | Tutar (Edip, 2026-09-03).
            div.innerHTML =
                '<div class="cart-line-col-no">' + (sentLineCount + idx + 1) + '</div>' +
                '<div class="cart-line-col-name"><span class="cart-line-name-text">' + escapeHtml(line.name) + (line.portionName ? ' (' + escapeHtml(line.portionName) + ')' : '') + '</span>' + badges +
                (line.kitchenNote ? '<div class="small text-secondary">Not: ' + escapeHtml(line.kitchenNote) + '</div>' : '') + '</div>' +
                '<div class="cart-line-col-qty">' + line.quantity + '</div>' +
                '<div class="cart-line-col-unit">' + escapeHtml(line.unit || 'Adet') + '</div>' +
                '<div class="cart-line-col-kdv">%' + line.taxRate + '</div>' +
                '<div class="cart-line-col-price">' + money(line.unitPrice) + '</div>' +
                '<div class="cart-line-col-total">' + money(lineTotal(line)) + '</div>';

            div.addEventListener('click', function () {
                selectedCartId = line.cartId;
                renderCart();
            });

            linesEl.appendChild(div);
        });

        syncRunningTotals(total + sentLinesTotal);
        sendBtn.disabled = false;
        if (payBtn) payBtn.disabled = (total + sentLinesTotal) <= 0;
        updateLineToolbar();
    }

    // Sabit satır işlem çubuğu - Edip, 2026-09-03: "mutfağa gönder dediğimizde bu butonlar
    // pasif oluyor artık olmasın hep aktif olsun ... sipariş sil butonu her zaman aktif olsun".
    // Butonlar ARTIK hiçbir zaman disabled olmuyor (mutfağa gönderilince sepet boşalıp seçili
    // satır kalmasa bile); her tıklama zaten kendi içinde "seçili satır yoksa hiçbir şey yapma"
    // koruması taşıyor (bkz. aşağıdaki click handler'ları), bu yüzden güvenli.
    //
    // Gönderilmiş (sent) satırlar (Edip, 2026-09-05, "Adisyona Dön"dükten sonra sağ taraftaki
    // küçük simgeler çıkmasın, sabit üst satırdaki alanlardan yapayım") - PENDING sepet
    // satırlarıyla AYNI tıkla-seç + sabit araç çubuğu mekanizmasını kullanır. Aralarındaki fark:
    // pending satır saf JS state (anında, sunucuya dokunmadan), sent satır GERÇEK bir DB satırı -
    // bu yüzden qty/ikram/sil işlemleri AYNI sunucu uçlarını (PIN onayı dahil) tetikler, sadece
    // eskiden küçük ikonların yaptığını şimdi bu üst çubuk yapıyor.
    var selectedSentLineId = null;
    var requireApprovalEditKitchenSent = root.getAttribute('data-require-second-approval-edit-kitchen-sent') === 'true';
    var requireApprovalComplimentary = root.getAttribute('data-require-second-approval-complimentary') === 'true';
    var requireApprovalDiscountForLine = root.getAttribute('data-require-second-approval-discount') === 'true';

    function selectedSentLineEl() {
        if (selectedSentLineId === null) return null;
        return document.querySelector('.cart-line.sent[data-line-id="' + selectedSentLineId + '"]');
    }

    document.querySelectorAll('.cart-line.sent.selectable').forEach(function (row) {
        row.addEventListener('click', function () {
            selectedCartId = null;
            selectedSentLineId = selectedSentLineId === parseInt(row.getAttribute('data-line-id'), 10) ? null : parseInt(row.getAttribute('data-line-id'), 10);
            document.querySelectorAll('.cart-line.sent').forEach(function (r) { r.classList.remove('selected'); });
            if (selectedSentLineId !== null) row.classList.add('selected');
            renderCart();
        });
    });

    function submitSentLineQtyChange(lineId, newQty) {
        if (newQty <= 0) { window.posAlert('Geçersiz miktar.'); return; }
        window.requestApproverPin(requireApprovalEditKitchenSent, function (pin) {
            document.getElementById('adjustQtyLineId').value = lineId;
            document.getElementById('adjustQtyValue').value = newQty;
            document.getElementById('adjustQtyApproverPin').value = pin || '';
            document.getElementById('adjust-line-qty-form').submit();
        });
    }

    // Gönderilmiş satıra indirim (Edip, 2026-09-05: "bu satır indirimini yapsın bu hatayı
    // vermesin üst butonlar aktif çalışsın" - önceki hali sadece bir uyarı gösterip fiş geneli
    // %İndirim'e yönlendiriyordu, gerçek bir uç nokta yoktu). Onay bayrağı
    // RequireSecondApprovalForDiscount'tur (satır düzenleme değil, bir İNDİRİM işlemi - bkz.
    // RestaurantPostingService.ApplyOrderLineDiscountAsync).
    function submitSentLineDiscount(lineId, amount) {
        window.requestApproverPin(requireApprovalDiscountForLine, function (pin) {
            document.getElementById('applyDiscountLineId').value = lineId;
            document.getElementById('applyDiscountAmount').value = amount;
            document.getElementById('applyDiscountApproverPin').value = pin || '';
            document.getElementById('apply-line-discount-form').submit();
        });
    }

    function submitSentLineComp(lineId) {
        window.posConfirm('Bu satırın ikram durumu değiştirilsin mi?', function () {
            window.requestApproverPin(requireApprovalComplimentary, function (pin) {
                document.getElementById('toggleCompLineId').value = lineId;
                document.getElementById('toggleCompApproverPin').value = pin || '';
                document.getElementById('toggle-line-comp-form').submit();
            });
        });
    }

    function updateLineToolbar() {
        var line = selectedLine();
        var sentEl = selectedSentLineEl();
        var qtyValueEl = document.getElementById('line-qty-value');
        qtyValueEl.textContent = line ? line.quantity : (sentEl ? sentEl.getAttribute('data-qty') : '–');
    }

    document.getElementById('line-qty-minus').addEventListener('click', function () {
        var line = selectedLine();
        if (line) { line.quantity = Math.max(1, line.quantity - 1); renderCart(); return; }
        var sentEl = selectedSentLineEl();
        if (!sentEl) return;
        var qty = parseFloat(sentEl.getAttribute('data-qty'));
        submitSentLineQtyChange(selectedSentLineId, Math.max(1, qty - 1));
    });
    document.getElementById('line-qty-plus').addEventListener('click', function () {
        var line = selectedLine();
        if (line) { line.quantity += 1; renderCart(); return; }
        var sentEl = selectedSentLineEl();
        if (!sentEl) return;
        var qty = parseFloat(sentEl.getAttribute('data-qty'));
        submitSentLineQtyChange(selectedSentLineId, qty + 1);
    });
    // Hazır not seçenekleri varsa (Edip, 2026-09-03: "otomatik not girilecek alanlar ekle")
    // porsiyon seçimindeki AYNI numaralı liste deseni - ayrı bir modal gerekmez, kasiyer numara
    // girip hazır notu seçebilir ya da kendi notunu yazabilir.
    var quickNotePresets = JSON.parse(document.getElementById('pos-quick-notes-data').textContent || '[]');
    document.getElementById('line-act-note').addEventListener('click', function () {
        var line = selectedLine();
        if (!line) {
            // Gönderilmiş satırın notu bu ekrandan değiştirilemez - ayrı bir sunucu ucu YOK,
            // uydurma bir "kaydedildi" mesajı gösterilmiyor.
            if (selectedSentLineEl()) window.posAlert('Gönderilmiş satırın notu buradan değiştirilemez.');
            return;
        }
        function applyNote(note) {
            if (note !== null) line.kitchenNote = note.trim() || null;
            renderCart();
        }
        if (quickNotePresets.length > 0) {
            var options = quickNotePresets.map(function (n, i) { return (i + 1) + ') ' + n; }).join('\n');
            window.posPrompt('Not (mutfağa iletilecek):\n' + options + '\n\n(Hazır not için numara girin, ya da kendi notunuzu yazın)', line.kitchenNote || '', function (choice) {
                var idx = parseInt(choice, 10);
                var note = (!isNaN(idx) && idx >= 1 && idx <= quickNotePresets.length && String(idx) === choice.trim()) ? quickNotePresets[idx - 1] : choice;
                applyNote(note);
            });
        } else {
            window.posPrompt('Not (mutfağa iletilecek):', line.kitchenNote || '', applyNote);
        }
    });
    document.getElementById('line-act-discount').addEventListener('click', function () {
        var line = selectedLine();
        if (!line) {
            var sentEl = selectedSentLineEl();
            if (!sentEl) return;
            var currentDiscount = parseFloat(sentEl.getAttribute('data-discount')) || 0;
            window.posPrompt('İndirim tutarı (₺):', currentDiscount, function (sentAmountStr) {
                var sentAmount = parseFloat(sentAmountStr);
                if (!isNaN(sentAmount) && sentAmount >= 0) {
                    submitSentLineDiscount(selectedSentLineId, sentAmount);
                }
            });
            return;
        }
        window.posPrompt('İndirim tutarı (₺):', line.discountAmount || 0, function (amountStr) {
            var amount = parseFloat(amountStr);
            if (!isNaN(amount) && amount >= 0) {
                line.discountAmount = amount;
                line.isComplimentary = false;
            }
            renderCart();
        });
    });
    document.getElementById('line-act-comp').addEventListener('click', function () {
        var line = selectedLine();
        if (line) { line.isComplimentary = !line.isComplimentary; renderCart(); return; }
        if (selectedSentLineId !== null) submitSentLineComp(selectedSentLineId);
    });
    document.getElementById('line-act-remove').addEventListener('click', function () {
        var line = selectedLine();
        if (line) {
            cart = cart.filter(function (x) { return x.cartId !== line.cartId; });
            selectedCartId = null;
            renderCart();
            return;
        }
        if (selectedSentLineId !== null && window.triggerSentLineCancel) window.triggerSentLineCancel(selectedSentLineId);
    });

    // Sağ ikon şeridindeki "Mutfak" - alt kısımdaki Mutfağa Gönder ile aynı işlemi tetikler,
    // ayrı bir gönderim mantığı yazılmaz (Edip, 2026-09-03: sağdaki sabit ikon şeridi isteği).
    var sideKitchenBtn = document.getElementById('side-kitchen-btn');
    if (sideKitchenBtn) {
        sideKitchenBtn.addEventListener('click', function () {
            var sendBtn = document.getElementById('send-kitchen-btn');
            if (sendBtn && !sendBtn.disabled) sendBtn.click();
        });
    }

    // Sepetteki bekleyen satırları mutfağa gönderir - hem "Mutfağa Gönder" butonu hem de
    // "Masaya Aktar" akışı (aktarımdan önce sepet boş kalmasın diye) BU fonksiyonu kullanır,
    // aynı gönderim isteği iki yerde ayrı ayrı yazılmaz.
    function flushCartToKitchen(onDone, onError) {
        if (cart.length === 0) { onDone(); return; }

        var payload = {
            checkId: checkId,
            submissionKey: document.getElementById('pos-submission-key').value,
            lines: cart.map(function (line) {
                return {
                    productId: line.productId,
                    productPortionId: line.productPortionId,
                    quantity: line.quantity,
                    discountAmount: line.discountAmount,
                    isComplimentary: line.isComplimentary,
                    kitchenNote: line.kitchenNote,
                    modifiers: []
                };
            })
        };

        fetch(sendUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': getCsrfToken() },
            body: JSON.stringify(payload)
        })
            .then(function (res) { return res.json().then(function (data) { return { ok: res.ok, data: data }; }); })
            .then(function (result) {
                if (!result.ok || !result.data.success) {
                    alert('Hata: ' + (result.data.error || 'Sipariş gönderilemedi.'));
                    onError();
                    return;
                }
                if (result.data.unroutedProductNames && result.data.unroutedProductNames.length > 0) {
                    // Gönderim sonrası masa durumuna otomatik dönmeden önce, mutfak istasyonu
                    // olmayan ürünleri kullanıcıya AÇIKÇA bildir (sessizce atlanmaz).
                    alert('Şu ürünler mutfak istasyonuna sahip değil, mutfağa gönderilmedi:\n' + result.data.unroutedProductNames.join('\n'));
                }
                onDone();
            })
            .catch(function () {
                alert('Sipariş gönderilirken bir bağlantı hatası oluştu.');
                onError();
            });
    }

    // Madde 22 (2026-09-05) - sistem geneli/yetki kapalıysa Check.cshtml bu butonu display:none
    // yapıyor (DOM'dan KALDIRMIYOR - aşağıdaki gibi başka JS'lerin de bu ID'ye güvenli erişebilmesi
    // için); bu yüzden burada bulunamama ihtimaline karşı GÜVENLİ null kontrolü eklendi (bu satır
    // daha önce korumasızdı - gerçek bir potansiyel hataydı, bulunup düzeltildi).
    var sendKitchenBtn = document.getElementById('send-kitchen-btn');
    if (sendKitchenBtn) {
        sendKitchenBtn.addEventListener('click', function () {
            if (cart.length === 0) return;
            var btn = this;
            btn.disabled = true;
            btn.textContent = 'Gönderiliyor...';
            // GERÇEK HATA (2026-09-05, Edip bildirdi) - burada data-back-url'e (Masa Durumu'na)
            // yönlendiriyordu, yani "Mutfağa Gönder" adisyon ekranından TAMAMEN çıkarıyordu; bu
            // yüzden alt ödeme butonları/ürün başlıkları "kayboluyor" gibi görünüyordu (aslında
            // farklı bir sayfaya geçilmiş oluyordu). Mutfağa gönderim adisyon ekranında KALMALI,
            // sadece sepeti temizleyip gönderilmiş satırları göstermek için sayfa yenilenmeli.
            flushCartToKitchen(
                function () { window.location.reload(); },
                function () { btn.disabled = false; btn.textContent = 'Mutfağa Gönder'; });
        });
    }

    // Self Satış'a özgü "Masaya Aktar" - bkz. Check.cshtml (yalnızca Self Satış adisyonlarında
    // render edilir). Aktarımdan önce sepette bekleyen (henüz mutfağa gönderilmemiş) satır varsa
    // önce onlar gönderilir, SONRA gerçek POST formu (RestaurantSelfSale/TransferToTable) sunucuya
    // gider - aksi halde henüz kaydedilmemiş sepet satırları aktarımda kaybolurdu.
    var transferForm = document.getElementById('transfer-to-table-form');
    if (transferForm) {
        transferForm.addEventListener('submit', function (e) {
            e.preventDefault();
            var btn = document.getElementById('confirm-transfer-btn');
            if (btn) { btn.disabled = true; btn.textContent = 'Aktarılıyor...'; }
            flushCartToKitchen(
                function () { transferForm.submit(); },
                function () { if (btn) { btn.disabled = false; btn.textContent = 'Masaya Aktar'; } });
        });
    }

    // Self Satış hızlı ödeme kısayolları (MASTER tasarım) - sepette bekleyen ürün varsa önce
    // mutfağa gönderilir (Self Satış'ta da ürünler normal sipariş satırı olarak işlenir), sonra
    // ?quickpay=<method> ile SAYFA YENİDEN YÜKLENİR ki PayableTotal sunucuda güncel hesaplansın;
    // asıl ödeme modalını açıp ön dolduran kısım restaurant-close-payment.js'teki
    // window.RestaurantQuickPay'dedir - burada sadece tetikleniyor.
    var quickPayButtons = document.querySelectorAll('.self-quickpay [data-quick-method]');
    quickPayButtons.forEach(function (btn) {
        btn.addEventListener('click', function () {
            var method = btn.getAttribute('data-quick-method');
            quickPayButtons.forEach(function (b) { b.disabled = true; });

            if (cart.length === 0) {
                if (window.RestaurantQuickPay) {
                    window.RestaurantQuickPay(method);
                }
                quickPayButtons.forEach(function (b) { b.disabled = false; });
                return;
            }

            flushCartToKitchen(
                function () {
                    var url = new URL(window.location.href);
                    url.searchParams.set('quickpay', method);
                    window.location.href = url.toString();
                },
                function () { quickPayButtons.forEach(function (b) { b.disabled = false; }); });
        });
    });

    // "Ödemeyi Al" - mutfağa hiç gönderilmeden de tahsilat alınabilsin diye, tıklanınca sepette
    // bekleyen (henüz gönderilmemiş) ürün varsa ÖNCE onlar sunucuya yazılır (KDS kapalıyken
    // doğrudan Servis Edildi olur, mutfak fişi açılmaz), SONRA sayfa ?openPayment=1 ile yeniden
    // yüklenir ki ödeme modalı güncel (yeni eklenenleri de içeren) tutarla açılsın - aksi halde
    // restaurant-close-payment.js sayfa yüklendiğindeki ESKİ PayableTotal'ı kullanırdı (Edip,
    // 2026-09-03: "masa siparişi giriyorum mutfağa göndermeden de tahsilat alabileyim"). Bu
    // listener restaurant-close-payment.js'in kendi tıklama dinleyicisinden ÖNCE eklenir (script
    // sırası) - sepette bir şey varsa stopImmediatePropagation ile o dinleyiciyi (eski tutarla
    // modalı direkt açardı) devre dışı bırakır; sepet boşsa dokunmadan geçer.
    var selfPayBtn = document.getElementById('self-pay-btn');
    if (selfPayBtn) {
        selfPayBtn.addEventListener('click', function (e) {
            if (cart.length === 0) return;
            e.stopImmediatePropagation();
            selfPayBtn.disabled = true;
            flushCartToKitchen(
                function () {
                    var url = new URL(window.location.href);
                    url.searchParams.set('openPayment', '1');
                    window.location.href = url.toString();
                },
                function () { selfPayBtn.disabled = false; });
        });
    }

    // "Adisyon" (onaylı mockup, madde birebir-uygulama) - tam ödeme modalını açan #self-pay-btn
    // ile TAMAMEN AYNI davranış, yeni bir akış İCAT EDİLMEDİ - sadece ikinci bir giriş noktası.
    var selfAdisyonBtn = document.getElementById('self-adisyon-btn');
    if (selfAdisyonBtn && selfPayBtn) {
        selfAdisyonBtn.addEventListener('click', function () {
            selfPayBtn.click();
        });
    }

    // --- Fiyat Gör (madde 5, 2026-09-05 teknik doküman) - Ürün Listesi ile AYNI gerçek stok DB
    // araması (SearchProducts, Ad/Barkod modu) - eskiden sadece zaten yüklü kategori kısayolu
    // kataloğunda (isim bazlı) arıyordu, kategori panelini asla etkilemez. Seçilen satır detay
    // paneline (Fiyat/KDV) düşer, "Ekrana Al" ile sepete eklenir - Ürün Listesi'nden farklı olarak
    // tek tıkla eklemez, önce fiyat/KDV'yi göstermesi gerekir (spec). ---
    (function () {
        var modalEl = document.getElementById('priceCheckModal');
        if (!modalEl) return;
        var searchEl = document.getElementById('price-check-search');
        var resultsEl = document.getElementById('price-check-results');
        var emptyEl = document.getElementById('price-check-empty');
        var detailEl = document.getElementById('price-check-detail');
        var nameEl = document.getElementById('price-check-name');
        var priceEl = document.getElementById('price-check-price');
        var taxEl = document.getElementById('price-check-tax');
        var addBtn = document.getElementById('price-check-add-btn');
        var nameBtn = document.getElementById('price-check-mode-name');
        var barcodeBtn = document.getElementById('price-check-mode-barcode');
        var searchUrl = root.getAttribute('data-search-products-url');
        var mode = 'name';
        var debounceTimer = null;
        var currentResults = [];
        var selected = null;

        function renderResults(products) {
            currentResults = products;
            resultsEl.innerHTML = '';
            emptyEl.style.display = products.length === 0 ? '' : 'none';
            products.forEach(function (p) {
                var row = document.createElement('tr');
                row.style.cursor = 'pointer';
                row.innerHTML = '<td>' + escapeHtml(p.stockCode || '') + '</td>' +
                    '<td>' + escapeHtml((p.barcodes && p.barcodes[0]) || '') + '</td>' +
                    '<td>' + escapeHtml(p.name) + '</td>' +
                    '<td>%' + p.taxRate + '</td>' +
                    '<td class="text-end">' + money(p.salePrice) + '</td>';
                row.addEventListener('click', function () { selectProduct(p); });
                resultsEl.appendChild(row);
            });
        }

        function selectProduct(p) {
            selected = p;
            nameEl.textContent = p.name;
            priceEl.textContent = money(p.salePrice);
            taxEl.textContent = '%' + p.taxRate;
            detailEl.style.display = '';
            addBtn.disabled = false;
        }

        function runSearch() {
            var term = searchEl.value.trim();
            if (!term) { renderResults([]); return; }
            fetch(searchUrl + '?term=' + encodeURIComponent(term) + '&mode=' + mode)
                .then(function (res) { return res.json(); })
                .then(function (data) { renderResults(data || []); })
                .catch(function () { renderResults([]); });
        }

        searchEl.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(runSearch, 250);
        });

        searchEl.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                clearTimeout(debounceTimer);
                if (currentResults.length === 1) { selectProduct(currentResults[0]); }
                else { runSearch(); }
            }
        });

        function setMode(newMode) {
            mode = newMode;
            nameBtn.classList.toggle('active', mode === 'name');
            barcodeBtn.classList.toggle('active', mode === 'barcode');
            runSearch();
        }
        nameBtn.addEventListener('click', function () { setMode('name'); });
        barcodeBtn.addEventListener('click', function () { setMode('barcode'); });

        function reset() {
            selected = null;
            detailEl.style.display = 'none';
            addBtn.disabled = true;
            searchEl.value = '';
            renderResults([]);
            setTimeout(function () { searchEl.focus(); }, 250);
        }

        addBtn.addEventListener('click', function () {
            if (!selected) return;
            addToCart(selected);
            var instance = bootstrap.Modal.getInstance(modalEl);
            if (instance) instance.hide();
        });

        modalEl.addEventListener('show.bs.modal', reset);
    })();

    // --- Ürün Listesi (madde 15-16, Edip 2026-09-04) - TAM stoktan arar (Fiyat Gör'ün aksine
    // sadece kısayol/kataloğa bağlı değil, sunucudaki SearchProducts full stok sorgusu). Ad/Barkod
    // modu arasında geçiş yapılabilir. Kategori alanına/sepete HİÇ dokunmaz - sadece seçilen ürünü
    // addToCart ile ekler (AYNI Fiyat Gör'ün kullandığı fonksiyon). ---
    (function () {
        var modalEl = document.getElementById('productListModal');
        if (!modalEl) return;
        var searchEl = document.getElementById('product-list-search');
        var resultsEl = document.getElementById('product-list-results');
        var emptyEl = document.getElementById('product-list-empty');
        var nameBtn = document.getElementById('product-list-mode-name');
        var barcodeBtn = document.getElementById('product-list-mode-barcode');
        var searchUrl = root.getAttribute('data-search-products-url');
        var mode = 'name';
        var debounceTimer = null;
        var currentResults = [];

        function renderResults(products) {
            currentResults = products;
            resultsEl.innerHTML = '';
            emptyEl.style.display = products.length === 0 ? '' : 'none';
            products.forEach(function (p) {
                var row = document.createElement('tr');
                row.style.cursor = 'pointer';
                row.innerHTML = '<td>' + escapeHtml(p.stockCode || '') + '</td>' +
                    '<td>' + escapeHtml((p.barcodes && p.barcodes[0]) || '') + '</td>' +
                    '<td>' + escapeHtml(p.name) + '</td>' +
                    '<td class="text-end">' + money(p.salePrice) + '</td>';
                row.addEventListener('click', function () { selectProduct(p); });
                resultsEl.appendChild(row);
            });
        }

        function selectProduct(p) {
            addToCart(p);
            var instance = bootstrap.Modal.getInstance(modalEl);
            if (instance) instance.hide();
        }

        function runSearch() {
            var term = searchEl.value.trim();
            if (!term) { renderResults([]); return; }
            fetch(searchUrl + '?term=' + encodeURIComponent(term) + '&mode=' + mode)
                .then(function (res) { return res.json(); })
                .then(function (data) { renderResults(data || []); })
                .catch(function () { renderResults([]); });
        }

        searchEl.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(runSearch, 250);
        });

        searchEl.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                clearTimeout(debounceTimer);
                if (currentResults.length === 1) {
                    selectProduct(currentResults[0]);
                } else {
                    runSearch();
                }
            }
        });

        function setMode(newMode) {
            mode = newMode;
            nameBtn.classList.toggle('active', mode === 'name');
            barcodeBtn.classList.toggle('active', mode === 'barcode');
            searchEl.placeholder = mode === 'barcode' ? 'Barkod okutun veya yazın...' : 'Ürün adı ara...';
            searchEl.value = '';
            renderResults([]);
            searchEl.focus();
        }
        nameBtn.addEventListener('click', function () { setMode('name'); });
        barcodeBtn.addEventListener('click', function () { setMode('barcode'); });

        modalEl.addEventListener('show.bs.modal', function () {
            setMode('name');
        });
        modalEl.addEventListener('shown.bs.modal', function () {
            // Ana barkod alanının aksine burada arama kutusuna odaklanmak GÜVENLİ - bu ayrı,
            // izole bir modal, fiziksel tarayıcıyı ana ekrandan hiç ayırmıyor.
            searchEl.focus();
        });
    })();

    // --- Klavye - dokunmatik ekranlarda fiziksel klavye olmadığı için (Edip, 2026-09-03:
    // "ekranlar dokunmatik olduğu için ona tıkladığım klavye açsın"). Son odaklanılan metin
    // alanına, imleç konumuna yazar. ---
    (function () {
        var keyboardEl = document.getElementById('pos-virtual-keyboard');
        var toggleBtn = document.getElementById('side-keyboard-btn');
        if (!keyboardEl || !toggleBtn) return;
        var activeTarget = null;
        var shiftOn = false;

        document.addEventListener('focusin', function (e) {
            var t = e.target;
            if (t && (t.tagName === 'INPUT' || t.tagName === 'TEXTAREA') && t.type !== 'hidden' && t.type !== 'checkbox') {
                activeTarget = t;
            }
        });

        function insertText(text) {
            if (!activeTarget) return;
            var start = activeTarget.selectionStart ?? activeTarget.value.length;
            var end = activeTarget.selectionEnd ?? activeTarget.value.length;
            var value = activeTarget.value;
            activeTarget.value = value.slice(0, start) + text + value.slice(end);
            var caret = start + text.length;
            activeTarget.setSelectionRange(caret, caret);
            activeTarget.dispatchEvent(new Event('input', { bubbles: true }));
            activeTarget.focus();
        }

        function backspace() {
            if (!activeTarget) return;
            var start = activeTarget.selectionStart ?? activeTarget.value.length;
            var end = activeTarget.selectionEnd ?? activeTarget.value.length;
            var value = activeTarget.value;
            if (start === end && start > 0) { start -= 1; }
            activeTarget.value = value.slice(0, start) + value.slice(end);
            activeTarget.setSelectionRange(start, start);
            activeTarget.dispatchEvent(new Event('input', { bubbles: true }));
            activeTarget.focus();
        }

        function applyShift() {
            keyboardEl.querySelectorAll('[data-key]').forEach(function (btn) {
                var key = btn.getAttribute('data-key');
                if (key.length === 1) btn.textContent = shiftOn ? key.toLocaleUpperCase('tr-TR') : key;
            });
        }

        keyboardEl.querySelectorAll('[data-key]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var key = btn.getAttribute('data-key');
                insertText(shiftOn && key.length === 1 ? key.toLocaleUpperCase('tr-TR') : key);
                if (shiftOn) { shiftOn = false; applyShift(); }
            });
        });

        keyboardEl.querySelectorAll('[data-action="backspace"]').forEach(function (btn) {
            btn.addEventListener('click', backspace);
        });
        keyboardEl.querySelector('[data-action="shift"]').addEventListener('click', function () {
            shiftOn = !shiftOn;
            applyShift();
        });
        keyboardEl.querySelector('[data-action="close"]').addEventListener('click', function () {
            keyboardEl.style.display = 'none';
        });

        toggleBtn.addEventListener('click', function () {
            var opening = keyboardEl.style.display === 'none';
            keyboardEl.style.display = opening ? '' : 'none';
            // Ekran boyutu son kaydedilen konumdan bu yana küçülmüş olabilir (ör. farklı bir
            // monitör) - gizliyken offsetWidth/Height 0 döndüğü için ilk yüklemedeki clamp
            // güvenilir değildi, açılırken YENİDEN clamp edilir.
            if (opening && typeof applyState === 'function') applyState(currentState());
        });

        // Serbest sürükleme + kademeli boyutlandırma + kalıcı hatırlama (2026-09-05 teknik
        // doküman madde 7: "fare/dokunma ile serbestçe sürüklenebilecek; ekran sınırlarının
        // dışına çıkamayacak", "+ ve - boyut butonları daha büyük dokunma hedeflerine sahip
        // olacak", "son konum ve boyutunu kullanıcı/terminal bazında hatırlayacak"). Terminal
        // bazında hatırlama için localStorage kullanılıyor - sunucuya hiç gitmiyor, o yüzden
        // gerçekten "bu bilgisayar/terminal" bazlı.
        var VK_STATE_KEY = 'sahinsoft-pos-keyboard-state';
        var MIN_SCALE = 0.7, MAX_SCALE = 1.6, SCALE_STEP = 0.1;

        function loadState() {
            try { return JSON.parse(window.localStorage.getItem(VK_STATE_KEY) || 'null') || {}; }
            catch (e) { return {}; }
        }
        function saveState(state) {
            try { window.localStorage.setItem(VK_STATE_KEY, JSON.stringify(state)); } catch (e) { /* yoksay */ }
        }

        function clampPosition(left, top) {
            var maxLeft = Math.max(0, window.innerWidth - keyboardEl.offsetWidth);
            var maxTop = Math.max(0, window.innerHeight - keyboardEl.offsetHeight);
            return { left: Math.min(Math.max(left, 0), maxLeft), top: Math.min(Math.max(top, 0), maxTop) };
        }

        function applyState(state) {
            var scale = state.scale || 1;
            keyboardEl.style.setProperty('--vk-scale', scale);
            if (state.left != null && state.top != null) {
                var clamped = clampPosition(state.left, state.top);
                keyboardEl.style.left = clamped.left + 'px';
                keyboardEl.style.top = clamped.top + 'px';
                keyboardEl.style.bottom = 'auto';
            }
        }

        function currentState() {
            var rect = keyboardEl.getBoundingClientRect();
            var scale = parseFloat(keyboardEl.style.getPropertyValue('--vk-scale')) || 1;
            return { left: rect.left, top: rect.top, scale: scale };
        }

        applyState(loadState());

        keyboardEl.querySelector('[data-vk-action="size-down"]').addEventListener('click', function () {
            var s = currentState();
            s.scale = Math.max(MIN_SCALE, Math.round((s.scale - SCALE_STEP) * 10) / 10);
            applyState(s);
            saveState(s);
        });
        keyboardEl.querySelector('[data-vk-action="size-up"]').addEventListener('click', function () {
            var s = currentState();
            s.scale = Math.min(MAX_SCALE, Math.round((s.scale + SCALE_STEP) * 10) / 10);
            applyState(s);
            saveState(s);
        });

        var dragHandle = document.getElementById('pos-vk-drag-handle');
        var dragging = false, dragStartX = 0, dragStartY = 0, startLeft = 0, startTop = 0;

        function onDragStart(clientX, clientY) {
            dragging = true;
            var rect = keyboardEl.getBoundingClientRect();
            startLeft = rect.left;
            startTop = rect.top;
            dragStartX = clientX;
            dragStartY = clientY;
        }
        function onDragMove(clientX, clientY) {
            if (!dragging) return;
            var clamped = clampPosition(startLeft + (clientX - dragStartX), startTop + (clientY - dragStartY));
            keyboardEl.style.left = clamped.left + 'px';
            keyboardEl.style.top = clamped.top + 'px';
            keyboardEl.style.bottom = 'auto';
        }
        function onDragEnd() {
            if (!dragging) return;
            dragging = false;
            saveState(currentState());
        }

        dragHandle.addEventListener('pointerdown', function (e) {
            if (e.target.closest('button')) return; // +/-/✕ tuşları sürüklemeyi başlatmasın
            dragHandle.setPointerCapture(e.pointerId);
            onDragStart(e.clientX, e.clientY);
        });
        dragHandle.addEventListener('pointermove', function (e) { onDragMove(e.clientX, e.clientY); });
        dragHandle.addEventListener('pointerup', onDragEnd);
        dragHandle.addEventListener('pointercancel', onDragEnd);

        // Pencere yeniden boyutlanırsa (ör. tarayıcı boyutu değişirse) klavye ekran dışında
        // kalmasın diye konum yeniden clamp edilir.
        window.addEventListener('resize', function () {
            if (keyboardEl.style.display === 'none') return;
            applyState(currentState());
        });
    })();

    // --- Sol numpad - bekleyen miktarı belirler, ürün tıklandığında o miktarda eklenir
    // (Edip, 2026-09-03: eski POS ekranındaki P/rakam şeridi referansı). ---
    (function () {
        var numpadEl = document.getElementById('cart-numpad');
        if (!numpadEl) return;
        var pBtn = document.getElementById('numpad-p-btn');
        var digitGrids = numpadEl.querySelectorAll('.cart-numpad-grid');
        var portionGrid = document.getElementById('numpad-portion-grid');
        var pendingStr = '';
        var portionMode = false;

        function setPortionMode(on) {
            portionMode = on;
            pBtn.classList.toggle('active', on);
            portionGrid.style.display = on ? '' : 'none';
            digitGrids.forEach(function (g) { g.style.display = on ? 'none' : ''; });
            pendingStr = '';
        }

        pBtn.addEventListener('click', function () { setPortionMode(!portionMode); });

        numpadEl.querySelectorAll('[data-digit]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                pendingStr = (pendingStr + btn.getAttribute('data-digit')).replace(/^0+(?=\d)/, '');
                var n = parseInt(pendingStr, 10);
                if (!isNaN(n) && n > 0) setPendingQuantity(n);
            });
        });

        numpadEl.querySelectorAll('[data-portion]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                setPendingQuantity(parseFloat(btn.getAttribute('data-portion')));
                setPortionMode(false);
            });
        });

        document.getElementById('numpad-clear-btn').addEventListener('click', function () {
            pendingStr = '';
            setPendingQuantity(1);
            if (portionMode) setPortionMode(false);
        });

        document.getElementById('numpad-back-btn').addEventListener('click', function () {
            pendingStr = pendingStr.slice(0, -1);
            var n = parseInt(pendingStr, 10);
            setPendingQuantity(!isNaN(n) && n > 0 ? n : 1);
        });
    })();

    // --- Nakit satırının altındaki hızlı işlemler: Sipariş Sil / İndirim / Kapat
    // (Edip, 2026-09-03: "sağ taraf nakit butonun altına sipariş sil indirim kapat butonlarına
    // yanyana koy renkli olsun"). Sadece BEKLEYEN (henüz gönderilmemiş) sepeti etkiler - zaten
    // gönderilmiş satırlar için ayrı İptal butonu var. ---
    (function () {
        var clearBtn = document.getElementById('clear-cart-btn');
        var discountBtn = document.getElementById('ticket-discount-btn');
        var closeBtn = document.getElementById('close-check-btn');
        if (!clearBtn) return;

        clearBtn.addEventListener('click', function () {
            if (cart.length > 0) {
                window.posConfirm('Bekleyen sepetteki ' + cart.length + ' kalem silinsin mi?', function () {
                    cart = [];
                    selectedCartId = null;
                    renderCart();
                }, null, { danger: true });
                return;
            }
            // Bekleyen (henüz gönderilmemiş) sepet boşsa - ör. Ödemeyi Al'a girip Adisyona Dön
            // sonrası HER ŞEY zaten sunucuya gönderilmiş olur (Edip, 2026-09-05: "yetkisi varsa
            // sipariş sil her koşulda çalışsın, hangi aşamadan dönerse dönsün") - bu durumda
            // adisyondaki GÖNDERİLMİŞ satırları topluca iptal eden gerçek uç noktaya (ClearOrder)
            // düşülür, sessizce hiçbir şey yapmadan durulmaz.
            if (document.querySelectorAll('.cart-line.sent').length === 0) return;
            var requireReason = root.getAttribute('data-require-cancellation-reason') === 'true';
            var requireApproval = root.getAttribute('data-require-second-approval-cancel-line') === 'true';
            function submitClearOrder(reason) {
                window.requestApproverPin(requireApproval, function (pin) {
                    document.getElementById('clearOrderReason').value = reason;
                    document.getElementById('clearOrderApproverPin').value = pin || '';
                    document.getElementById('clear-order-form').submit();
                });
            }
            window.posConfirm('Adisyondaki TÜM ürünler silinecek ve adisyon kapanacak. Devam edilsin mi?', function () {
                if (requireReason) {
                    window.posPrompt('İptal gerekçesi:', '', function (reason) {
                        if (!reason) { window.posAlert('İptal gerekçesi zorunludur.'); return; }
                        submitClearOrder(reason);
                    });
                } else {
                    submitClearOrder('Kasiyer - Sipariş Sil');
                }
            }, null, { danger: true });
        });

        discountBtn.addEventListener('click', function () {
            if (window.openTicketDiscountModal) window.openTicketDiscountModal(false);
        });

        // Adisyonda hiçbir aktif ürün yoksa (hiç eklenmedi ya da hepsi iptal edildi) "Kapat"
        // masayı da boşaltır - "Masayı Boşalt" ile aynı işlem, self-pay-btn zaten aynı koşulla
        // (bekleyen + gönderilmiş toplam <= 0) disabled oluyor, o bayrağı tekrar kullanılır
        // (Edip, 2026-09-03: "kapat tuşuna bastığında masayı boşaltsın").
        closeBtn.addEventListener('click', function () {
            var payBtn = document.getElementById('self-pay-btn');
            if (payBtn && payBtn.disabled) {
                document.getElementById('void-empty-check-form').submit();
            } else {
                window.location.href = root.getAttribute('data-back-url');
            }
        });
    })();

    // --- İndirim modalı (adisyon TOPLAMINA tutar indirimi) - "İndirim" butonuna tıklayınca
    // (Edip, 2026-09-03: "indirim butonuna tıkladığımda böyle küçük bir pencere açılsın yüzdeler
    // ve tutarsal indirim yapma yapılsın") yüzde/tutar modu + numpad + canlı önizleme. ÜSTTEKİ
    // (satır bazlı, cart-line-toolbar) İndirim'den AYRI: bu her zaman TOPLAM tutara uygulanır
    // (Edip: "üstte ürünü seçip indirim tuşuna bastığında satır indirimi, altta bastığında
    // indirim tuşuna tutar indirimi toplam tutara, bide tahsilatta indirim o da tutar indirimi
    // sayılsın"). Sepette bekleyen (henüz gönderilmemiş) satır varsa uygulamadan önce mutfağa
    // yazılır (ApplyDiscount yalnızca kalıcı RestaurantOrderLine'ları günceller), SONRA sunucuya
    // gönderilir ve sayfa yeniden yüklenir - Tahsilat akışından açıldıysa ?openPayment=1 ile. ---
    (function () {
        var modalEl = document.getElementById('ticketDiscountModal');
        if (!modalEl) return;
        var applyDiscountUrl = root.getAttribute('data-apply-discount-url');
        var requireApprovalDiscount = root.getAttribute('data-require-second-approval-discount') === 'true';
        // Sunucudaki ApplyTicketDiscountAsync bu tutarı SADECE RestaurantCheck.TicketDiscountAmount'a
        // yazar (2026-09-05 teknik doküman madde 4) - satırlara hiç dağıtılmaz/dokunulmaz. Önizleme
        // burada HAM brüt tutar (satırların kendi indirim/ikramları düşülmüş toplamı) üzerinden
        // hesaplanır - sunucudaki netLinesTotal ile AYNI taban.
        var sentLinesGrossTotal = parseFloat(root.getAttribute('data-sent-lines-gross-total')) || 0;
        var mode = 'percent';
        var digits = '';
        var fromPayment = false;

        function cartGrossTotal() {
            return cart.reduce(function (sum, l) { return sum + l.quantity * l.unitPrice; }, 0);
        }

        function grossTotal() {
            return cartGrossTotal() + sentLinesGrossTotal;
        }

        function enteredValue() {
            var n = parseFloat(digits.replace(',', '.'));
            return isNaN(n) ? 0 : n;
        }

        function computedDiscountAmount() {
            var gross = grossTotal();
            var raw = mode === 'percent' ? gross * (enteredValue() / 100) : enteredValue();
            return Math.max(0, Math.min(raw, gross));
        }

        function refreshPreview() {
            var gross = grossTotal();
            var discount = computedDiscountAmount();
            document.getElementById('discount-gross-total').textContent = money(gross);
            document.getElementById('discount-amount-preview').textContent = money(discount);
            document.getElementById('discount-net-total').textContent = money(gross - discount);
            document.getElementById('discount-entry-display').textContent = digits === '' ? '0' : digits;
            document.getElementById('discount-entry-label').textContent = mode === 'percent' ? 'GİRİLEN YÜZDE' : 'GİRİLEN TUTAR (₺)';
        }

        window.openTicketDiscountModal = function (isFromPayment) {
            if (grossTotal() <= 0) { window.posAlert('Adisyonda ürün yok.'); return; }
            fromPayment = !!isFromPayment;
            mode = 'percent';
            digits = '';
            modalEl.querySelectorAll('[data-discount-mode]').forEach(function (b) {
                b.classList.toggle('active', b.getAttribute('data-discount-mode') === mode);
            });
            refreshPreview();
            new bootstrap.Modal(modalEl).show();
        };

        var payDiscountBtn = document.getElementById('pay-discount-btn');
        if (payDiscountBtn) {
            payDiscountBtn.addEventListener('click', function () {
                var payModalEl = document.getElementById('closePaymentModal');
                var payModalInstance = payModalEl && bootstrap.Modal.getInstance(payModalEl);
                if (payModalInstance) payModalInstance.hide();
                window.openTicketDiscountModal(true);
            });
        }

        modalEl.querySelectorAll('[data-discount-mode]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                mode = btn.getAttribute('data-discount-mode');
                digits = '';
                modalEl.querySelectorAll('[data-discount-mode]').forEach(function (b) { b.classList.toggle('active', b === btn); });
                refreshPreview();
            });
        });

        modalEl.querySelectorAll('[data-percent]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                mode = 'percent';
                digits = btn.getAttribute('data-percent');
                modalEl.querySelectorAll('[data-discount-mode]').forEach(function (b) {
                    b.classList.toggle('active', b.getAttribute('data-discount-mode') === 'percent');
                });
                refreshPreview();
            });
        });

        modalEl.querySelectorAll('[data-dnum]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var v = btn.getAttribute('data-dnum');
                if (v === ',' && digits.indexOf(',') !== -1) return;
                if (digits === '0' && v !== ',') digits = '';
                digits += v;
                refreshPreview();
            });
        });

        document.getElementById('discount-num-clear').addEventListener('click', function () {
            digits = '';
            refreshPreview();
        });

        document.getElementById('discount-clear-btn').addEventListener('click', function () {
            mode = 'percent';
            digits = '';
            refreshPreview();
            submitDiscount(0);
        });

        function submitDiscount(amount) {
            var applyBtn = document.getElementById('discount-apply-btn');
            applyBtn.disabled = true;
            applyBtn.textContent = 'Uygulanıyor...';

            function fail() {
                applyBtn.disabled = false;
                applyBtn.textContent = 'Uygula';
            }

            flushCartToKitchen(
                function () {
                    window.requestApproverPin(requireApprovalDiscount, function (pin) {
                        fetch(applyDiscountUrl, {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-CSRF-TOKEN': getCsrfToken() },
                            body: 'checkId=' + encodeURIComponent(checkId) + '&amount=' + encodeURIComponent(amount) + '&approverPin=' + encodeURIComponent(pin || '')
                        })
                            .then(function (res) { return res.json(); })
                            .then(function (data) {
                                if (!data.success) {
                                    window.posAlert('Hata: ' + (data.message || 'İndirim uygulanamadı.'));
                                    fail();
                                    return;
                                }
                                var url = new URL(window.location.href);
                                if (fromPayment) url.searchParams.set('openPayment', '1');
                                window.location.href = url.toString();
                            })
                            .catch(function () {
                                window.posAlert('Bağlantı hatası oluştu.');
                                fail();
                            });
                    });
                },
                fail);
        }

        document.getElementById('discount-apply-btn').addEventListener('click', function () {
            submitDiscount(computedDiscountAmount());
        });
    })();

    // --- Ürün şablonu satır başlığı/satırları yatay kaydırma senkronu - çok sıkışık viewport'
    // larda (ör. %150 Windows ölçeklendirme) sütunlar sığmayıp bu üç ayrı kapsayıcı (başlık,
    // gönderilmiş satırlar, bekleyen satırlar) kendi içinde yatay kayabiliyor (bkz. CSS); AYRI
    // scroll konumları olduğu için biri kaydırılınca sütunlar hizasız kalırdı - burada üçü
    // birbirine senkronize edilir (Edip, 2026-09-04: "üst üste binmemeli"). ---
    (function () {
        var syncTargets = [
            document.getElementById('cart-lines'),
            document.getElementById('cart-sent-lines'),
            document.querySelector('.cart-lines-header-scroll')
        ].filter(Boolean);
        if (syncTargets.length < 2) return;
        var syncing = false;
        syncTargets.forEach(function (el) {
            el.addEventListener('scroll', function () {
                if (syncing) return;
                syncing = true;
                syncTargets.forEach(function (other) { if (other !== el) other.scrollLeft = el.scrollLeft; });
                syncing = false;
            });
        });
    })();

    // --- Fişi Beklet - bekleyen (henüz gönderilmemiş) sepet varsa ÖNCE mutfağa/sunucuya
    // gönderilir (flushCartToKitchen), SONRA adisyon Held olarak işaretlenir (bkz.
    // RestaurantSelfSaleController.Hold) - hiçbir şey sadece bu tarayıcıda kalmaz, başka bir
    // terminalden de Bekleyen Fişler'de görünür/geri çağrılabilir. ---
    (function () {
        var holdBtn = document.getElementById('side-hold-btn');
        if (!holdBtn) return;
        holdBtn.addEventListener('click', function () {
            if (!isSelfSale) {
                window.location.href = root.getAttribute('data-back-url');
                return;
            }
            holdBtn.disabled = true;
            flushCartToKitchen(
                function () { document.getElementById('hold-self-sale-form').submit(); },
                function () { holdBtn.disabled = false; window.posAlert('Sepet gönderilirken bağlantı hatası oluştu.'); });
        });
    })();

    // --- Bekleyen Fişler (2026-09-05 teknik doküman madde 8) - GERÇEK DB sorgusu
    // (RestaurantSelfSaleController.HeldReceipts), localStorage YOK - hangi terminalden bakılırsa
    // bakılsın aynı liste. Karta tıklamak Recall'a POST eder (HeldAtUtc temizlenir), sonra Check
    // ekranına döner - localStorage/?restoreHeld=1 kalıntısı YOK. ---
    (function () {
        var modalEl = document.getElementById('heldReceiptsModal');
        var badgeEl = document.getElementById('held-receipts-badge');
        var titleEl = document.getElementById('held-receipts-title');
        if (!modalEl) return;
        var listEl = document.getElementById('held-receipts-list');
        var heldReceiptsUrl = root.getAttribute('data-held-receipts-url');
        var recallUrl = root.getAttribute('data-recall-held-url');

        function waitLabel(heldAtIso) {
            var mins = Math.max(0, Math.floor((Date.now() - new Date(heldAtIso).getTime()) / 60000));
            if (mins < 1) return '<1 dk';
            if (mins < 60) return mins + ' dk';
            return Math.floor(mins / 60) + 's ' + (mins % 60) + 'dk';
        }
        function timeLabel(heldAtIso) {
            var d = new Date(heldAtIso);
            return String(d.getHours()).padStart(2, '0') + ':' + String(d.getMinutes()).padStart(2, '0');
        }

        function fetchList(onDone) {
            fetch(heldReceiptsUrl).then(function (r) { return r.json(); }).then(onDone).catch(function () { onDone([]); });
        }

        // Canlı sayaç (madde 18: "Bekleyen Fişler (8)" gibi) - sidebar rozeti sayfa yüklenişinde
        // ve her Fişi Beklet sonrası güncellenir. Artık GERÇEK bir sunucu sorgusu - başka bir
        // terminalde bekletilen bir fiş de bu sayaca yansır.
        function updateBadge() {
            fetchList(function (list) {
                if (badgeEl) {
                    badgeEl.textContent = String(list.length);
                    badgeEl.style.display = list.length > 0 ? '' : 'none';
                }
            });
        }
        updateBadge();
        window.updateHeldReceiptsBadge = updateBadge;

        function recall(checkId) {
            var fd = new FormData();
            fd.append('__RequestVerificationToken', getCsrfToken());
            fd.append('checkId', checkId);
            fetch(recallUrl, { method: 'POST', body: fd }).then(function () {
                window.location.href = '/Restaurant/Check/' + checkId;
            });
        }

        modalEl.addEventListener('show.bs.modal', function () {
            listEl.innerHTML = '<p class="text-secondary small p-2">Yükleniyor...</p>';
            fetchList(function (list) {
                updateBadge();
                if (titleEl) titleEl.textContent = 'Bekleyen Fişler (' + list.length + ')';
                if (list.length === 0) {
                    listEl.innerHTML = '<p class="text-secondary small p-2">Bekleyen fiş yok.</p>';
                    return;
                }
                // Sunucu zaten HeldAtUtc'ye göre en eski önce sıralı döner.
                listEl.innerHTML = '';
                list.forEach(function (entry, idx) {
                    var card = document.createElement('a');
                    card.href = '#';
                    card.className = 'held-receipt-card' + (idx === 0 ? ' oldest' : '');
                    card.innerHTML =
                        '<div class="held-receipt-card-top">' +
                            '<span class="held-receipt-card-number">' + escapeHtml(entry.checkNumber) + '</span>' +
                            '<span class="held-receipt-card-type">' + escapeHtml(entry.sourceLabel) + '</span>' +
                        '</div>' +
                        '<div class="held-receipt-card-meta">' +
                            '<span>' + timeLabel(entry.heldAtUtc) + '</span>' +
                            '<span>' + (idx === 0 ? '<span class="held-receipt-card-oldest-tag">EN ESKİ · </span>' : '') + waitLabel(entry.heldAtUtc) + '</span>' +
                        '</div>' +
                        '<div class="held-receipt-card-bottom">' +
                            '<span class="held-receipt-card-count">' + entry.itemCount + ' kalem</span>' +
                            '<span class="held-receipt-card-total">' + money(entry.total) + '</span>' +
                        '</div>';
                    card.addEventListener('click', function (e) { e.preventDefault(); recall(entry.checkId); });
                    listEl.appendChild(card);
                });
            });
        });
    })();

    // --- Cari Ekle (madde 13, Edip 2026-09-04, onaylı Self Satış tasarımı) - adisyona bir
    // müşteri bağlar; seçilince buton etiketi cari adını gösterir. "Açık Hesap" ödeme yöntemi
    // (restaurant-close-payment.js) bu değer dolu olmadan kullanılamaz. lookup-picker.js zaten
    // #AttachedCustomerId (hidden) + #attach-customer-btn (.lookup-trigger) ikilisini kendi
    // event delegation'ıyla yönetiyor - burada sadece seçim sonrası sunucuya kalıcı yazıyoruz. ---
    (function () {
        var hiddenInput = document.getElementById('AttachedCustomerId');
        var label = document.getElementById('attach-customer-label');
        if (!hiddenInput || !label) return;
        hiddenInput.addEventListener('lookup:selected', function (evt) {
            var item = evt.detail.item;
            fetch('/Restaurant/AttachCustomer', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-CSRF-TOKEN': getCsrfToken() },
                body: 'checkId=' + encodeURIComponent(checkId) + '&customerId=' + encodeURIComponent(hiddenInput.value)
            })
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (data.success) {
                        label.textContent = item.name || 'Cari Ekle';
                        if (window.RestaurantOpenAccountReady) window.RestaurantOpenAccountReady(true);
                    } else {
                        window.posAlert(data.message || 'Cari eklenemedi.');
                    }
                })
                .catch(function () { window.posAlert('Bağlantı hatası oluştu.'); });
        });
    })();

    // --- Fiş İkram (madde 11, Edip 2026-09-04) - adisyondaki TÜM ürünleri (gönderilmiş VE
    // bekleyen) sunucuda kalıcı olarak ikram eder, ciroya dahil edilmez. Administrator gerekçe
    // girmeden uygulayabilir, diğer yetkili kullanıcılar Kime/Neden doldurmak zorundadır -
    // bkz. ApplyReceiptComplimentaryAsync. Önce bekleyen sepet mutfağa/kalıcı satıra gönderilir
    // (flushCartToKitchen) ki hiçbir ürün ikramın dışında kalmasın. ---
    (function () {
        var btn = document.getElementById('side-ticket-comp-btn');
        var modalEl = document.getElementById('receiptComplimentaryModal');
        if (!btn || !modalEl) return;
        var applyUrl = root.getAttribute('data-apply-receipt-comp-url');
        var isAdmin = root.getAttribute('data-is-admin') === 'true';
        var errorEl = document.getElementById('comp-reason-error');
        var forInput = document.getElementById('comp-reason-for');
        var whyInput = document.getElementById('comp-reason-why');
        var noteInput = document.getElementById('comp-reason-note');

        function submitComplimentary() {
            var reasonFor = forInput.value.trim();
            var reasonWhy = whyInput.value.trim();
            if (!isAdmin && (!reasonFor || !reasonWhy)) {
                errorEl.textContent = 'Kime ve Neden alanları zorunludur.';
                errorEl.style.display = 'block';
                return;
            }
            var applyBtn = document.getElementById('comp-reason-apply-btn');
            applyBtn.disabled = true;
            errorEl.style.display = 'none';
            fetch(applyUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-CSRF-TOKEN': getCsrfToken() },
                body: 'checkId=' + encodeURIComponent(checkId) +
                    '&reasonFor=' + encodeURIComponent(reasonFor) +
                    '&reasonWhy=' + encodeURIComponent(reasonWhy) +
                    '&note=' + encodeURIComponent(noteInput.value.trim())
            })
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    applyBtn.disabled = false;
                    if (!data.success) {
                        errorEl.textContent = data.message || 'İkram uygulanamadı.';
                        errorEl.style.display = 'block';
                        return;
                    }
                    window.location.reload();
                })
                .catch(function () {
                    applyBtn.disabled = false;
                    errorEl.textContent = 'Bağlantı hatası oluştu.';
                    errorEl.style.display = 'block';
                });
        }

        document.getElementById('comp-reason-apply-btn').addEventListener('click', submitComplimentary);

        btn.addEventListener('click', function () {
            flushCartToKitchen(function () {
                forInput.value = '';
                whyInput.value = '';
                noteInput.value = '';
                errorEl.style.display = 'none';
                if (isAdmin) {
                    window.posConfirm('Adisyondaki TÜM ürünler ikram edilecek - ciroya dahil edilmeyecek. Devam edilsin mi?', function () {
                        new bootstrap.Modal(modalEl).show();
                    });
                } else {
                    new bootstrap.Modal(modalEl).show();
                }
            }, function () { window.posAlert('Sepet gönderilirken hata oluştu.'); });
        });
    })();

    // --- Fiş Listesi (2026-09-05 teknik doküman madde 9) - Excel-vari filtrelenebilir liste
    // (bkz. RestaurantController.FilteredReceipts). Satıra tıklamak (SADECE Administrator) eski
    // davranışı korur: "Ekrana Al" GERÇEK BİR DÜZELTME - kapanmış fiş muhasebe bütünlüğü gereği
    // yerinde değiştirilemez, önce fiş ters kayıtla iptal edilir (CancelReceiptForEdit), sonra
    // satırları sepete klonlanır ki kasiyer ürün ekleyip/silip yeniden ringleyebilsin. ---
    (function () {
        var modalEl = document.getElementById('receiptListModal');
        if (!modalEl) return;
        var listEl = document.getElementById('receipt-list-body');
        var emptyEl = document.getElementById('receipt-list-empty');
        var totalsEl = document.getElementById('receipt-list-totals');
        var fromEl = document.getElementById('receipt-list-from');
        var toEl = document.getElementById('receipt-list-to');
        var sourceEl = document.getElementById('receipt-list-source');
        var paymentEl = document.getElementById('receipt-list-payment');
        var statusEl = document.getElementById('receipt-list-status');
        var searchEl = document.getElementById('receipt-list-search');
        var clearBtn = document.getElementById('receipt-list-clear-btn');
        var exportBtn = document.getElementById('receipt-list-export-btn');
        var filteredReceiptsUrl = root.getAttribute('data-filtered-receipts-url');
        var receiptLinesUrl = root.getAttribute('data-receipt-lines-url');
        var cancelReceiptUrl = root.getAttribute('data-cancel-receipt-url');
        var isAdmin = root.getAttribute('data-is-admin') === 'true';
        var lastRows = [];

        function cloneLinesIntoCart(lines) {
            var missing = 0;
            lines.forEach(function (l) {
                var product = findCatalogProduct(l.productId);
                if (!product) { missing += 1; return; }
                cart.push({
                    cartId: ++cartSeq,
                    productId: product.productId,
                    productPortionId: null,
                    name: product.name,
                    portionName: null,
                    unitPrice: product.salePrice,
                    taxRate: product.taxRate,
                    quantity: l.quantity,
                    discountAmount: 0,
                    isComplimentary: false,
                    kitchenNote: null,
                    hasKitchenStation: product.hasKitchenStation
                });
            });
            renderCart();
            var instance = bootstrap.Modal.getInstance(modalEl);
            if (instance) instance.hide();
            if (missing > 0) window.posAlert(missing + ' kalem artık kataloğa dahil değil, eklenemedi.');
        }

        function todayIso() { return new Date().toISOString().slice(0, 10); }

        function loadList() {
            listEl.innerHTML = '<tr><td colspan="9" class="text-secondary small p-2">Yükleniyor...</td></tr>';
            var params = new URLSearchParams({
                from: fromEl.value || todayIso(),
                to: toEl.value || todayIso(),
                sourceType: sourceEl.value,
                payment: paymentEl.value,
                status: statusEl.value,
                q: searchEl.value
            });
            fetch(filteredReceiptsUrl + '?' + params.toString())
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    lastRows = data.rows || [];
                    // Ödeme türü filtresi SADECE bu tarih/kaynak filtresinde GERÇEKTEN var olan
                    // türlerden oluşur - hard-code liste değil (mevcut seçim korunur).
                    var currentPayment = paymentEl.value;
                    paymentEl.innerHTML = '<option value="">Tüm Ödemeler</option>';
                    (data.availablePaymentFilters || []).forEach(function (f) {
                        var opt = document.createElement('option');
                        opt.value = f.key; opt.textContent = f.label;
                        if (f.key === currentPayment) opt.selected = true;
                        paymentEl.appendChild(opt);
                    });
                    var t = data.totals || { count: 0, grandTotal: 0, cancelledCount: 0 };
                    totalsEl.textContent = t.count + ' fiş listeleniyor · Toplam ' + money(t.grandTotal) + (t.cancelledCount > 0 ? ' · ' + t.cancelledCount + ' iptal' : '');
                    emptyEl.style.display = lastRows.length === 0 ? '' : 'none';
                    listEl.innerHTML = '';
                    lastRows.forEach(function (r) {
                        var row = document.createElement('tr');
                        if (r.isCancelled) row.style.opacity = '0.6';
                        if (isAdmin && !r.isCancelled) row.style.cursor = 'pointer';
                        row.innerHTML = '<td>' + escapeHtml(r.dateLabel) + '</td>' +
                            '<td>' + escapeHtml(r.timeLabel) + '</td>' +
                            '<td>' + escapeHtml(r.documentNumber) + '</td>' +
                            '<td>' + escapeHtml(r.masaLabel) + '</td>' +
                            '<td>' + escapeHtml(r.customerName) + '</td>' +
                            '<td>' + escapeHtml(r.cashierName) + '</td>' +
                            '<td class="text-end">' + money(r.grandTotal) + '</td>' +
                            '<td>' + escapeHtml(r.paymentLabel) + '</td>' +
                            '<td>' + (r.isCancelled ? '<span class="text-danger">İptal</span>' : '<span class="text-success">Tamamlandı</span>') + '</td>';
                        if (!r.isCancelled) {
                            row.addEventListener('click', function () {
                                if (!isAdmin) {
                                    window.posAlert('Kapanmış bir fişi düzeltmek için yönetici yetkisi gerekir.');
                                    return;
                                }
                                window.posConfirm(r.documentNumber + ' fişi İPTAL EDİLİP satırları düzeltme için sepete eklenecek. Devam edilsin mi?', function () {
                                    var fd = new FormData();
                                    fd.append('__RequestVerificationToken', getCsrfToken());
                                    fd.append('retailSaleId', r.id);
                                    fetch(cancelReceiptUrl, { method: 'POST', body: fd })
                                        .then(function (resp) { return resp.json(); })
                                        .then(function (result) {
                                            if (!result.success) {
                                                window.posAlert(result.message || 'Fiş iptal edilemedi.');
                                                return;
                                            }
                                            fetch(receiptLinesUrl + '?retailSaleId=' + r.id).then(function (resp) { return resp.json(); }).then(cloneLinesIntoCart);
                                        });
                                }, null, { danger: true });
                            });
                        }
                        listEl.appendChild(row);
                    });
                });
        }

        [sourceEl, paymentEl, statusEl].forEach(function (el) { el.addEventListener('change', loadList); });
        var searchDebounce = null;
        searchEl.addEventListener('input', function () { clearTimeout(searchDebounce); searchDebounce = setTimeout(loadList, 300); });
        [fromEl, toEl].forEach(function (el) { el.addEventListener('change', loadList); });

        clearBtn.addEventListener('click', function () {
            fromEl.value = todayIso();
            toEl.value = todayIso();
            sourceEl.value = 'all';
            paymentEl.value = '';
            statusEl.value = '';
            searchEl.value = '';
            loadList();
        });

        // Excel'e Aktar - gerçek CSV (Excel bunu doğrudan açar), sadece o an FİLTRELENMİŞ
        // satırlar - kategori raporlarındaki export ile AYNI desen (data-csv-export kullanmıyor
        // çünkü bu tablo her açılışta yeniden çiziliyor, statik <table> değil).
        exportBtn.addEventListener('click', function () {
            var header = ['Tarih', 'Saat', 'Fiş No', 'Masa/Kaynak', 'Müşteri', 'Kasiyer', 'Tutar', 'Ödeme Türü', 'Durum'];
            var lines = [header.join(';')];
            lastRows.forEach(function (r) {
                lines.push([r.dateLabel, r.timeLabel, r.documentNumber, r.masaLabel, r.customerName, r.cashierName,
                    r.grandTotal.toFixed(2).replace('.', ','), r.paymentLabel, r.isCancelled ? 'İptal' : 'Tamamlandı']
                    .map(function (v) { return '"' + String(v).replace(/"/g, '""') + '"'; }).join(';'));
            });
            var blob = new Blob(['﻿' + lines.join('\r\n')], { type: 'text/csv;charset=utf-8;' });
            var url = URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.href = url;
            a.download = 'fis-listesi-' + (fromEl.value || todayIso()) + '.csv';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        });

        modalEl.addEventListener('show.bs.modal', function () {
            if (!fromEl.value) fromEl.value = todayIso();
            if (!toEl.value) toEl.value = todayIso();
            loadList();
        });
    })();

    renderCategories();
    renderCart();
})();
