// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// --- Canlı liste arama: tüm arama kutuları için tek standart davranış ---
// data-live-search="#tabloId" işaretli input'lar, sayfa yenilenmeden yazdıkça
// o tablonun satırlarını anında filtreler (Teklif Stüdyosu'ndaki F9 aramasıyla aynı mantık).
(function () {
    function turkishNormalize(str) {
        return String(str || '')
            .replace(/İ/g, 'i').replace(/I/g, 'i').replace(/ı/g, 'i')
            .replace(/Ğ/g, 'g').replace(/ğ/g, 'g')
            .replace(/Ü/g, 'u').replace(/ü/g, 'u')
            .replace(/Ş/g, 's').replace(/ş/g, 's')
            .replace(/Ö/g, 'o').replace(/ö/g, 'o')
            .replace(/Ç/g, 'c').replace(/ç/g, 'c')
            .toLowerCase();
    }

    function initLiveListSearch(input) {
        const table = document.querySelector(input.dataset.liveSearch);
        if (!table) return;
        const tbody = table.tBodies[0];
        if (!tbody) return;

        const rows = Array.from(tbody.rows).filter(r => !r.hasAttribute('data-live-search-empty'));
        if (rows.length === 0) return;

        const emptyRow = document.createElement('tr');
        emptyRow.setAttribute('data-live-search-empty', '');
        emptyRow.style.display = 'none';
        const cell = document.createElement('td');
        cell.colSpan = rows[0].cells.length || 1;
        cell.className = 'text-center text-secondary py-4';
        cell.textContent = 'Aramanızla eşleşen kayıt bulunamadı.';
        emptyRow.appendChild(cell);
        tbody.appendChild(emptyRow);

        input.addEventListener('input', () => {
            const query = turkishNormalize(input.value.trim());
            let visibleCount = 0;
            rows.forEach(row => {
                const match = query === '' || turkishNormalize(row.textContent).includes(query);
                row.style.display = match ? '' : 'none';
                if (match) visibleCount++;
            });
            emptyRow.style.display = visibleCount === 0 ? '' : 'none';
            table.dispatchEvent(new CustomEvent('erp:rowschange'));
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('input[data-live-search]').forEach(initLiveListSearch);
    });
})();

// --- Sıralanabilir liste kolonları + toplam satırı: tüm liste sayfaları için tek standart ---
// <table data-sortable> içindeki <th data-sort="text|number"> başlıklarına tıklayınca o kolona
// göre satırlar sıralanır (tekrar tıklayınca ters çevirir). <tfoot> içindeki [data-sum-col]
// hücreleri, ilgili kolonun canlı aramayla eşleşen (arama kutusuyla gizlenmemiş) satırlar
// üzerinden toplamını gösterir — sayfalamadan bağımsızdır (tüm eşleşen sayfaların toplamı),
// ama arama filtresine göre her 'erp:rowschange' olayında yeniden hesaplanır.
(function () {
    // Sunucu tarafında toplam/tutar kolonları her zaman "N2" (en-US kültürü: virgül binlik,
    // nokta ondalık — bkz. Program.cs RequestLocalizationOptions) ile basılıyor; ayraçları buna
    // göre ayıklıyoruz, Türkçe (nokta binlik/virgül ondalık) değil.
    function parseNumber(text) {
        var cleaned = String(text || '')
            .replace(/,/g, '')
            .replace(/[^\d.-]/g, '');
        return parseFloat(cleaned) || 0;
    }

    // "dd.MM.yyyy" biçimindeki tarihleri karşılaştırılabilir bir sayıya (yyyyMMdd) çevirir.
    function parseDate(text) {
        var match = /(\d{1,2})\.(\d{1,2})\.(\d{4})/.exec(String(text || ''));
        if (!match) return 0;
        return parseInt(match[3] + match[2].padStart(2, '0') + match[1].padStart(2, '0'), 10);
    }

    function dataRows(tbody) {
        return Array.from(tbody.rows).filter(function (r) {
            return !r.hasAttribute('data-live-search-empty');
        });
    }

    function initSortableTable(table) {
        var tbody = table.tBodies[0];
        if (!tbody) return;

        table.querySelectorAll('thead th[data-sort]').forEach(function (th) {
            th.classList.add('erp-sortable-th');
            th.addEventListener('click', function () {
                var index = Array.from(th.parentNode.children).indexOf(th);
                var type = th.dataset.sort;
                var ascending = th.dataset.sortDir !== 'asc';

                table.querySelectorAll('thead th[data-sort]').forEach(function (h) {
                    h.removeAttribute('data-sort-dir');
                    h.classList.remove('sorted-asc', 'sorted-desc');
                });
                th.dataset.sortDir = ascending ? 'asc' : 'desc';
                th.classList.add(ascending ? 'sorted-asc' : 'sorted-desc');

                var rows = dataRows(tbody);
                rows.sort(function (a, b) {
                    var aCell = a.cells[index], bCell = b.cells[index];
                    var aText = aCell ? aCell.textContent.trim() : '';
                    var bText = bCell ? bCell.textContent.trim() : '';
                    if (type === 'number') {
                        return ascending ? parseNumber(aText) - parseNumber(bText) : parseNumber(bText) - parseNumber(aText);
                    }
                    if (type === 'date') {
                        return ascending ? parseDate(aText) - parseDate(bText) : parseDate(bText) - parseDate(aText);
                    }
                    return ascending ? aText.localeCompare(bText, 'tr') : bText.localeCompare(aText, 'tr');
                });
                rows.forEach(function (r) { tbody.appendChild(r); });
                table.dispatchEvent(new CustomEvent('erp:rowschange'));
            });
        });
    }

    function visibleDataRows(tbody) {
        return dataRows(tbody).filter(function (r) { return r.style.display !== 'none'; });
    }

    function initTotalsRow(table) {
        var tbody = table.tBodies[0];
        if (!tbody) return;
        var cells = table.querySelectorAll('tfoot [data-sum-col]');
        if (cells.length === 0) return;

        function recalc() {
            var rows = visibleDataRows(tbody);
            cells.forEach(function (cell) {
                var index = parseInt(cell.dataset.sumCol, 10);
                var sum = rows.reduce(function (acc, row) {
                    var rowCell = row.cells[index];
                    return acc + (rowCell ? parseNumber(rowCell.textContent) : 0);
                }, 0);
                // Toplam satırı da tablodaki diğer tutarlarla aynı (en-US/"N2") biçimde gösterilsin.
                cell.textContent = sum.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            });
        }

        table.addEventListener('erp:rowschange', recalc);
        recalc();
    }

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('table[data-sortable]').forEach(function (table) {
            initSortableTable(table);
            initTotalsRow(table);
        });
    });
})();

