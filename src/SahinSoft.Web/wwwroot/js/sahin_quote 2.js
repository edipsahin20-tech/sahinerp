/* ==========================================================================
   admin.sahinbilisim.com.tr - TEKLİF & PDF PORTALI CORE ENGINE (app.js)
   Garanti BBVA & Live ASP.NET Core SQL Database Integration Engine
   ========================================================================== */

// --- GLOBAL STATE ---
let sahinCatalog = [];
let currentQuoteItems = [];
let savedProposals = [];
let exchangeRates = { USD: 36.50, EUR: 39.80 };
let activeCurrency = 'TRY';
let liveCustomers = [];

// INITIAL STOCK CATALOG SEEDED FROM sahinbilisim.com.tr
const DEFAULT_SAHIN_CATALOG = [
  { id: 'SHN-POS-101', dbId: 1, name: 'ŞahinSoft 15.6" Çift Ekran Dokunmatik POS PC (Intel i5, 8GB RAM, 128GB SSD)', category: 'Dokunmatik POS PC', unit: 'Adet', price: 18500.00, stock: 45, kdv: 20 },
  { id: 'SHN-POS-102', dbId: 2, name: 'ŞahinSoft Endüstriyel Dokunmatik Kasa POS PC Terminali (Kapasitif Dokunmatik)', category: 'Dokunmatik POS PC', unit: 'Adet', price: 15200.00, stock: 30, kdv: 20 },
  { id: 'SHN-TERM-201', dbId: 3, name: 'Android 11 Endüstriyel El Terminali (2D Zebra Barkod Okuyuculu, IP67)', category: 'El Terminalleri', unit: 'Adet', price: 21000.00, stock: 25, kdv: 20 },
  { id: 'SHN-TERM-202', dbId: 4, name: 'Android Restoran & Garson PDA Sipariş Terminali (Dahili Termal Yazıcılı)', category: 'Restoran PDA', unit: 'Adet', price: 12500.00, stock: 60, kdv: 20 },
  { id: 'SHN-BAR-301', dbId: 5, name: 'ŞahinSoft 2D Kablosuz Bluetooth Barkod Okuyucu (Şarj Stantlı)', category: 'Barkod Okuyucular', unit: 'Adet', price: 3400.00, stock: 120, kdv: 20 },
  { id: 'SHN-BAR-302', dbId: 6, name: 'Çok Yönlü Masaüstü OMNI Barkod Okuyucu (Market & Perakende)', category: 'Barkod Okuyucular', unit: 'Adet', price: 4200.00, stock: 85, kdv: 20 },
  { id: 'SHN-PRN-401', dbId: 7, name: '80mm Otomatik Kesmeli Termal Fiş Yazıcısı (USB + Ethernet + RS232)', category: 'Termal Yazıcılar', unit: 'Adet', price: 3800.00, stock: 95, kdv: 20 },
  { id: 'SHN-PRN-402', dbId: 8, name: 'Endüstriyel Barkod & Etiket Yazıcı (Termal Transfer / Direkt Termal)', category: 'Termal Yazıcılar', unit: 'Adet', price: 9500.00, stock: 40, kdv: 20 },
  { id: 'SHN-SCL-501', dbId: 9, name: 'Barkodlu Elektronik Terazi (30 kg Kapasite, Fiyat Hesablı)', category: 'Elektronik Teraziler', unit: 'Adet', price: 14500.00, stock: 20, kdv: 20 },
  { id: 'SHN-SOFT-601', dbId: 10, name: 'ŞahinSoft Perakende & Hızlı Satış Otomasyon Yazılımı (Süresiz Lisans)', category: 'Yazılımlar', unit: 'Lisans', price: 9500.00, stock: 999, kdv: 20 },
  { id: 'SHN-SOFT-602', dbId: 11, name: 'ŞahinSoft Depo WMS & Sevkiyat Yönetim Otomasyonu (El Terminali Modüllü)', category: 'Yazılımlar', unit: 'Lisans', price: 24000.00, stock: 999, kdv: 20 },
  { id: 'SHN-SOFT-603', dbId: 12, name: 'ŞahinSoft Restoran & Kafe Adisyon Yazılımı (Garson PDA & Mutfak Entegre)', category: 'Yazılımlar', unit: 'Lisans', price: 11000.00, stock: 999, kdv: 20 },
  { id: 'SHN-SOFT-604', dbId: 13, name: 'Mikro ERP / Ticari Yazılım Çift Yönlü Canlı Entegrasyon Modülü', category: 'Yazılımlar', unit: 'Lisans', price: 16500.00, stock: 999, kdv: 20 },
  { id: 'SHN-EDON-701', dbId: 14, name: 'GİB Uyumlu e-Fatura & e-Arşiv Entegrasyon Paketi (1.000 Kontör Dahil)', category: 'E-Dönüşüm', unit: 'Paket', price: 2500.00, stock: 999, kdv: 20 },
  { id: 'SHN-EDON-702', dbId: 15, name: 'e-İrsaliye ve e-Müstahsil Makbuzu Entegrasyon Modülü', category: 'E-Dönüşüm', unit: 'Paket', price: 3200.00, stock: 999, kdv: 20 },
  { id: 'SHN-ACC-801', dbId: 16, name: 'Ağır Hizmet Tipi Ağaç / Çelik Para Çekmecesi (5 Banknot, RJ11 Bağlantılı)', category: 'Dokunmatik POS PC', unit: 'Adet', price: 2100.00, stock: 110, kdv: 20 }
];

