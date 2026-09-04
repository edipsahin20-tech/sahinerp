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
            if (!pin) { window.alert('PIN girilmelidir.'); return; }
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

    // --- Fişi Beklet / Bekleyen Fişler - henüz mutfağa gönderilmemiş sepetler, sunucuya değil bu
    // tarayıcının localStorage'ına yazılır (Edip, 2026-09-03: "sağ tarafa fişi beklet, bekleyen
    // fişler"). Sayfa hangi adisyona (checkId) ait açılırsa, o adisyon için bekletilmiş bir sepet
    // varsa otomatik geri yüklenir - "Bekleyen Fişler" listesindeki bir satıra tıklamak zaten o
    // adisyonun URL'sine gider. ---
    var HELD_CARTS_KEY = 'sahinsoft-pos-held-carts';

    function readHeldCarts() {
        try {
            return JSON.parse(window.localStorage.getItem(HELD_CARTS_KEY) || '[]');
        } catch (e) {
            return [];
        }
    }

    function writeHeldCarts(list) {
        try {
            window.localStorage.setItem(HELD_CARTS_KEY, JSON.stringify(list));
        } catch (e) {
            // localStorage kullanılamıyorsa (gizli sekme vb.) sessizce yok say - bekletme
            // işlevi devre dışı kalır ama sayfa çalışmaya devam eder.
        }
    }

    function restoreHeldCartIfAny() {
        var list = readHeldCarts();
        var idx = -1;
        for (var i = 0; i < list.length; i++) { if (list[i].checkId === checkId) { idx = i; break; } }
        if (idx === -1) return;
        var held = list[idx];
        list.splice(idx, 1);
        writeHeldCarts(list);
        cart = held.cart || [];
        cart.forEach(function (line) { cartSeq = Math.max(cartSeq, line.cartId); });
    }

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

    function addToCart(product) {
        var portionId = null;
        var portionName = null;
        var unitPrice = product.salePrice;

        if (product.portions && product.portions.length > 0) {
            var names = product.portions.map(function (x, i) { return (i + 1) + ') ' + x.name; }).join('\n');
            var choice = window.prompt('Porsiyon seçin:\n' + names + '\n\n(Numara girin, boş bırakırsanız varsayılan porsiyon kullanılır)');
            var selected = null;
            if (choice && !isNaN(parseInt(choice, 10))) {
                selected = product.portions[parseInt(choice, 10) - 1];
            } else {
                selected = product.portions.find(function (x) { return x.isDefault; }) || product.portions[0];
            }
            if (selected) {
                portionId = selected.portionId;
                portionName = selected.name;
                unitPrice = selected.priceOverride != null ? selected.priceOverride : product.salePrice;
            }
        }

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

        if (cart.length === 0) {
            linesEl.innerHTML = '<p class="text-secondary small p-2">Ürün eklemek için soldan seçim yapın.</p>';
            totalEl.textContent = money(sentLinesTotal);
            sendBtn.disabled = true;
            if (payBtn) payBtn.disabled = sentLinesTotal <= 0;
            updateLineToolbar();
            return;
        }

        linesEl.innerHTML = '';
        var total = 0;
        cart.forEach(function (line) {
            total += lineTotal(line);
            var div = document.createElement('div');
            div.className = 'cart-line' + (line.cartId === selectedCartId ? ' selected' : '');
            var badges = '';
            if (line.isComplimentary) badges += ' <span class="badge text-bg-info-subtle text-info-emphasis">İKRAM</span>';
            else if (line.discountAmount > 0) badges += ' <span class="badge text-bg-warning-subtle text-warning-emphasis">İndirim ' + money(line.discountAmount) + '</span>';
            if (!line.hasKitchenStation) badges += ' <span class="badge text-bg-secondary-subtle" title="Mutfak istasyonu tanımlı değil">İstasyonsuz</span>';

            // Sabit şablon: Ürün Adı | Miktar | Birim | KDV | Fiyat | Tutar (Edip, 2026-09-03).
            div.innerHTML =
                '<div class="cart-line-col-name">' + escapeHtml(line.name) + (line.portionName ? ' (' + escapeHtml(line.portionName) + ')' : '') + badges +
                (line.kitchenNote ? '<div class="small text-secondary">Not: ' + escapeHtml(line.kitchenNote) + '</div>' : '') + '</div>' +
                '<div class="cart-line-col-qty">' + line.quantity + '</div>' +
                '<div class="cart-line-col-unit">' + escapeHtml(line.unit || 'Adet') + '</div>' +
                '<div class="cart-line-col-kdv">%' + line.taxRate + '</div>' +
                '<div class="cart-line-col-price">' + money(line.unitPrice) + '</div>' +
                '<div class="cart-line-col-total">' + money(lineTotal(line)) + '</div>' +
                '<div></div>';

            div.addEventListener('click', function () {
                selectedCartId = line.cartId;
                renderCart();
            });

            linesEl.appendChild(div);
        });

        totalEl.textContent = money(total + sentLinesTotal);
        sendBtn.disabled = false;
        if (payBtn) payBtn.disabled = (total + sentLinesTotal) <= 0;
        updateLineToolbar();
    }

    // Sabit satır işlem çubuğu - Edip, 2026-09-03: "mutfağa gönder dediğimizde bu butonlar
    // pasif oluyor artık olmasın hep aktif olsun ... sipariş sil butonu her zaman aktif olsun".
    // Butonlar ARTIK hiçbir zaman disabled olmuyor (mutfağa gönderilince sepet boşalıp seçili
    // satır kalmasa bile); her tıklama zaten kendi içinde "seçili satır yoksa hiçbir şey yapma"
    // koruması taşıyor (bkz. aşağıdaki click handler'ları), bu yüzden güvenli.
    function updateLineToolbar() {
        var line = selectedLine();
        var qtyValueEl = document.getElementById('line-qty-value');
        qtyValueEl.textContent = line ? line.quantity : '–';
    }

    document.getElementById('line-qty-minus').addEventListener('click', function () {
        var line = selectedLine();
        if (!line) return;
        line.quantity = Math.max(1, line.quantity - 1);
        renderCart();
    });
    document.getElementById('line-qty-plus').addEventListener('click', function () {
        var line = selectedLine();
        if (!line) return;
        line.quantity += 1;
        renderCart();
    });
    // Hazır not seçenekleri varsa (Edip, 2026-09-03: "otomatik not girilecek alanlar ekle")
    // porsiyon seçimindeki AYNI numaralı liste deseni - ayrı bir modal gerekmez, kasiyer numara
    // girip hazır notu seçebilir ya da kendi notunu yazabilir.
    var quickNotePresets = JSON.parse(document.getElementById('pos-quick-notes-data').textContent || '[]');
    document.getElementById('line-act-note').addEventListener('click', function () {
        var line = selectedLine();
        if (!line) return;
        var note;
        if (quickNotePresets.length > 0) {
            var options = quickNotePresets.map(function (n, i) { return (i + 1) + ') ' + n; }).join('\n');
            var choice = window.prompt('Not (mutfağa iletilecek):\n' + options + '\n\n(Hazır not için numara girin, ya da kendi notunuzu yazın)', line.kitchenNote || '');
            if (choice === null) { return; }
            var idx = parseInt(choice, 10);
            note = (!isNaN(idx) && idx >= 1 && idx <= quickNotePresets.length && String(idx) === choice.trim()) ? quickNotePresets[idx - 1] : choice;
        } else {
            note = window.prompt('Not (mutfağa iletilecek):', line.kitchenNote || '');
        }
        if (note !== null) line.kitchenNote = note.trim() || null;
        renderCart();
    });
    document.getElementById('line-act-discount').addEventListener('click', function () {
        var line = selectedLine();
        if (!line) return;
        var amountStr = window.prompt('İndirim tutarı (₺):', line.discountAmount || 0);
        var amount = parseFloat(amountStr);
        if (!isNaN(amount) && amount >= 0) {
            line.discountAmount = amount;
            line.isComplimentary = false;
        }
        renderCart();
    });
    document.getElementById('line-act-comp').addEventListener('click', function () {
        var line = selectedLine();
        if (!line) return;
        line.isComplimentary = !line.isComplimentary;
        renderCart();
    });
    document.getElementById('line-act-remove').addEventListener('click', function () {
        var line = selectedLine();
        if (!line) return;
        cart = cart.filter(function (x) { return x.cartId !== line.cartId; });
        selectedCartId = null;
        renderCart();
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

    document.getElementById('send-kitchen-btn').addEventListener('click', function () {
        if (cart.length === 0) return;
        var btn = this;
        btn.disabled = true;
        btn.textContent = 'Gönderiliyor...';
        flushCartToKitchen(
            function () { window.location.href = root.getAttribute('data-back-url'); },
            function () { btn.disabled = false; btn.textContent = 'Mutfağa Gönder'; });
    });

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

    // --- Fiyat Gör - müşteri kasada fiyat sorduğunda kataloğu tekrar sorgulamadan (zaten
    // yüklü) hızlı bakış (Edip, 2026-09-03). Seçilen ürün "Ekrana Al" ile sepete eklenir. ---
    (function () {
        var modalEl = document.getElementById('priceCheckModal');
        if (!modalEl) return;
        var searchEl = document.getElementById('price-check-search');
        var resultsEl = document.getElementById('price-check-results');
        var detailEl = document.getElementById('price-check-detail');
        var nameEl = document.getElementById('price-check-name');
        var priceEl = document.getElementById('price-check-price');
        var taxEl = document.getElementById('price-check-tax');
        var addBtn = document.getElementById('price-check-add-btn');
        var allProducts = [];
        catalog.forEach(function (cat) { cat.products.forEach(function (p) { allProducts.push(p); }); });
        var selected = null;

        function renderResults(products) {
            resultsEl.innerHTML = '';
            products.slice(0, 30).forEach(function (p) {
                var row = document.createElement('div');
                row.className = 'price-check-result-row';
                row.innerHTML = '<span>' + escapeHtml(p.name) + '</span><strong>' + money(p.salePrice) + '</strong>';
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

        function reset() {
            selected = null;
            detailEl.style.display = 'none';
            addBtn.disabled = true;
            searchEl.value = '';
            renderResults(allProducts);
        }

        searchEl.addEventListener('input', function () {
            var term = searchEl.value.trim().toLocaleLowerCase('tr-TR');
            renderResults(!term ? allProducts : allProducts.filter(function (p) {
                return p.name.toLocaleLowerCase('tr-TR').indexOf(term) !== -1;
            }));
        });

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
            keyboardEl.style.display = keyboardEl.style.display === 'none' ? '' : 'none';
        });

        // Dock (sol/sağ) + boyut kontrolü (madde 17: "taşınabilir - sol veya sağ", "kontrol
        // edilebilir şekilde yeniden boyutlandırılabilir"). Free-drag DEĞİL - bilinçli olarak
        // her zaman bir köşeye sabitlenir ki EKRAN DIŞINA HİÇ TAŞMASIN.
        var SIZES = ['sm', 'md', 'lg'];
        keyboardEl.querySelector('[data-vk-action="dock-left"]').addEventListener('click', function () {
            keyboardEl.setAttribute('data-dock', 'left');
        });
        keyboardEl.querySelector('[data-vk-action="dock-right"]').addEventListener('click', function () {
            keyboardEl.setAttribute('data-dock', 'right');
        });
        keyboardEl.querySelector('[data-vk-action="size-down"]').addEventListener('click', function () {
            var idx = Math.max(0, SIZES.indexOf(keyboardEl.getAttribute('data-size')) - 1);
            keyboardEl.setAttribute('data-size', SIZES[idx]);
        });
        keyboardEl.querySelector('[data-vk-action="size-up"]').addEventListener('click', function () {
            var idx = Math.min(SIZES.length - 1, SIZES.indexOf(keyboardEl.getAttribute('data-size')) + 1);
            keyboardEl.setAttribute('data-size', SIZES[idx]);
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
            if (cart.length === 0) return;
            if (!window.confirm('Bekleyen sepetteki ' + cart.length + ' kalem silinsin mi?')) return;
            cart = [];
            selectedCartId = null;
            renderCart();
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
        // Sunucudaki ApplyTicketDiscountAsync girilen tutarı satırların HAM brüt tutarına
        // (Quantity * UnitPriceSnapshot, önceki indirimler hariç) göre dağıtır ve önceki satır
        // indirimlerinin YERİNE geçer - bu yüzden önizleme de aynı HAM brüt tutarı esas almalı,
        // yoksa satırlarda önceden indirim varsa (örn. ikinci kez indirim uygulanırken) önizleme
        // ile gerçek sonuç uyuşmaz.
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
            if (grossTotal() <= 0) { window.alert('Adisyonda ürün yok.'); return; }
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
            flushCartToKitchen(
                function () {
                    fetch(applyDiscountUrl, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-CSRF-TOKEN': getCsrfToken() },
                        body: 'checkId=' + encodeURIComponent(checkId) + '&amount=' + encodeURIComponent(amount)
                    })
                        .then(function (res) { return res.json(); })
                        .then(function (data) {
                            if (!data.success) {
                                window.alert('Hata: ' + (data.message || 'İndirim uygulanamadı.'));
                                applyBtn.disabled = false;
                                applyBtn.textContent = 'Uygula';
                                return;
                            }
                            var url = new URL(window.location.href);
                            if (fromPayment) url.searchParams.set('openPayment', '1');
                            window.location.href = url.toString();
                        })
                        .catch(function () {
                            window.alert('Bağlantı hatası oluştu.');
                            applyBtn.disabled = false;
                            applyBtn.textContent = 'Uygula';
                        });
                },
                function () {
                    applyBtn.disabled = false;
                    applyBtn.textContent = 'Uygula';
                });
        }

        document.getElementById('discount-apply-btn').addEventListener('click', function () {
            submitDiscount(computedDiscountAmount());
        });
    })();

    // --- Gönderilmiş satırlarda miktar düzeltme / ikram - Edip, 2026-09-04, madde 9/21/25:
    // "satır kilidi TAMAMEN KALDIRILACAK ... kullanıcının düzenleme/miktar değiştirme/silme/
    // ikram/indirim/not/iptal yapabilmesini yetkiler belirleyecektir". Butonlar artık HER ZAMAN
    // görünür/tıklanabilir - yetki kontrolü sunucu tarafında (RestaurantPermissionService), izin
    // yoksa TempData["Error"] ile normal hata akışı üzerinden bildirilir. ---
    (function () {
        var requireApprovalEditKitchenSent = root.getAttribute('data-require-second-approval-edit-kitchen-sent') === 'true';
        var requireApprovalComplimentary = root.getAttribute('data-require-second-approval-complimentary') === 'true';

        document.querySelectorAll('.sent-line-qty-btn').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var current = btn.getAttribute('data-qty');
                var input = window.prompt('Yeni miktar:', current);
                if (input === null) return;
                var qty = parseFloat(input.replace(',', '.'));
                if (isNaN(qty) || qty <= 0) { window.alert('Geçersiz miktar.'); return; }
                window.requestApproverPin(requireApprovalEditKitchenSent, function (pin) {
                    document.getElementById('adjustQtyLineId').value = btn.getAttribute('data-line-id');
                    document.getElementById('adjustQtyValue').value = qty;
                    document.getElementById('adjustQtyApproverPin').value = pin || '';
                    document.getElementById('adjust-line-qty-form').submit();
                });
            });
        });

        document.querySelectorAll('.sent-line-comp-btn').forEach(function (btn) {
            btn.addEventListener('click', function () {
                if (!window.confirm('Bu satırın ikram durumu değiştirilsin mi?')) return;
                window.requestApproverPin(requireApprovalComplimentary, function (pin) {
                    document.getElementById('toggleCompLineId').value = btn.getAttribute('data-line-id');
                    document.getElementById('toggleCompApproverPin').value = pin || '';
                    document.getElementById('toggle-line-comp-form').submit();
                });
            });
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

    // --- Fişi Beklet ---
    (function () {
        var holdBtn = document.getElementById('side-hold-btn');
        if (!holdBtn) return;
        holdBtn.addEventListener('click', function () {
            if (cart.length > 0) {
                var list = readHeldCarts().filter(function (x) { return x.checkId !== checkId; });
                list.push({
                    checkId: checkId,
                    checkNumber: checkNumber,
                    tableLabel: tableLabel,
                    savedAtIso: new Date().toISOString(),
                    cart: cart
                });
                writeHeldCarts(list);
            }
            if (isSelfSale) {
                document.getElementById('hold-self-sale-form').submit();
            } else {
                window.location.href = root.getAttribute('data-back-url');
            }
        });
    })();

    // --- Bekleyen Fişler ---
    (function () {
        var modalEl = document.getElementById('heldReceiptsModal');
        if (!modalEl) return;
        var listEl = document.getElementById('held-receipts-list');

        function lineCount(entry) { return entry.cart.length; }
        function lineTotalSum(entry) {
            return entry.cart.reduce(function (sum, l) { return sum + lineTotal(l); }, 0);
        }

        modalEl.addEventListener('show.bs.modal', function () {
            var list = readHeldCarts();
            if (list.length === 0) {
                listEl.innerHTML = '<p class="text-secondary small p-2">Bekleyen fiş yok.</p>';
                return;
            }
            listEl.innerHTML = '';
            list.slice().reverse().forEach(function (entry) {
                var row = document.createElement('a');
                row.href = '/Restaurant/Check/' + entry.checkId;
                row.className = 'price-check-result-row';
                row.style.display = 'flex';
                row.style.textDecoration = 'none';
                row.style.color = 'inherit';
                row.innerHTML = '<span>' + escapeHtml(entry.checkNumber) + ' · ' + escapeHtml(entry.tableLabel) + ' <span class="text-secondary small">(' + lineCount(entry) + ' kalem)</span></span><strong>' + money(lineTotalSum(entry)) + '</strong>';
                listEl.appendChild(row);
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
                        window.alert(data.message || 'Cari eklenemedi.');
                    }
                })
                .catch(function () { window.alert('Bağlantı hatası oluştu.'); });
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
                if (isAdmin && !window.confirm('Adisyondaki TÜM ürünler ikram edilecek - ciroya dahil edilmeyecek. Devam edilsin mi?')) {
                    return;
                }
                new bootstrap.Modal(modalEl).show();
            }, function () { window.alert('Sepet gönderilirken hata oluştu.'); });
        });
    })();

    // --- Fiş Listesi - bugün kesilmiş fişler. "Ekrana Al" GERÇEK BİR DÜZELTME: kapanmış fiş
    // muhasebe bütünlüğü gereği yerinde değiştirilemez, bu yüzden önce fiş ters kayıtla iptal
    // edilir (Administrator gerektirir - bkz. CancelReceiptForEdit), sonra satırları sepete
    // klonlanır ki kasiyer ürün ekleyip/silip/ödeme tipini değiştirip yeniden ringleyebilsin
    // (Edip, 2026-09-03: "muhasebe mantığında öyle çalışsın ama restoran mantığında adisyonu
    // ekrana alıp düzenleyebilir/ürün ekleyip silebilir/ödeme tipini değiştirebilir"). ---
    (function () {
        var modalEl = document.getElementById('receiptListModal');
        if (!modalEl) return;
        var listEl = document.getElementById('receipt-list-body');
        var todayReceiptsUrl = root.getAttribute('data-today-receipts-url');
        var receiptLinesUrl = root.getAttribute('data-receipt-lines-url');
        var cancelReceiptUrl = root.getAttribute('data-cancel-receipt-url');
        var isAdmin = root.getAttribute('data-is-admin') === 'true';

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
            if (missing > 0) window.alert(missing + ' kalem artık kataloğa dahil değil, eklenemedi.');
        }

        modalEl.addEventListener('show.bs.modal', function () {
            listEl.innerHTML = '<p class="text-secondary small p-2">Yükleniyor...</p>';
            fetch(todayReceiptsUrl).then(function (r) { return r.json(); }).then(function (receipts) {
                if (receipts.length === 0) {
                    listEl.innerHTML = '<p class="text-secondary small p-2">Bugün kesilmiş fiş yok.</p>';
                    return;
                }
                listEl.innerHTML = '';
                receipts.forEach(function (r) {
                    var row = document.createElement('div');
                    row.className = 'price-check-result-row';
                    row.innerHTML = '<span>' + escapeHtml(r.timeLabel) + ' · ' + escapeHtml(r.documentNumber) + ' <span class="text-secondary small">(' + escapeHtml(r.tableName) + ')</span></span><strong>' + money(r.grandTotal) + '</strong>';
                    row.addEventListener('click', function () {
                        if (!isAdmin) {
                            window.alert('Kapanmış bir fişi düzeltmek için yönetici yetkisi gerekir.');
                            return;
                        }
                        if (!window.confirm(r.documentNumber + ' fişi İPTAL EDİLİP satırları düzeltme için sepete eklenecek. Devam edilsin mi?')) return;
                        var fd = new FormData();
                        fd.append('__RequestVerificationToken', getCsrfToken());
                        fd.append('retailSaleId', r.id);
                        fetch(cancelReceiptUrl, { method: 'POST', body: fd })
                            .then(function (resp) { return resp.json(); })
                            .then(function (result) {
                                if (!result.success) {
                                    window.alert(result.message || 'Fiş iptal edilemedi.');
                                    return;
                                }
                                fetch(receiptLinesUrl + '?retailSaleId=' + r.id).then(function (resp) { return resp.json(); }).then(cloneLinesIntoCart);
                            });
                    });
                    listEl.appendChild(row);
                });
            });
        });
    })();

    restoreHeldCartIfAny();
    renderCategories();
    renderCart();
})();
