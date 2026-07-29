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
        return "generic";
    }

    function runSearch(term) {
        if (!activeTrigger) return;
        var endpoint = activeTrigger.getAttribute("data-lookup-endpoint");
        var mySeq = ++searchSeq;
        var resultsEl = modalEl.querySelector(".lookup-modal-results");
        resultsEl.innerHTML = '<div class="text-center text-secondary py-3">Aranıyor...</div>';

        fetch(endpoint + "?q=" + encodeURIComponent(term || ""))
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

    // Mikro tarzı: stok 0 ise arama sonucunda kırmızı "Stokta Yok" uyarısı gösterir (seçimi engellemez).
    function stockCell(stock) {
        var qty = Number(stock || 0);
        if (qty <= 0) {
            return '<span style="color:#b91c1c;font-weight:600;">Stokta Yok</span>';
        }
        return formatMoney(qty);
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
            headHtml = '<tr><th>Stok Kodu</th><th>Stok İsmi</th><th>Kategori</th><th class="text-end">Fiyat</th><th class="text-end">Miktar</th></tr>';
            rowsHtml = items.map(function (item, idx) {
                return '<tr data-lookup-idx="' + idx + '">' +
                    '<td><span class="lookup-code-badge">' + escapeHtml(item.code || '') + '</span></td>' +
                    '<td>' + escapeHtml(item.name || '') + '</td>' +
                    '<td>' + escapeHtml(item.category || '') + '</td>' +
                    '<td class="text-end">' + formatMoney(item.salePrice) + ' ₺</td>' +
                    '<td class="text-end">' + stockCell(item.stock) + '</td>' +
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
        if (displayEl) displayEl.value = (item.code ? item.code + " - " : "") + item.name;

        var evt = new CustomEvent("lookup:selected", { bubbles: true, detail: { item: item, hiddenEl: hiddenEl, displayEl: displayEl, trigger: trigger } });
        (hiddenEl || trigger).dispatchEvent(evt);
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
        var value = input.value.trim().replace(/\*+$/, "");
        if (!value) return;
        e.preventDefault();
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
