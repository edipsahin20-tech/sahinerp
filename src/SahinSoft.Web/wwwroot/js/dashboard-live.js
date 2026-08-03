(() => {
    "use strict";

    // Bu ekran açıkken başka bir yerde (aynı kullanıcı, başka bir sekme, başka bir kullanıcı) yeni
    // bir hareket (fatura, tahsilat/tediye, sipariş...) kaydedilirse, sayfa elle yenilenmeden
    // kartlar/grafikler/listeler kendiliğinden güncellensin diye periyodik olarak aynı sayfa tekrar
    // sunucudan alınır ve içeriği değiştirilir. Küçük/az kullanıcılı bir iç uygulama için SignalR gibi
    // gerçek zamanlı bir altyapı orantısız olurdu — basit polling yeterli ve bakımı kolay.
    const POLL_INTERVAL_MS = 20000;
    const pageEl = document.querySelector(".dash-page");
    const chartDataEl = document.getElementById("dashboardChartData");
    if (!pageEl) return;

    let inFlight = false;

    function refresh() {
        if (inFlight || document.hidden) return;
        inFlight = true;
        fetch(location.href, { headers: { "X-Dashboard-Live-Refresh": "1" } })
            .then(response => response.ok ? response.text() : Promise.reject())
            .then(html => {
                const doc = new DOMParser().parseFromString(html, "text/html");
                const freshPage = doc.querySelector(".dash-page");
                const freshChartData = doc.getElementById("dashboardChartData");
                if (!freshPage) return;

                // Kullanıcı o an "Özel" tarih aralığı formunu dolduruyorsa üzerine yazma — bir sonraki
                // periyotta tekrar denenir.
                const customFormOpen = document.querySelector(".dash-custom-period.show");
                if (customFormOpen) return;

                pageEl.innerHTML = freshPage.innerHTML;
                if (chartDataEl && freshChartData) {
                    chartDataEl.textContent = freshChartData.textContent;
                }
                if (typeof window.initDashboardCharts === "function") {
                    window.initDashboardCharts();
                }
            })
            .catch(() => {})
            .finally(() => { inFlight = false; });
    }

    setInterval(refresh, POLL_INTERVAL_MS);
})();
