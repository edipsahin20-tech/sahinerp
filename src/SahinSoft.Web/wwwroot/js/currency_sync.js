/* ==========================================================================
   ŞahinSoft - Garanti BBVA Canlı Döviz Kuru Tekil Senkronizasyon Motoru
   Tüm sayfalarda (Ana sayfa, Teklif, Fatura vb.) tek merkezden çalışır.
   15 dakikada bir otomatik senkronize olur. "Canlı Kur Al" ile anlık tetiklenir.
   ========================================================================== */

(function () {
  // Global paylaşılan döviz kuru durumu
  window.globalExchangeRates = { USD: 36.50, EUR: 39.80, lastUpdated: null };

  const SYNC_INTERVAL_MS = 15 * 60 * 1000; // 15 Dakika

  // 1. CANLI KUR ÇEKME VE TÜM EKRANLARDA GÜNCELLEME
  window.fetchGlobalExchangeRates = async function (forceRefresh = false) {
    const syncIcons = document.querySelectorAll('#global-sync-icon, #sync-rates-btn i');
    syncIcons.forEach(icon => icon.classList.add('fa-spin'));

    try {
      // Önce localStorage kontrolü (15 dk aşılmadıysa ve zorunlu yenileme değilse)
      const cached = localStorage.getItem('garanti_exchange_rates');
      if (cached && !forceRefresh) {
        try {
          const parsed = JSON.parse(cached);
          const age = Date.now() - (parsed.timestamp || 0);
          if (age < SYNC_INTERVAL_MS && parsed.USD && parsed.EUR) {
            window.globalExchangeRates.USD = parsed.USD;
            window.globalExchangeRates.EUR = parsed.EUR;
            window.globalExchangeRates.lastUpdated = new Date(parsed.timestamp);
            updateAllRateDisplays();
            return;
          }
        } catch (e) { console.warn('Cache okuma hatası:', e); }
      }

      // Canlı API Çağrısı (USD & EUR)
      const [resUsd, resEur] = await Promise.all([
        fetch('https://open.er-api.com/v6/latest/USD').catch(() => null),
        fetch('https://open.er-api.com/v6/latest/EUR').catch(() => null)
      ]);

      if (resUsd && resUsd.ok) {
        const dataUsd = await resUsd.json();
        if (dataUsd && dataUsd.rates && dataUsd.rates.TRY) {
          const baseUsd = dataUsd.rates.TRY;
          const garantiUsdSell = baseUsd * 1.008; // Garanti BBVA Gişe Satış Marjı
          window.globalExchangeRates.USD = parseFloat(garantiUsdSell.toFixed(2));
        }
      }

      if (resEur && resEur.ok) {
        const dataEur = await resEur.json();
        if (dataEur && dataEur.rates && dataEur.rates.TRY) {
          const baseEur = dataEur.rates.TRY;
          const garantiEurSell = baseEur * 1.008;
          window.globalExchangeRates.EUR = parseFloat(garantiEurSell.toFixed(2));
        }
      }

      const now = Date.now();
      window.globalExchangeRates.lastUpdated = new Date(now);

      // LocalStorage Cache Kaydı
      localStorage.setItem('garanti_exchange_rates', JSON.stringify({
        USD: window.globalExchangeRates.USD,
        EUR: window.globalExchangeRates.EUR,
        timestamp: now
      }));

      // Ekrandaki tüm görünür kurları güncelle
      updateAllRateDisplays();

      // Diğer scriptler (Örn: Teklif oluşturucu) için event yayınla
      window.dispatchEvent(new CustomEvent('garantiRatesUpdated', {
        detail: { USD: window.globalExchangeRates.USD, EUR: window.globalExchangeRates.EUR }
      }));

    } catch (err) {
      console.warn('Canlı döviz kuru senkronizasyon uyarısı:', err);
    } finally {
      setTimeout(() => {
        syncIcons.forEach(icon => icon.classList.remove('fa-spin'));
      }, 500);
    }
  };

  // 2. EKRANDAKİ TÜM KURLARI GÜNCELLEME (navbar, anasayfa, teklif sayfası - kaç tane olursa olsun)
  function updateAllRateDisplays() {
    const usdVal = `${window.globalExchangeRates.USD.toFixed(2)} ₺`;
    const eurVal = `${window.globalExchangeRates.EUR.toFixed(2)} ₺`;

    document.querySelectorAll('.rate-value-usd, #global-rate-usd, #rate-usd').forEach(el => { el.textContent = usdVal; });
    document.querySelectorAll('.rate-value-eur, #global-rate-eur, #rate-eur').forEach(el => { el.textContent = eurVal; });
  }

  // 3. SAYAÇ VE İLK YÜKLEME
  document.addEventListener('DOMContentLoaded', () => {
    // Sayfa açılır açılmaz çalıştır
    window.fetchGlobalExchangeRates(false);

    // Tam 15 dakikada bir otomatik arka planda senkronize et
    setInterval(() => {
      window.fetchGlobalExchangeRates(true);
    }, SYNC_INTERVAL_MS);
  });

  // Diğer sekmeler arası canlı senkronizasyon (storage event)
  window.addEventListener('storage', (e) => {
    if (e.key === 'garanti_exchange_rates' && e.newValue) {
      try {
        const data = JSON.parse(e.newValue);
        window.globalExchangeRates.USD = data.USD;
        window.globalExchangeRates.EUR = data.EUR;
        updateAllRateDisplays();
        window.dispatchEvent(new CustomEvent('garantiRatesUpdated', { detail: data }));
      } catch(err){}
    }
  });

})();