// --- Sekmeli kart formu (Stok/Cari Tanıtım Kartı): [data-tab-btn] tıklanınca aynı isimdeki
// [data-tab-panel]'i gösterir, diğerlerini gizler. Tek <form> içinde kaldığı için sekme
// değişimi hiçbir veriyi kaybetmez; sunucuya post edilen alanlar değişmez.
(function () {
    document.addEventListener('click', function (event) {
        var btn = event.target.closest('[data-tab-btn]');
        if (!btn) return;
        var group = btn.closest('.erp-tabs');
        if (!group) return;
        group.querySelectorAll('[data-tab-btn]').forEach(function (b) { b.classList.remove('active'); });
        btn.classList.add('active');

        var panelsContainer = group.parentElement;
        var name = btn.dataset.tabBtn;
        panelsContainer.querySelectorAll('[data-tab-panel]').forEach(function (panel) {
            panel.hidden = panel.dataset.tabPanel !== name;
        });
    });
})();

// --- Sayfalama: <table data-paginate="20"> aramaya/sıralamaya göre GERÇEKTEN filtrelenmiş
// satırları sayfalara böler (arama/sıralama modülleri 'erp:rowschange' olayını tetikler, bu
// modül onu dinleyip sayfayı yeniden hesaplar) — sabit/dekoratif bir sayfalama değildir. ---
(function () {
    // Aramanın gizlediği satırları tespit eder (inline style.display'e bakar); sayfalamanın
    // kendi gizlemesi ayrı bir CSS sınıfı (erp-page-hidden) kullanır, böylece ikisi çakışmaz.
    function searchVisibleRows(tbody) {
        return Array.from(tbody.rows).filter(function (r) {
            return !r.hasAttribute('data-live-search-empty') && r.style.display !== 'none';
        });
    }

    function initPagination(table) {
        var tbody = table.tBodies[0];
        if (!tbody) return;
        var pageSize = parseInt(table.dataset.paginate, 10) || 20;
        var footer = document.querySelector('[data-pagination-for="#' + table.id + '"]');
        var countLabel = document.querySelector('[data-pagination-count-for="#' + table.id + '"]');
        var currentPage = 1;

        function render() {
            var rows = searchVisibleRows(tbody);
            var totalPages = Math.max(1, Math.ceil(rows.length / pageSize));
            if (currentPage > totalPages) currentPage = totalPages;

            rows.forEach(function (row, index) {
                var page = Math.floor(index / pageSize) + 1;
                row.classList.toggle('erp-page-hidden', page !== currentPage);
            });

            if (countLabel) {
                countLabel.textContent = 'Toplam ' + rows.length + ' kayıt gösteriliyor';
            }
            if (footer) {
                var html = '';
                html += '<button type="button" data-page="prev"' + (currentPage <= 1 ? ' disabled' : '') + '>&lsaquo;</button>';
                for (var p = 1; p <= totalPages; p++) {
                    html += '<button type="button" data-page="' + p + '" class="' + (p === currentPage ? 'active' : '') + '">' + p + '</button>';
                }
                html += '<button type="button" data-page="next"' + (currentPage >= totalPages ? ' disabled' : '') + '>&rsaquo;</button>';
                footer.innerHTML = html;
            }
        }

        if (footer) {
            footer.addEventListener('click', function (event) {
                var btn = event.target.closest('button[data-page]');
                if (!btn) return;
                var rows = visibleDataRows(tbody);
                var totalPages = Math.max(1, Math.ceil(rows.length / pageSize));
                if (btn.dataset.page === 'prev') currentPage = Math.max(1, currentPage - 1);
                else if (btn.dataset.page === 'next') currentPage = Math.min(totalPages, currentPage + 1);
                else currentPage = parseInt(btn.dataset.page, 10);
                render();
            });
        }

        table.addEventListener('erp:rowschange', function () { currentPage = 1; render(); });
        render();
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('table[data-paginate]').forEach(initPagination);
    });
})();

