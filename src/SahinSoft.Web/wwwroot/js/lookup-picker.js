(function () {
    "use strict";

    var modalEl = null;
    var bsModal = null;
    var activeTrigger = null;
    var searchTimer = null;
    var searchSeq = 0;

    function ensureModal() {
        if (modalEl) return;
        modalEl = document.getElementById("lookupModal");
        if (!modalEl) return;
        bsModal = new bootstrap.Modal(modalEl);

        modalEl.querySelector(".lookup-modal-search").addEventListener("input", function () {
            scheduleSearch(this.value);
        });
        modalEl.querySelector(".lookup-modal-search").addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                e.preventDefault();
                clearTimeout(searchTimer);
                runSearch(this.value);
            }
        });
        modalEl.querySelector(".lookup-modal-results").addEventListener("click", function (e) {
            var row = e.target.closest("[data-lookup-item]");
            if (!row) return;
            selectItem(JSON.parse(row.getAttribute("data-lookup-item")));
        });
        modalEl.addEventListener("shown.bs.modal", function () {
            modalEl.querySelector(".lookup-modal-search").focus();
        });
    }

    function scheduleSearch(term) {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(function () { runSearch(term); }, 250);
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

    function renderResults(items) {
        var resultsEl = modalEl.querySelector(".lookup-modal-results");
        if (!items.length) {
            resultsEl.innerHTML = '<div class="text-center text-secondary py-3">Sonuç bulunamadı.</div>';
            return;
        }

        var html = '<table class="table table-sm table-hover mb-0"><thead><tr><th>Kod</th><th>Ad</th></tr></thead><tbody>';
        items.forEach(function (item) {
            html += '<tr data-lookup-item=\'' + JSON.stringify(item).replace(/'/g, "&#39;") + '\' style="cursor:pointer">' +
                '<td>' + escapeHtml(item.code || "") + '</td>' +
                '<td>' + escapeHtml(item.name || "") + '</td>' +
                '</tr>';
        });
        html += '</tbody></table>';
        resultsEl.innerHTML = html;
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
        modalEl.querySelector(".lookup-modal-title").textContent = trigger.getAttribute("data-lookup-title") || "Kayıt Seç";
        var searchInput = modalEl.querySelector(".lookup-modal-search");
        searchInput.value = "";
        modalEl.querySelector(".lookup-modal-results").innerHTML = "";
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

    // Quicksearch inputs: e.g. barcode scanner into product name/code field.
    // Enter or F9 (Mikro tarzı arama tuşu) tetikler; tek eşleşme varsa otomatik seçilir (barkod davranışı).
    // Sonuna * konması da desteklenir (kozmetik, arama zaten "içerir" mantığında).
    document.addEventListener("keydown", function (e) {
        if (e.key !== "Enter" && e.key !== "F9") return;
        var input = e.target.closest(".lookup-quicksearch");
        if (!input) return;
        e.preventDefault();

        var endpoint = input.getAttribute("data-lookup-endpoint");
        var hiddenId = input.getAttribute("data-target-hidden");
        var hiddenEl = document.getElementById(hiddenId);
        var value = input.value.trim().replace(/\*+$/, "");
        if (!value) return;

        fetch(endpoint + "?q=" + encodeURIComponent(value))
            .then(function (r) { return r.json(); })
            .then(function (data) {
                var items = data.items || [];
                if (items.length === 1) {
                    applySelection(pseudoTriggerFor(input), items[0]);
                } else if (items.length > 1) {
                    var fakeTrigger = pseudoTriggerFor(input);
                    ensureModal();
                    activeTrigger = fakeTrigger;
                    modalEl.querySelector(".lookup-modal-title").textContent = input.getAttribute("data-lookup-title") || "Kayıt Seç";
                    modalEl.querySelector(".lookup-modal-search").value = value;
                    bsModal.show();
                    runSearch(value);
                }
            });
    });

    function pseudoTriggerFor(input) {
        return {
            getAttribute: function (name) { return input.getAttribute(name); }
        };
    }
})();
