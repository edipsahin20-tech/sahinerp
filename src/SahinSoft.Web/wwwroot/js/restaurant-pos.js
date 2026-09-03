(function () {
    'use strict';

    var root = document.getElementById('pos-root');
    if (!root) return;

    var checkId = parseInt(root.getAttribute('data-check-id'), 10);
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
            btn.textContent = cat.categoryName;
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

        if (cart.length === 0) {
            linesEl.innerHTML = '<p class="text-secondary small p-2">Ürün eklemek için sağdan seçim yapın.</p>';
            totalEl.textContent = money(0);
            sendBtn.disabled = true;
            return;
        }

        linesEl.innerHTML = '';
        var total = 0;
        cart.forEach(function (line) {
            total += lineTotal(line);
            var div = document.createElement('div');
            div.className = 'cart-line';
            var badges = '';
            if (line.isComplimentary) badges += ' <span class="badge text-bg-info-subtle text-info-emphasis">İKRAM</span>';
            else if (line.discountAmount > 0) badges += ' <span class="badge text-bg-warning-subtle text-warning-emphasis">İndirim ' + money(line.discountAmount) + '</span>';
            if (!line.hasKitchenStation) badges += ' <span class="badge text-bg-secondary-subtle" title="Mutfak istasyonu tanımlı değil">İstasyonsuz</span>';

            div.innerHTML =
                '<div>' +
                '  <div>' + line.quantity + 'x ' + escapeHtml(line.name) + (line.portionName ? ' (' + escapeHtml(line.portionName) + ')' : '') + badges + '</div>' +
                '  <div class="small text-secondary">' + money(lineTotal(line)) + (line.kitchenNote ? ' · Not: ' + escapeHtml(line.kitchenNote) : '') + '</div>' +
                '  <div class="line-toolbar">' +
                '    <button type="button" data-act="minus">-</button>' +
                '    <button type="button" data-act="plus">+</button>' +
                '    <button type="button" data-act="note">Not</button>' +
                '    <button type="button" data-act="discount">İndirim</button>' +
                '    <button type="button" data-act="comp">İkram</button>' +
                '    <button type="button" data-act="remove">Sil</button>' +
                '  </div>' +
                '</div>';

            div.querySelector('[data-act="minus"]').addEventListener('click', function () {
                line.quantity = Math.max(1, line.quantity - 1);
                renderCart();
            });
            div.querySelector('[data-act="plus"]').addEventListener('click', function () {
                line.quantity += 1;
                renderCart();
            });
            div.querySelector('[data-act="note"]').addEventListener('click', function () {
                var note = window.prompt('Not (mutfağa iletilecek):', line.kitchenNote || '');
                if (note !== null) line.kitchenNote = note.trim() || null;
                renderCart();
            });
            div.querySelector('[data-act="discount"]').addEventListener('click', function () {
                var amountStr = window.prompt('İndirim tutarı (₺):', line.discountAmount || 0);
                var amount = parseFloat(amountStr);
                if (!isNaN(amount) && amount >= 0) {
                    line.discountAmount = amount;
                    line.isComplimentary = false;
                }
                renderCart();
            });
            div.querySelector('[data-act="comp"]').addEventListener('click', function () {
                line.isComplimentary = !line.isComplimentary;
                renderCart();
            });
            div.querySelector('[data-act="remove"]').addEventListener('click', function () {
                cart = cart.filter(function (x) { return x.cartId !== line.cartId; });
                renderCart();
            });

            linesEl.appendChild(div);
        });

        totalEl.textContent = money(total);
        sendBtn.disabled = false;
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

    renderCategories();
    renderCart();
})();