// INITIALIZE APP ON DOM READY
document.addEventListener('DOMContentLoaded', () => {
  initStorage();
  populateCategoryDropdowns();
  populateProductDropdown();
  generateNewQuoteNumber();
  setDefaultDates();
  
  // Seed initial sample proposal items for instant view
  seedSampleQuoteItems();
  
  // Fetch Garanti Bank / Live exchange rates automatically
  fetchExchangeRates();

  // Fetch Live ASP.NET Core SQL Database Products and Customers
  loadLiveDbCatalog();
  loadLiveDbCustomers();

  updateCalculations();
  updatePdfPreview();
  renderDashboardMetrics();

  // Auto refresh rates every 60 seconds
  setInterval(fetchExchangeRates, 60000);
});

// --- LIVE ASP.NET CORE SQL DATABASE FETCH ---
async function loadLiveDbCatalog() {
  try {
    const res = await fetch('/Quotes/GetCatalogDataApi');
    if (res.ok) {
      const data = await res.json();
      if (data && data.products && data.products.length > 0) {
        sahinCatalog = data.products;
        populateCategoryDropdowns();
        populateProductDropdown();
        renderStockManagementTable();
        renderDashboardMetrics();
      }
    }
  } catch (err) {
    console.log('ASP.NET SQL veritabanı stok servis bilgisi (fallback aktif):', err);
  }
}

async function loadLiveDbCustomers() {
  try {
    const res = await fetch('/Quotes/GetCustomersApi');
    if (res.ok) {
      const customers = await res.json();
      if (customers && customers.length > 0) {
        liveCustomers = customers;
        setupCustomerAutocomplete();
      }
    }
  } catch (err) {
    console.log('ASP.NET SQL veritabanı cari servis bilgisi (fallback aktif):', err);
  }
}

function setupCustomerAutocomplete() {
  const companyInput = document.getElementById('cust-company');
  if (!companyInput || liveCustomers.length === 0) return;

  companyInput.addEventListener('change', () => {
    const val = companyInput.value.toLowerCase();
    const found = liveCustomers.find(c => c.name.toLowerCase().includes(val) || c.company.toLowerCase().includes(val));
    if (found) {
      document.getElementById('cust-contact').value = found.contact || found.name;
      document.getElementById('cust-phone').value = found.phone || '';
      document.getElementById('cust-email').value = found.email || '';
      document.getElementById('cust-tax-office').value = found.taxOffice || '';
      document.getElementById('cust-address').value = found.address || '';
      document.getElementById('cust-company').dataset.customerId = found.id;
      updatePdfPreview();
    }
  });
}

// --- TEK MERKEZDEN SENKRONİZE GARANTİ BBVA DÖVİZ KURU SERVİSİ ---
function fetchExchangeRates() {
  if (typeof window.fetchGlobalExchangeRates === 'function') {
    window.fetchGlobalExchangeRates(true);
  }
}

// Global Garanti BBVA canlı kur güncelleme dinleyicisi (Tek merkezli senkronizasyon)
window.addEventListener('garantiRatesUpdated', (e) => {
  if (e.detail && e.detail.USD && e.detail.EUR) {
    exchangeRates.USD = e.detail.USD;
    exchangeRates.EUR = e.detail.EUR;

    const usdEl = document.getElementById('rate-usd');
    const eurEl = document.getElementById('rate-eur');
    if (usdEl) usdEl.textContent = `${exchangeRates.USD.toFixed(2)} ₺`;
    if (eurEl) eurEl.textContent = `${exchangeRates.EUR.toFixed(2)} ₺`;

    updateCalculations();
    updatePdfPreview();
  }
});

// --- 1. STORAGE & INIT FUNCTIONS ---
function initStorage() {
  const localCatalog = localStorage.getItem('sahin_admin_catalog');
  if (localCatalog) {
    try { sahinCatalog = JSON.parse(localCatalog); } catch(e) { sahinCatalog = [...DEFAULT_SAHIN_CATALOG]; }
  } else {
    sahinCatalog = [...DEFAULT_SAHIN_CATALOG];
    localStorage.setItem('sahin_admin_catalog', JSON.stringify(sahinCatalog));
  }

  const localHistory = localStorage.getItem('sahin_admin_proposals');
  if (localHistory) {
    try { savedProposals = JSON.parse(localHistory); } catch(e) { savedProposals = []; }
  } else {
    savedProposals = [];
  }
}

