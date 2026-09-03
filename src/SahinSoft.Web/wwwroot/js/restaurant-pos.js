(function () {
    'use strict';

    var root = document.getElementById('pos-root');
    if (!root) return;

    var checkId = parseInt(root.getAttribute('data-check-id'), 10);
    var checkNumber = root.getAttribute('data-check-number');
    var tableLabel = root.getAttribute('data-table-label');
    var isSelfSale = root.getAttribute('data-is-self-sale') === 'true';
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

    // MASTER tasarımdaki "Ürün ara veya barkod okut..." kutusu - aktif kategoriden bağımsız,
    // TÜM kataloğu ada göre süzer (Edip, 2026-09-03: MASTER_SahinSoft_Restoran_POS_Premium.html).
    if (searchEl) {
        searchEl.addEventListener('input', function () {
            var term = searchEl.value.trim().toLocaleLowerCase('tr-TR');
            if (!term) {
                tabsEl.style.display = '';
                if (activeCategory) renderProducts(activeCategory);
                return;
            }
            tabsEl.style.display = 'none';
            var matches = [];
            catalog.forEach(function (cat) {
                cat.products.forEach(function (p) {
                    if (p.name.toLocaleLowerCase('tr-TR').indexOf(term) !== -1) matches.push(p);
                });
            });
            renderProductList(matches);
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
            quantity: 1,
            discountAmount: 0,
            isComplimentary: false,
            kitchenNote: null,
            hasKitchenStation: product.hasKitchenStation
        });
        selectedCartId = cartSeq;
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

        if (cart.length === 0) {
            linesEl.innerHTML = '<p class="text-secondary small p-2">Ürün eklemek için soldan seçim yapın.</p>';
            totalEl.textContent = money(0);
            sendBtn.disabled = true;
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

            div.innerHTML =
                '<div>' +
                '  <div>' + line.quantity + 'x ' + escapeHtml(line.name) + (line.portionName ? ' (' + escapeHtml(line.portionName) + ')' : '') + badges + '</div>' +
                '  <div class="small text-secondary">' + money(line.unitPrice) + ' · Tutar: ' + money(lineTotal(line)) + (line.kitchenNote ? ' · Not: ' + escapeHtml(line.kitchenNote) : '') + '</div>' +
                '</div>';

            div.addEventListener('click', function () {
                selectedCartId = line.cartId;
                renderCart();
            });

            linesEl.appendChild(div);
        });

        totalEl.textContent = money(total);
        sendBtn.disabled = false;
        updateLineToolbar();
    }

    // Sabit satır işlem çubuğunun durumu - seçili satır yoksa hepsi disabled, varsa miktar
    // ve butonlar seçili satırı yansıtır.
    function updateLineToolbar() {
        var line = selectedLine();
        var qtyValueEl = document.getElementById('line-qty-value');
        var buttons = [
            document.getElementById('line-qty-minus'),
            document.getElementById('line-qty-plus'),
            document.getElementById('line-act-note'),
            document.getElementById('line-act-discount'),
            document.getElementById('line-act-comp'),
            document.getElementById('line-act-remove')
        ];
        buttons.forEach(function (btn) { btn.disabled = !line; });
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
    document.getElementById('line-act-note').addEventListener('click', function () {
        var line = selectedLine();
        if (!line) return;
        var note = window.prompt('Not (mutfağa iletilecek):', line.kitchenNote || '');
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

        keyboardEl.querySelector('[data-action="backspace"]').addEventListener('click', backspace);
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

    // --- Fiş İkram - sepetteki TÜM (henüz gönderilmemiş) satırları tek tuşla ikram yapar,
    // satır bazlı İkram butonu zaten var (sabit çubuk), bu onun toplu hali. ---
    (function () {
        var btn = document.getElementById('side-ticket-comp-btn');
        if (!btn) return;
        btn.addEventListener('click', function () {
            if (cart.length === 0) return;
            if (!window.confirm('Sepetteki ' + cart.length + ' kalemin tamamı ikram olarak işaretlensin mi?')) return;
            cart.forEach(function (line) { line.isComplimentary = true; });
            renderCart();
        });
    })();

    // --- Fiş Listesi - bugün kesilmiş fişler, "Ekrana Al" ile satırları sepete klonlar. ---
    (function () {
        var modalEl = document.getElementById('receiptListModal');
        if (!modalEl) return;
        var listEl = document.getElementById('receipt-list-body');
        var todayReceiptsUrl = root.getAttribute('data-today-receipts-url');
        var receiptLinesUrl = root.getAttribute('data-receipt-lines-url');

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
                        if (!window.confirm(r.documentNumber + ' fişinin satırları sepete eklensin mi?')) return;
                        fetch(receiptLinesUrl + '?retailSaleId=' + r.id).then(function (resp) { return resp.json(); }).then(function (lines) {
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
