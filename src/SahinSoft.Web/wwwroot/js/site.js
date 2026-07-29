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
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('input[data-live-search]').forEach(initLiveListSearch);
    });
})();
