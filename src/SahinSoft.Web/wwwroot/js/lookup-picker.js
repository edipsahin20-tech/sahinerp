(function () {
    "use strict";

    var modalEl = null;
    var bsModal = null;
    var activeTrigger = null;
    var searchTimer = null;
    var searchSeq = 0;
    var currentItems = [];
    var highlightIndex = -1;

    // --- Yazarken anında öneri açılır listesi (Teklif Stüdyosu'ndaki F9 aramasıyla aynı his) ---
    var quickDropdown = null;
    var quickDropdownInput = null;
    var quickDropdownItems = [];
    var quickDropdownHighlight = -1;
    var quickSearchTimer = null;
    var quickSearchSeq = 0;

    function ensureQuickDropdown() {
        if (quickDropdown) return;
        quickDropdown = document.createElement("div");
        quickDropdown.className = "lookup-quick-dropdown";
        document.body.appendChild(quickDropdown);
        quickDropdown.addEventListener("mousedown", function (e) { e.preventDefault(); });
        quickDropdown.addEventListener("click", function (e) {
            var row = e.target.closest("[data-idx]");
            if (!row || !quickDropdownInput) return;
            var item = quickDropdownItems[Number(row.getAttribute("data-idx"))];
            if (item) applySelection(pseudoTriggerFor(quickDropdownInput), item);
            hideQuickDropdown();
        });
    }

    function positionQuickDropdown(input) {
        var rect = input.getBoundingClientRect();
        quickDropdown.style.left = (rect.left + window.scrollX) + "px";
        quickDropdown.style.top = (rect.bottom + window.scrollY + 2) + "px";
        quickDropdown.style.width = Math.max(rect.width, 260) + "px";
    }

    function hideQuickDropdown() {
        if (quickDropdown) quickDropdown.style.display = "none";
        quickDropdownItems = [];
        quickDropdownHighlight = -1;
        quickDropdownInput = null;
    }

    function quickItemSubtitle(item) {
        if (item.category) return item.category;
        if (item.taxOffice) return item.taxOffice;
        return "";
    }

    function quickItemAmount(item) {
        if (item.salePrice != null) return formatMoney(item.salePrice) + " ₺";
        if (item.debit != null || item.credit != null) {
            var bal = Math.max((item.debit || 0) - (item.credit || 0), 0);
            return bal > 0 ? formatMoney(bal) + " ₺ borç" : "";
        }
        return "";
    }

    function renderQuickDropdown(input, items) {
        quickDropdownInput = input;
        quickDropdownItems = items;
        quickDropdownHighlight = items.length > 0 ? 0 : -1;
        ensureQuickDropdown();

        quickDropdown.innerHTML = items.length === 0
            ? '<div class="lookup-quick-empty">Sonuç bulunamadı.</div>'
            : items.map(function (item, idx) {
                var sub = quickItemSubtitle(item);
                var amount = quickItemAmount(item);
                return '<div class="lookup-quick-item' + (idx === 0 ? " highlighted" : "") + '" data-idx="' + idx + '">' +
                    '<span class="lookup-quick-main">' +
                    '<span class="lookup-code-badge">' + escapeHtml(item.code || "") + '</span> ' +
                    '<span>' + escapeHtml(item.name || "") + '</span>' +
                    (sub ? ' <small class="text-secondary">' + escapeHtml(sub) + '</small>' : '') +
                    '</span>' +
                    (amount ? '<span class="lookup-quick-amount">' + amount + '</span>' : '') +
                    '</div>';
            }).join('');

        positionQuickDropdown(input);
        quickDropdown.style.display = "block";
    }

    function highlightQuickDropdown() {
        if (!quickDropdown) return;
        quickDropdown.querySelectorAll("[data-idx]").forEach(function (el) {
            el.classList.toggle("highlighted", Number(el.getAttribute("data-idx")) === quickDropdownHighlight);
        });
        var active = quickDropdown.querySelector(".highlighted");
        if (active) active.scrollIntoView({ block: "nearest" });
    }

    function ensureModal() {
        if (modalEl) return;
        modalEl = document.getElementById("lookupModal");
        if (!modalEl) return;
        bsModal = new bootstrap.Modal(modalEl);

        var searchInput = modalEl.querySelector(".lookup-modal-search");
        searchInput.addEventListener("input", function () {
            scheduleSearch(this.value);
        });
        searchInput.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                e.preventDefault();
                clearTimeout(searchTimer);
                if (highlightIndex >= 0 && currentItems[highlightIndex]) {
                    selectItem(currentItems[highlightIndex]);
                } else {
                    runSearch(this.value);
                }
                return;
            }
            if (e.key === "ArrowDown") {
                e.preventDefault();
                moveHighlight(1);
                return;
            }
            if (e.key === "ArrowUp") {
                e.preventDefault();
                moveHighlight(-1);
            }
        });
        modalEl.querySelector(".lookup-modal-results").addEventListener("click", function (e) {
            var row = e.target.closest("[data-lookup-idx]");
            if (!row) return;
            var item = currentItems[Number(row.getAttribute("data-lookup-idx"))];
            if (item) selectItem(item);
        });
        modalEl.querySelector(".lookup-modal-from").addEventListener("change", function () { runSearch(searchInput.value); });
        modalEl.querySelector(".lookup-modal-to").addEventListener("change", function () { runSearch(searchInput.value); });
        modalEl.addEventListener("shown.bs.modal", function () {
            modalEl.querySelector(".lookup-modal-search").focus();
        });
    }

    function scheduleSearch(term) {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(function () { runSearch(term); }, 250);
    }

    function currentMode() {
        var endpoint = (activeTrigger && activeTrigger.getAttribute("data-lookup-endpoint")) || "";
        if (endpoint.indexOf("/Lookup/Products") !== -1) return "product";
        if (endpoint.indexOf("/Lookup/Customers") !== -1) return "customer";
        if (endpoint.indexOf("/Lookup/TaxRates") !== -1) return "taxrate";
        if (endpoint.indexOf("/Lookup/InvoicesBrowse") !== -1) return "invoice";
        return "generic";
    }

    function runSearch(term) {
        if (!activeTrigger) return;
        var endpoint = activeTrigger.getAttribute("data-lookup-endpoint");
        var mySeq = ++searchSeq;
        var resultsEl = modalEl.querySelector(".lookup-modal-results");
        resultsEl.innerHTML = '<div class="text-center text-secondary py-3">Aranıyor...</div>';

        var url = endpoint + "?q=" + encodeURIComponent(term || "");
        if (currentMode() === "invoice") {
            var fromVal = modalEl.querySelector(".lookup-modal-from").value;
            var toVal = modalEl.querySelector(".lookup-modal-to").value;
            if (fromVal) url += "&from=" + encodeURIComponent(fromVal);
            if (toVal) url += "&to=" + encodeURIComponent(toVal);
        }

        fetch(url)
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (mySeq !== searchSeq) return;
                renderResults(data.items || []);
            })
            .catch(function () {
                if (mySeq !== searchSeq) return;
                resultsEl.innerHTML = '<div class="text-center text-danger py-3">Arama sırasında hata oluştu.</div>';
            });
    }

    function formatMoney(amount) {
        return Number(amount || 0).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // Gerçek miktar her zaman gösterilir (0, pozitif veya negatif olabilir) — "Stokta Yok" gibi
    // metinle maskelenmez; sadece sıfır/negatifse kırmızıyla vurgulanır.
    function stockCell(stock) {
        var qty = Number(stock || 0);
        var text = qty.toLocaleString('tr-TR', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
        if (qty <= 0) {
            return '<span style="color:#b91c1c;font-weight:600;">' + text + '</span>';
        }
        return text;
    }

    function renderResults(items) {
        currentItems = items;
        highlightIndex = items.length > 0 ? 0 : -1;

        var resultsEl = modalEl.querySelector(".lookup-modal-results");
        if (!items.length) {
            resultsEl.innerHTML = '<div class="text-center text-secondary py-3">Sonuç bulunamadı.</div>';
            return;
        }

        var mode = currentMode();
        var headHtml, rowsHtml;

        if (mode === "product") {
            headHtml = '<tr><th>Stok Kodu</th><th>Stok İsmi</th><th class="text-end">Miktar</th><th class="text-end">Son Alış Fiyatı</th><th class="text-end">Son Satış Fiyatı</th></tr>';
            rowsHtml = items.map(function (item, idx) {
                return '<tr data-lookup-idx="' + idx + '">' +
                    '<td><span class="lookup-code-badge">' + escapeHtml(item.code || '') + '</span></td>' +
                    '<td>' + escapeHtml(item.name || '') + '</td>' +
                    '<td class="text-end">' + stockCell(item.stock) + '</td>' +
                    '<td class="text-end">' + (item.lastPurchasePrice != null ? formatMoney(item.lastPurchasePrice) + ' ₺' : '-') + '</td>' +
                    '<td class="text-end">' + (item.lastSalePrice != null ? formatMoney(item.lastSalePrice) + ' ₺' : '-') + '</td>' +
                    '</tr>';
            }).join('');
        } else if (mode === "customer") {
            headHtml = '<tr><th>Cari Kodu</th><th>Cari İsmi</th><th class="text-end">Borç</th><th class="text-end">Alacak</th></tr>';
            rowsHtml = items.map(function (item, idx) {
                var debit = item.debit || 0, credit = item.credit || 0;
                return '<tr data-lookup-idx="' + idx + '">' +
                    '<td><span class="lookup-code-badge">' + escapeHtml(item.code || '') + '</span></td>' +
                    '<td>' + escapeHtml(item.name || '') + '</td>' +
                    '<td class="text-end">' + formatMoney(Math.max(debit - credit, 0)) + ' ₺</td>' +
                    '<td class="text-end">' + formatMoney(Math.max(credit - debit, 0)) + ' ₺</td>' +
                    '</tr>';
            }).join('');
        } else if (mode === "taxrate") {
            headHtml = '<tr><th>Kod</th><th>Ad</th><th class="text-end">Oran</th></tr>';
            rowsHtml = items.map(function (item, idx) {
                return '<tr data-lookup-idx="' + idx + '">' +
                    '<td><span class="lookup-code-badge">' + escapeHtml(item.code || '') + '</span></td>' +
                    '<td>' + escapeHtml(item.name || '') + '</td>' +
                    '<td class="text-end">%' + escapeHtml(String(item.rate != null ? item.rate : '')) + '</td>' +
                    '</tr>';
            }).join('');
        } else if (mode === "invoice") {
            headHtml = '<tr><th>Belge No</th><th>Cari</th><th>Tarih</th><th class="text-end">Tutar</th><th>Durum</th></tr>';
            rowsHtml = items.map(function (item, idx) {
                var statusClass = item.statusText === "Onaylı" ? "text-success" : (item.statusText === "İptal" ? "text-danger" : "text-secondary");
                return '<tr data-lookup-idx="' + idx + '">' +
                    '<td><span class="lookup-code-badge">' + escapeHtml(item.code || '') + '</span></td>' +
                    '<td>' + escapeHtml(item.customerName || '') + '</td>' +
                    '<td>' + escapeHtml(item.invoiceDate || '') + '</td>' +
                    '<td class="text-end">' + formatMoney(item.grandTotal) + ' ₺</td>' +
                    '<td class="' + statusClass + ' fw-semibold">' + escapeHtml(item.statusText || '') + '</td>' +
                    '</tr>';
            }).join('');
        } else {
            headHtml = '<tr><th>Kod</th><th>Ad</th></tr>';
            rowsHtml = items.map(function (item, idx) {
                return '<tr data-lookup-idx="' + idx + '">' +
                    '<td><span class="lookup-code-badge">' + escapeHtml(item.code || '') + '</span></td>' +
                    '<td>' + escapeHtml(item.name || '') + '</td>' +
                    '</tr>';
            }).join('');
        }

        resultsEl.innerHTML = '<table class="table table-sm table-hover mb-0 lookup-results-table"><thead>' + headHtml + '</thead><tbody>' + rowsHtml + '</tbody></table>';
        highlightRow();
    }

    function highlightRow() {
        var resultsEl = modalEl.querySelector(".lookup-modal-results");
        resultsEl.querySelectorAll("tr[data-lookup-idx]").forEach(function (row) {
            row.classList.toggle("highlighted", Number(row.getAttribute("data-lookup-idx")) === highlightIndex);
        });
        var activeRow = resultsEl.querySelector("tr.highlighted");
        if (activeRow) activeRow.scrollIntoView({ block: "nearest" });
    }

    function moveHighlight(delta) {
        if (currentItems.length === 0) return;
        highlightIndex = Math.max(0, Math.min(currentItems.length - 1, highlightIndex + delta));
        highlightRow();
    }

    function escapeHtml(s) {
        return String(s).replace(/[&<>"']/g, function (c) {
            return { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c];
        });
    }

    function selectItem(item) {
        if (!activeTrigger) return;
        applySelection(activeTrigger, item);
        if (bsModal) bsModal.hide();
    }

    function applySelection(trigger, item) {
        var hiddenId = trigger.getAttribute("data-target-hidden");
        var displayId = trigger.getAttribute("data-target-display");
        var hiddenEl = document.getElementById(hiddenId);
        var displayEl = document.getElementById(displayId);
        if (hiddenEl) hiddenEl.value = item.id;

        var row = (displayEl || hiddenEl) ? (displayEl || hiddenEl).closest("tr") : null;
        var hasStockCodeCol = row && row.querySelector(".stock-code-input");
        var endpoint = trigger.getAttribute("data-lookup-endpoint") || "";
        // Birim (UnitOfMeasure) seçicisinde Kod ve Ad genelde aynı kelime olduğu için ("Adet - Adet")
        // Kategori/Cari gibi "Kod - Ad" değil, sadece Ad gösterilir.
        var isUnitLookup = endpoint.indexOf("/Lookup/UnitsOfMeasure") !== -1;

        if (displayEl) {
            displayEl.value = (hasStockCodeCol || !item.code || isUnitLookup) ? (item.name || "") : ((item.code ? item.code + " - " : "") + item.name);
        }

        if (row) {
            var codeEl = row.querySelector(".stock-code-input");
            if (codeEl) codeEl.value = item.code || "";
            var nameEl = row.querySelector(".product-name-input");
            if (nameEl) nameEl.value = item.name || "";
        }

        var evt = new CustomEvent("lookup:selected", { bubbles: true, detail: { item: item, hiddenEl: hiddenEl, displayEl: displayEl, trigger: trigger } });
        (hiddenEl || trigger).dispatchEvent(evt);

        if (row) {
            var qtyInput = row.querySelector(".quantity-input");
            if (qtyInput) {
                setTimeout(function () {
                    qtyInput.focus();
                    qtyInput.select();
                }, 50);
            }
        }
    }

    function openModal(trigger) {
        ensureModal();
        if (!modalEl) return;
        activeTrigger = trigger;
        modalEl.querySelector(".lookup-modal-title").innerHTML = '<i class="fa-solid fa-magnifying-glass"></i> ' + escapeHtml(trigger.getAttribute("data-lookup-title") || "Kayıt Seç");
        var searchInput = modalEl.querySelector(".lookup-modal-search");
        searchInput.value = "";
        modalEl.querySelector(".lookup-modal-results").innerHTML = "";
        currentItems = [];
        highlightIndex = -1;

        var extraFilters = modalEl.querySelector(".lookup-modal-extra-filters");
        var isInvoiceMode = currentMode() === "invoice";
        extraFilters.style.display = isInvoiceMode ? "flex" : "none";
        if (isInvoiceMode) {
            modalEl.querySelector(".lookup-modal-from").value = "";
            modalEl.querySelector(".lookup-modal-to").value = "";
        }

        bsModal.show();
        runSearch("");
    }

    document.addEventListener("click", function (e) {
        var trigger = e.target.closest(".lookup-trigger");
        if (trigger) {
            e.preventDefault();
            openModal(trigger);
        }
    });

    // Yazarken anında (canlı) öneri listesi: 250ms debounce ile arama yapılır, açılır listede
    // gösterilir. Ok tuşlarıyla gezilir, Enter/tıklama ile seçilir.
    document.addEventListener("input", function (e) {
        var input = e.target.closest(".lookup-quicksearch");
        if (!input) return;
        clearTimeout(quickSearchTimer);
        var value = input.value.trim().replace(/\*+$/, "");
        if (!value) { hideQuickDropdown(); return; }

        quickSearchTimer = setTimeout(function () {
            var endpoint = input.getAttribute("data-lookup-endpoint");
            var mySeq = ++quickSearchSeq;
            fetch(endpoint + "?q=" + encodeURIComponent(value))
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    if (mySeq !== quickSearchSeq) return;
                    renderQuickDropdown(input, (data.items || []).slice(0, 8));
                })
                .catch(function () { hideQuickDropdown(); });
        }, 220);
    });

    document.addEventListener("focusout", function (e) {
        if (e.target.closest(".lookup-quicksearch")) {
            setTimeout(hideQuickDropdown, 150);
        }
    });

    // Quicksearch inputs: e.g. barcode scanner into product name/code field.
    // Açılır liste açıkken ok tuşları/Enter onu yönetir; F9 her zaman tam pencereyi açar (Mikro
    // tarzı arama tuşu). Açılır liste yoksa Enter, tek eşleşme varsa otomatik seçer (barkod davranışı),
    // birden fazla eşleşme varsa tam pencereyi açar. Sonuna * konması da desteklenir.
    document.addEventListener("keydown", function (e) {
        var input = e.target.closest(".lookup-quicksearch");
        if (!input) return;

        var dropdownOpen = quickDropdown && quickDropdown.style.display === "block" && quickDropdownItems.length > 0;
        if (dropdownOpen) {
            if (e.key === "ArrowDown") {
                e.preventDefault();
                quickDropdownHighlight = Math.min(quickDropdownItems.length - 1, quickDropdownHighlight + 1);
                highlightQuickDropdown();
                return;
            }
            if (e.key === "ArrowUp") {
                e.preventDefault();
                quickDropdownHighlight = Math.max(0, quickDropdownHighlight - 1);
                highlightQuickDropdown();
                return;
            }
            if (e.key === "Enter" && quickDropdownHighlight >= 0) {
                e.preventDefault();
                applySelection(pseudoTriggerFor(input), quickDropdownItems[quickDropdownHighlight]);
                hideQuickDropdown();
                return;
            }
            if (e.key === "Escape") {
                hideQuickDropdown();
                return;
            }
        }

        if (e.key !== "Enter" && e.key !== "F9") return;
        // Bu alan bir <form> içindeyse Enter'ın formu erken göndermesini her durumda engelle
        // (arama kutusu boş olsa bile) — aksi halde taslak satır girilirken kazara kaydedilir.
        e.preventDefault();
        var value = input.value.trim().replace(/\*+$/, "");
        if (!value) return;
        hideQuickDropdown();

        if (e.key === "Enter") {
            var endpoint = input.getAttribute("data-lookup-endpoint");
            fetch(endpoint + "?q=" + encodeURIComponent(value))
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    var items = data.items || [];
                    if (items.length === 1) {
                        applySelection(pseudoTriggerFor(input), items[0]);
                    } else if (items.length > 1) {
                        openFullSearch(input, value);
                    }
                });
            return;
        }

        openFullSearch(input, value);
    });

    function openFullSearch(input, value) {
        var fakeTrigger = pseudoTriggerFor(input);
        ensureModal();
        activeTrigger = fakeTrigger;
        modalEl.querySelector(".lookup-modal-title").innerHTML = '<i class="fa-solid fa-magnifying-glass"></i> ' + escapeHtml(input.getAttribute("data-lookup-title") || "Kayıt Seç");
        modalEl.querySelector(".lookup-modal-search").value = value || "";
        bsModal.show();
        runSearch(value || "");
    }

    function pseudoTriggerFor(input) {
        return {
            getAttribute: function (name) { return input.getAttribute(name); }
        };
    }
})();