function reloadOriginalCatalog() {
  if (confirm('sahinbilisim.com.tr orijinal stok listesini varsayılana sıfırlamak istediğinize emin misiniz?')) {
    sahinCatalog = [...DEFAULT_SAHIN_CATALOG];
    localStorage.setItem('sahin_admin_catalog', JSON.stringify(sahinCatalog));
    populateCategoryDropdowns();
    populateProductDropdown();
    renderStockManagementTable();
    renderDashboardMetrics();
    alert('Stok kataloğu sahinbilisim.com.tr varsayılanları ile başarıyla güncellendi!');
  }
}

function generateNewQuoteNumber() {
  const year = new Date().getFullYear();
  const randomNum = Math.floor(1000 + Math.random() * 9000);
  const quoteNo = `TEK-${year}-${randomNum}`;
  document.getElementById('quote-no').value = quoteNo;
  document.getElementById('pdf-val-no').textContent = quoteNo;
}

function setDefaultDates() {
  const today = new Date().toISOString().split('T')[0];
  document.getElementById('quote-date').value = today;
  
  const formattedToday = new Date().toLocaleDateString('tr-TR');
  document.getElementById('pdf-val-date').textContent = formattedToday;
}

function seedSampleQuoteItems() {
  currentQuoteItems = [
    {
      id: 'SHN-POS-101',
      dbId: 1,
      name: 'ŞahinSoft 15.6" Çift Ekran Dokunmatik POS PC (Intel i5, 8GB RAM, 128GB SSD)',
      code: 'SHN-POS-101',
      unit: 'Adet',
      qty: 2,
      price: 18500.00,
      kdv: 20,
      discount: 5
    },
    {
      id: 'SHN-SOFT-601',
      dbId: 10,
      name: 'ŞahinSoft Perakende & Hızlı Satış Otomasyon Yazılımı (Süresiz Lisans)',
      code: 'SHN-SOFT-601',
      unit: 'Lisans',
      qty: 2,
      price: 9500.00,
      kdv: 20,
      discount: 10
    },
    {
      id: 'SHN-BAR-301',
      dbId: 5,
      name: 'ŞahinSoft 2D Kablosuz Bluetooth Barkod Okuyucu (Şarj Stantlı)',
      code: 'SHN-BAR-301',
      unit: 'Adet',
      qty: 2,
      price: 3400.00,
      kdv: 20,
      discount: 0
    }
  ];
}

// --- 2. DROPDOWN & CATALOG POPULATION ---
function populateCategoryDropdowns() {
  const catSelect = document.getElementById('picker-category');
  const categories = [...new Set(sahinCatalog.map(item => item.category))];

  catSelect.innerHTML = '<option value="ALL">Tüm Kategoriler (sahinbilisim.com.tr)</option>';
  categories.forEach(cat => {
    const opt = document.createElement('option');
    opt.value = cat;
    opt.textContent = cat;
    catSelect.appendChild(opt);
  });
}

function populateProductDropdown(filteredCategory = 'ALL') {
  const prodSelect = document.getElementById('picker-product');
  prodSelect.innerHTML = '<option value="">-- Lütfen Ürün Seçiniz --</option>';

  const items = filteredCategory === 'ALL' 
    ? sahinCatalog 
    : sahinCatalog.filter(i => i.category === filteredCategory);

  items.forEach(item => {
    const opt = document.createElement('option');
    opt.value = item.id;
    opt.textContent = `[${item.id}] ${item.name} - ${formatMoney(item.price)} ₺`;
    prodSelect.appendChild(opt);
  });
}

function filterProductDropdown() {
  const selectedCat = document.getElementById('picker-category').value;
  populateProductDropdown(selectedCat);
}

function onProductSelected() {
  const prodId = document.getElementById('picker-product').value;
  if (!prodId) return;

  const item = sahinCatalog.find(i => i.id === prodId);
  if (item) {
    document.getElementById('item-price').value = item.price;
    document.getElementById('item-unit').value = item.unit || 'Adet';
    document.getElementById('item-kdv').value = item.kdv || 20;
  }
}

// --- 3. ITEM ADDITION & TABLE MANAGEMENT ---
function addItemToQuote() {
  const prodId = document.getElementById('picker-product').value;
  const qty = parseFloat(document.getElementById('item-qty').value) || 1;
  const price = parseFloat(document.getElementById('item-price').value) || 0;
  const unit = document.getElementById('item-unit').value;
  const kdv = parseFloat(document.getElementById('item-kdv').value) || 20;
  const discount = parseFloat(document.getElementById('item-discount').value) || 0;

  if (!prodId) {
    alert('Lütfen teklife eklemek için stok kataloğundan bir ürün seçiniz.');
    return;
  }

  const catalogItem = sahinCatalog.find(i => i.id === prodId);
  if (!catalogItem) return;

  currentQuoteItems.push({
    id: catalogItem.id,
    dbId: catalogItem.dbId,
    code: catalogItem.id,
    name: catalogItem.name,
    unit: unit,
    qty: qty,
    price: price,
    kdv: kdv,
    discount: discount
  });

  // Reset picker inputs
  document.getElementById('picker-product').value = '';
  document.getElementById('item-price').value = '';
  document.getElementById('item-qty').value = '1';
  document.getElementById('item-discount').value = '0';

  updateCalculations();
  updatePdfPreview();
}