// --- "Gelişmiş" filtre açma/kapama: [data-advanced-toggle] tıklanınca aynı konteynerdeki
// [data-advanced-filters] panelini açar/kapatır. Salt görsel; içindeki alanlar normal <form>
// elemanlarıdır, filtre mantığı sunucu tarafında route/query string ile çalışır. ---
(function () {
    document.addEventListener('click', function (event) {
        var btn = event.target.closest('[data-advanced-toggle]');
        if (!btn) return;
        var panel = document.querySelector(btn.dataset.advancedToggle);
        if (!panel) return;
        panel.classList.toggle('open');
        btn.classList.toggle('open', panel.classList.contains('open'));
    });
})();

// --- "Excel'e Aktar": [data-csv-export] tıklanınca hedef tablodaki GÖRÜNÜR (arama/sıralama
// sonrası) satırları gerçek bir .csv dosyası olarak indirir — Excel'de doğrudan açılır. ---
(function () {
    function tableToCsv(table) {
        var rows = [];
        var headerCells = Array.from(table.tHead ? table.tHead.rows[0].cells : []);
        rows.push(headerCells.map(function (c) { return csvCell(c.textContent); }).join(';'));

        Array.from(table.tBodies[0].rows).forEach(function (row) {
            if (row.hasAttribute('data-live-search-empty')) return;
            if (row.style.display === 'none') return;
            var cells = Array.from(row.cells).map(function (c) { return csvCell(c.textContent); });
            rows.push(cells.join(';'));
        });
        return rows.join('\r\n');
    }

    function csvCell(text) {
        var value = String(text || '').trim().replace(/\s+/g, ' ');
        return '"' + value.replace(/"/g, '""') + '"';
    }

    document.addEventListener('click', function (event) {
        var btn = event.target.closest('[data-csv-export]');
        if (!btn) return;
        var table = document.querySelector(btn.dataset.csvExport);
        if (!table) return;
        var csv = '﻿' + tableToCsv(table);
        var blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        var url = URL.createObjectURL(blob);
        var link = document.createElement('a');
        link.href = url;
        link.download = (btn.dataset.csvName || 'liste') + '.csv';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    });
})();