function addCustomLineItem() {
  const name = prompt('Özel Kalem / Hizmet Açıklamasını Giriniz:', 'Özel Yazılım Entegrasyonu & Saha Montaj Hizmeti');
  if (!name) return;

  const priceStr = prompt('Birim Fiyatı Giriniz (TL):', '2500');
  const price = parseFloat(priceStr) || 0;

  currentQuoteItems.push({
    id: 'CUST-' + Math.floor(Math.random() * 1000),
    dbId: null,
    code: 'HİZMET',
    name: name,
    unit: 'Hizmet',
    qty: 1,
    price: price,
    kdv: 20,
    discount: 0
  });

  updateCalculations();
  updatePdfPreview();
}

function removeQuoteItem(index) {
  currentQuoteItems.splice(index, 1);
  updateCalculations();
  updatePdfPreview();
}

function updateItemName(index, val) {
  if (currentQuoteItems[index]) {
    currentQuoteItems[index].name = val;
    updatePdfPreview();
  }
}

function updateItemQty(index, val) {
  currentQuoteItems[index].qty = Math.max(1, parseFloat(val) || 1);
  updateCalculations();
  updatePdfPreview();
}

function updateItemPrice(index, val) {
  currentQuoteItems[index].price = Math.max(0, parseFloat(val) || 0);
  updateCalculations();
  updatePdfPreview();
}

function updateItemDiscount(index, val) {
  currentQuoteItems[index].discount = Math.min(100, Math.max(0, parseFloat(val) || 0));
  updateCalculations();
  updatePdfPreview();
}

// --- 4. FINANCIAL CALCULATIONS & CURRENCY ---
function handleCurrencyChange() {
  activeCurrency = document.getElementById('quote-currency').value;
  
  const symbols = { TRY: '₺', USD: '$', EUR: '€' };
  document.querySelectorAll('.curr-symbol').forEach(el => {
    el.textContent = symbols[activeCurrency] || '₺';
  });

  updateCalculations();
  updatePdfPreview();
}

function convertPrice(priceInTL) {
  if (activeCurrency === 'USD') return priceInTL / exchangeRates.USD;
  if (activeCurrency === 'EUR') return priceInTL / exchangeRates.EUR;
  return priceInTL;
}

function getCurrencySymbol() {
  return activeCurrency === 'USD' ? '$' : activeCurrency === 'EUR' ? '€' : '₺';
}

function updateCalculations() {
  const tbody = document.getElementById('quote-items-body');
  document.getElementById('items-count-badge').textContent = currentQuoteItems.length;

  if (currentQuoteItems.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="9" style="text-align: center; color: var(--text-muted); padding: 24px;">
          <i class="fa-solid fa-basket-shopping" style="font-size: 24px; margin-bottom: 8px; display: block;"></i>
          Henüz teklife kalem eklenmedi. Yukarıdaki panelden ürün seçebilirsiniz.
        </td>
      </tr>
    `;
    document.getElementById('calc-subtotal').textContent = `0.00 ${getCurrencySymbol()}`;
    document.getElementById('calc-discount-row').style.display = 'none';
    document.getElementById('calc-kdv').textContent = `0.00 ${getCurrencySymbol()}`;
    document.getElementById('calc-grand-total').textContent = `0.00 ${getCurrencySymbol()}`;
    return;
  }

  let subtotal = 0;
  let totalDiscount = 0;
  let totalKdv = 0;
  const symbol = getCurrencySymbol();

  tbody.innerHTML = '';

  currentQuoteItems.forEach((item, idx) => {
    const itemUnitPrice = convertPrice(item.price);
    const grossTotal = item.qty * itemUnitPrice;
    const discAmount = grossTotal * (item.discount / 100);
    const netTotal = grossTotal - discAmount;
    const kdvAmount = netTotal * (item.kdv / 100);

    subtotal += grossTotal;
    totalDiscount += discAmount;
    totalKdv += kdvAmount;

    const escapedName = escapeHtml(item.name);

    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${idx + 1}</td>
      <td>
        <span class="item-code-badge">${item.code}</span>
        <input type="text" class="form-control item-name-input" value="${escapedName}" onchange="updateItemName(${idx}, this.value)" title="Bu teklif için ürün açıklamasını düzenleyebilirsiniz">
      </td>
      <td>${item.unit}</td>
      <td>
        <input type="number" class="form-control" style="width: 60px; padding: 4px;" value="${item.qty}" min="1" onchange="updateItemQty(${idx}, this.value)">
      </td>
      <td>
        <input type="number" class="form-control" style="width: 90px; padding: 4px;" value="${itemUnitPrice.toFixed(2)}" step="0.01" onchange="updateItemPrice(${idx}, this.value)">
      </td>
      <td>%${item.kdv}</td>
      <td>
        <input type="number" class="form-control" style="width: 60px; padding: 4px;" value="${item.discount}" min="0" max="100" onchange="updateItemDiscount(${idx}, this.value)">
      </td>
      <td><strong>${formatMoney(netTotal)} ${symbol}</strong></td>
      <td>
        <button class="btn btn-sm btn-outline" onclick="removeQuoteItem(${idx})" title="Kalemi Sil" style="color: var(--accent-red); border-color: transparent;">
          <i class="fa-solid fa-trash-can"></i>
        </button>
      </td>
    `;
    tbody.appendChild(tr);
  });

  const grandTotal = (subtotal - totalDiscount) + totalKdv;

  document.getElementById('calc-subtotal').textContent = `${formatMoney(subtotal)} ${symbol}`;
  
  if (totalDiscount > 0) {
    document.getElementById('calc-discount-row').style.display = 'flex';
    document.getElementById('calc-discount-total').textContent = `-${formatMoney(totalDiscount)} ${symbol}`;
  } else {
    document.getElementById('calc-discount-row').style.display = 'none';
  }

  document.getElementById('calc-kdv').textContent = `${formatMoney(totalKdv)} ${symbol}`;
  document.getElementById('calc-grand-total').textContent = `${formatMoney(grandTotal)} ${symbol}`;
}

// --- 5. LIVE PDF PREVIEW SYNCHRONIZER ---
function updatePdfPreview() {
  const company = document.getElementById('cust-company').value || 'ABC Lojistik ve Ticaret A.Ş.';
  const contact = document.getElementById('cust-contact').value || 'Ahmet Yılmaz';
  const phone = document.getElementById('cust-phone').value || '0532 000 00 00';
  const email = document.getElementById('cust-email').value || 'ahmet@abclojistik.com';
  const tax = document.getElementById('cust-tax-office').value || 'Kadıköy V.D. 1234567890';
  const address = document.getElementById('cust-address').value || 'Ataşehir, İstanbul';

  const quoteNo = document.getElementById('quote-no').value || 'TEK-2026-0842';
  const rawDate = document.getElementById('quote-date').value;
  const formattedDate = rawDate ? new Date(rawDate).toLocaleDateString('tr-TR') : new Date().toLocaleDateString('tr-TR');
  const validity = document.getElementById('quote-validity').value;

  const payment = document.getElementById('terms-payment').value;
  const delivery = document.getElementById('terms-delivery').value;
  const note = document.getElementById('quote-note').value;
  const bankSelect = document.getElementById('terms-bank');
  const bankText = bankSelect.options[bankSelect.selectedIndex].text;

  // Sync Header & Customer Details
  document.getElementById('pdf-val-no').textContent = quoteNo;
  document.getElementById('pdf-val-date').textContent = formattedDate;
  document.getElementById('pdf-val-validity').textContent = validity;
  
  document.getElementById('pdf-val-company').textContent = company;
  document.getElementById('pdf-val-contact').textContent = contact;
  document.getElementById('pdf-val-phone').textContent = phone;
  document.getElementById('pdf-val-email').textContent = email;
  document.getElementById('pdf-val-tax').textContent = tax;
  document.getElementById('pdf-val-address').textContent = address;
  document.getElementById('pdf-sig-customer-name').textContent = company;

  document.getElementById('pdf-val-currency').textContent = `${activeCurrency} (${getCurrencySymbol()})`;
  document.getElementById('pdf-val-payment').textContent = payment;
  document.getElementById('pdf-val-delivery').textContent = delivery;
  document.getElementById('pdf-val-bank').textContent = bankText;

  if (note && note.trim() !== '') {
    document.getElementById('pdf-note-container').style.display = 'block';
    document.getElementById('pdf-val-note').textContent = note;
  } else {
    document.getElementById('pdf-note-container').style.display = 'none';
  }

  // Populate PDF Items Table
  const pdfTbody = document.getElementById('pdf-items-body');
  pdfTbody.innerHTML = '';

  const symbol = getCurrencySymbol();
  let subtotal = 0;
  let totalDiscount = 0;
  let totalKdv = 0;

  if (currentQuoteItems.length === 0) {
    pdfTbody.innerHTML = `
      <tr>
        <td colspan="9" style="text-align: center; color: #94a3b8; padding: 20px;">
          Teklife henüz ürün kalemi eklenmemiştir.
        </td>
      </tr>
    `;
  } else {
    currentQuoteItems.forEach((item, idx) => {
      const itemUnitPrice = convertPrice(item.price);
      const grossTotal = item.qty * itemUnitPrice;
      const discAmount = grossTotal * (item.discount / 100);
      const netTotal = grossTotal - discAmount;
      const kdvAmount = netTotal * (item.kdv / 100);

      subtotal += grossTotal;
      totalDiscount += discAmount;
      totalKdv += kdvAmount;

      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td style="text-align: center;">${idx + 1}</td>
        <td><strong>${item.code}</strong></td>
        <td>${escapeHtml(item.name)}</td>
        <td>${item.unit}</td>
        <td style="text-align: center;">${item.qty}</td>
        <td style="text-align: right;">${formatMoney(itemUnitPrice)} ${symbol}</td>
        <td style="text-align: center;">%${item.kdv}</td>
        <td style="text-align: center;">${item.discount > 0 ? '%' + item.discount : '-'}</td>
        <td style="text-align: right;"><strong>${formatMoney(netTotal)} ${symbol}</strong></td>
      `;
      pdfTbody.appendChild(tr);
    });
  }

  const grandTotal = (subtotal - totalDiscount) + totalKdv;

  document.getElementById('pdf-val-subtotal').textContent = `${formatMoney(subtotal)} ${symbol}`;
  
  if (totalDiscount > 0) {
    document.getElementById('pdf-discount-row').style.display = 'flex';
    document.getElementById('pdf-val-discount').textContent = `-${formatMoney(totalDiscount)} ${symbol}`;
  } else {
    document.getElementById('pdf-discount-row').style.display = 'none';
  }

  document.getElementById('pdf-val-kdv').textContent = `${formatMoney(totalKdv)} ${symbol}`;
  document.getElementById('pdf-val-grand').textContent = `${formatMoney(grandTotal)} ${symbol}`;
}

// --- 6. SAVE & HIGH QUALITY PDF EXPORT ENGINE ---
function saveAndExportPdf() {
  const company = document.getElementById('cust-company').value;
  const contact = document.getElementById('cust-contact').value;

  if (!company || !contact) {
    alert('Lütfen teklif oluşturmak için Firma Unvanı ve İlgili Kişi alanlarını doldurunuz.');
    return;
  }

  if (currentQuoteItems.length === 0) {
    alert('Teklif oluşturmak için en az 1 adet ürün eklemelisiniz.');
    return;
  }

  updatePdfPreview();

  // Save proposal to storage history & SQL Database API
  saveProposalToHistory('Onaylandı / PDF İndirildi');

  const element = document.getElementById('pdf-template');
  const quoteNo = document.getElementById('quote-no').value;
  const sanitizedCompany = company.replace(/[^a-zA-Z0-9]/g, '_');
  const filename = `Sahin_Bilisim_Teklif_${quoteNo}_${sanitizedCompany}.pdf`;

  const opt = {
    margin: [0, 0, 0, 0],
    filename: filename,
    image: { type: 'jpeg', quality: 0.98 },
    html2canvas: { scale: 2, useCORS: true, letterRendering: true },
    jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' }
  };

  html2pdf().set(opt).from(element).save().then(() => {
    alert(`"${filename}" başarıyla oluşturuldu, SQL veritabanına kaydedildi ve cihazınıza indirildi!`);
    renderDashboardMetrics();
  }).catch(err => {
    console.error('PDF Generation Error:', err);
    alert('PDF oluşturulurken bir uyarı oluştu. Doğrudan yazdır butonu ile PDF olarak yazdırabilirsiniz.');
  });
}

function saveAsDraft() {
  saveProposalToHistory('Taslak');
  alert('Teklifiniz başarıyla SQL veritabanına taslak olarak kaydedildi!');
  renderDashboardMetrics();
}

function saveProposalToHistory(status = 'Taslak') {
  const quoteNo = document.getElementById('quote-no').value;
  const company = document.getElementById('cust-company').value || 'ABC Lojistik';
  const contact = document.getElementById('cust-contact').value || 'Yetkili';
  const date = document.getElementById('quote-date').value;
  const companyInput = document.getElementById('cust-company');
  const customerId = companyInput ? parseInt(companyInput.dataset.customerId || '0') : 0;

  // Calculate totals
  let subtotal = 0;
  let totalDiscount = 0;
  let totalKdv = 0;
  currentQuoteItems.forEach(i => {
    const itemUnitPrice = convertPrice(i.price);
    const gross = i.qty * itemUnitPrice;
    const disc = gross * (i.discount / 100);
    const net = gross - disc;
    subtotal += gross;
    totalDiscount += disc;
    totalKdv += net * (i.kdv / 100);
  });

  const grandTotal = (subtotal - totalDiscount) + totalKdv;

  const proposalObj = {
    no: quoteNo,
    quoteNumber: quoteNo,
    customerId: customerId > 0 ? customerId : null,
    company: company,
    contact: contact,
    phone: document.getElementById('cust-phone').value || '',
    email: document.getElementById('cust-email').value || '',
    taxOffice: document.getElementById('cust-tax-office').value || '',
    address: document.getElementById('cust-address').value || '',
    date: date,
    quoteDate: date ? new Date(date).toISOString() : new Date().toISOString(),
    currency: activeCurrency,
    currencyCode: activeCurrency,
    exchangeRate: exchangeRates[activeCurrency] || 1,
    notes: document.getElementById('quote-note').value || '',
    grandTotal: grandTotal,
    status: status,
    items: JSON.parse(JSON.stringify(currentQuoteItems)),
    savedAt: new Date().toISOString()
  };

  // Local storage save
  const existingIdx = savedProposals.findIndex(p => p.no === quoteNo);
  if (existingIdx >= 0) {
    savedProposals[existingIdx] = proposalObj;
  } else {
    savedProposals.unshift(proposalObj);
  }

  localStorage.setItem('sahin_admin_proposals', JSON.stringify(savedProposals));
  document.getElementById('saved-count').textContent = savedProposals.length;

  // ASP.NET Core SQL Database API Call
  fetch('/Quotes/SaveQuoteApi', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(proposalObj)
  }).then(r => r.json()).then(res => {
    if (res && res.success) {
      console.log('Teklif SQL Veritabanına başarıyla kaydedildi! Kayıt ID:', res.id);
    }
  }).catch(e => console.warn('SQL Veritabanı kayıt uyarısı:', e));
}

// --- 7. DASHBOARD METRICS ---
function renderDashboardMetrics() {
  document.getElementById('metric-total-proposals').textContent = savedProposals.length;
  document.getElementById('saved-count').textContent = savedProposals.length;
  document.getElementById('metric-stock-count').textContent = `${sahinCatalog.length} Ürün`;

  let totalVolTL = 0;
  let approvedCount = 0;

  savedProposals.forEach(p => {
    let amtTL = p.grandTotal;
    if (p.currency === 'USD') amtTL *= exchangeRates.USD;
    if (p.currency === 'EUR') amtTL *= exchangeRates.EUR;

    totalVolTL += amtTL;
    if (p.status.includes('Onaylandı')) approvedCount++;
  });

  document.getElementById('metric-total-volume').textContent = `${formatMoney(totalVolTL)} ₺`;
  document.getElementById('metric-approved-count').textContent = approvedCount;
}

// --- 8. MODAL HANDLERS & TABLES ---
function openStockModal() {
  document.getElementById('stock-modal').classList.add('active');
  renderStockManagementTable();
}

function closeStockModal() {
  document.getElementById('stock-modal').classList.remove('active');
}

function renderStockManagementTable() {
  const tbody = document.getElementById('stock-manage-body');
  const query = document.getElementById('stock-search-input').value.toLowerCase();

  const filtered = sahinCatalog.filter(i => 
    i.name.toLowerCase().includes(query) || 
    i.id.toLowerCase().includes(query) ||
    i.category.toLowerCase().includes(query)
  );

  tbody.innerHTML = '';

  filtered.forEach(item => {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td><span class="item-code-badge">${item.id}</span></td>
      <td><strong>${item.name}</strong></td>
      <td>${item.category}</td>
      <td><strong>${formatMoney(item.price)} ₺</strong></td>
      <td>${item.stock} ${item.unit || 'Adet'}</td>
      <td><span class="badge-status approved">Stokta Var</span></td>
      <td>
        <button class="btn btn-sm btn-outline-gold" onclick="editProduct('${item.id}')">Düzenle</button>
        <button class="btn btn-sm btn-outline" onclick="deleteProduct('${item.id}')" style="color: var(--accent-red);">Sil</button>
      </td>
    `;
    tbody.appendChild(tr);
  });
}

function showAddProductSubModal() {
  document.getElementById('product-modal-title').innerHTML = '<i class="fa-solid fa-box-open"></i> Yeni Ürün Ekle';
  document.getElementById('product-edit-form').reset();
  document.getElementById('edit-prod-id').value = '';
  document.getElementById('product-edit-modal').classList.add('active');
}

function closeProductEditModal() {
  document.getElementById('product-edit-modal').classList.remove('active');
}

function editProduct(prodId) {
  const item = sahinCatalog.find(i => i.id === prodId);
  if (!item) return;

  document.getElementById('product-modal-title').innerHTML = '<i class="fa-solid fa-pen-to-square"></i> Ürün Düzenle';
  document.getElementById('edit-prod-id').value = item.id;
  document.getElementById('edit-prod-code').value = item.id;
  document.getElementById('edit-prod-name').value = item.name;
  document.getElementById('edit-prod-category').value = item.category;
  document.getElementById('edit-prod-unit').value = item.unit || 'Adet';
  document.getElementById('edit-prod-price').value = item.price;
  document.getElementById('edit-prod-stock').value = item.stock;

  document.getElementById('product-edit-modal').classList.add('active');
}

function handleSaveProduct(e) {
  e.preventDefault();
  const prodId = document.getElementById('edit-prod-id').value;
  const code = document.getElementById('edit-prod-code').value;
  const name = document.getElementById('edit-prod-name').value;
  const category = document.getElementById('edit-prod-category').value;
  const unit = document.getElementById('edit-prod-unit').value;
  const price = parseFloat(document.getElementById('edit-prod-price').value) || 0;
  const stock = parseInt(document.getElementById('edit-prod-stock').value) || 0;

  if (prodId) {
    const idx = sahinCatalog.findIndex(i => i.id === prodId);
    if (idx >= 0) {
      sahinCatalog[idx] = { id: code, name, category, unit, price, stock, kdv: 20 };
    }
  } else {
    sahinCatalog.push({ id: code, name, category, unit, price, stock, kdv: 20 });
  }

  localStorage.setItem('sahin_admin_catalog', JSON.stringify(sahinCatalog));
  populateCategoryDropdowns();
  populateProductDropdown();
  renderStockManagementTable();
  renderDashboardMetrics();
  closeProductEditModal();
}

function deleteProduct(prodId) {
  if (confirm(`"${prodId}" ürünü stok listesinden silmek istediğinize emin misiniz?`)) {
    sahinCatalog = sahinCatalog.filter(i => i.id !== prodId);
    localStorage.setItem('sahin_admin_catalog', JSON.stringify(sahinCatalog));
    populateCategoryDropdowns();
    populateProductDropdown();
    renderStockManagementTable();
    renderDashboardMetrics();
  }
}

// HISTORY MODAL
function openHistoryModal() {
  document.getElementById('history-modal').classList.add('active');
  renderHistoryTable();
}

function closeHistoryModal() {
  document.getElementById('history-modal').classList.remove('active');
}

function renderHistoryTable() {
  const tbody = document.getElementById('history-manage-body');
  const query = document.getElementById('history-search-input').value.toLowerCase();

  const filtered = savedProposals.filter(p => 
    p.no.toLowerCase().includes(query) || 
    p.company.toLowerCase().includes(query) ||
    p.contact.toLowerCase().includes(query)
  );

  document.getElementById('history-total-count').textContent = savedProposals.length;
  tbody.innerHTML = '';

  if (filtered.length === 0) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align: center; color: var(--text-muted); padding: 20px;">Kayıtlı teklif bulunamadı.</td></tr>`;
    return;
  }

  filtered.forEach(p => {
    const symbol = p.currency === 'USD' ? '$' : p.currency === 'EUR' ? '€' : '₺';
    const statusClass = p.status.includes('Onaylandı') ? 'approved' : 'draft';
    
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td><span class="item-code-badge">${p.no}</span></td>
      <td>${p.date ? new Date(p.date).toLocaleDateString('tr-TR') : '-'}</td>
      <td><strong>${p.company}</strong></td>
      <td>${p.contact}</td>
      <td><strong>${formatMoney(p.grandTotal)} ${symbol}</strong></td>
      <td><span class="badge-status ${statusClass}">${p.status}</span></td>
      <td>
        <button class="btn btn-sm btn-gold" onclick="loadProposalFromHistory('${p.no}')">Yükle / Düzenle</button>
        <button class="btn btn-sm btn-outline" onclick="deleteHistoryProposal('${p.no}')" style="color: var(--accent-red);">Sil</button>
      </td>
    `;
    tbody.appendChild(tr);
  });
}

function loadProposalFromHistory(quoteNo) {
  const prop = savedProposals.find(p => p.no === quoteNo);
  if (!prop) return;

  document.getElementById('quote-no').value = prop.no;
  document.getElementById('cust-company').value = prop.company;
  document.getElementById('cust-contact').value = prop.contact;
  if (prop.date) document.getElementById('quote-date').value = prop.date;
  if (prop.currency) {
    document.getElementById('quote-currency').value = prop.currency;
    activeCurrency = prop.currency;
  }

  currentQuoteItems = JSON.parse(JSON.stringify(prop.items || []));
  
  updateCalculations();
  updatePdfPreview();
  closeHistoryModal();
}

function deleteHistoryProposal(quoteNo) {
  if (confirm(`"${quoteNo}" numaralı teklifi geçmişten silmek istiyor musunuz?`)) {
    savedProposals = savedProposals.filter(p => p.no !== quoteNo);
    localStorage.setItem('sahin_admin_proposals', JSON.stringify(savedProposals));
    renderHistoryTable();
    renderDashboardMetrics();
  }
}

function resetFormWithNewNo() {
  document.getElementById('quote-form').reset();
  currentQuoteItems = [];
  generateNewQuoteNumber();
  setDefaultDates();
  updateCalculations();
  updatePdfPreview();
}

// --- HELPER UTILS ---
function formatMoney(amount) {
  return Number(amount || 0).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function escapeHtml(str) {
  return String(str || '').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}
