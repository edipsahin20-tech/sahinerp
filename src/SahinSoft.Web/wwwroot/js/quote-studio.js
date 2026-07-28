/* ==========================================================================
   admin.sahinbilisim.com.tr - TEKLİF & PDF PORTALI CORE ENGINE (app.js)
   Garanti BBVA & Live ASP.NET Core SQL Database Integration Engine
   ========================================================================== */

// --- GLOBAL STATE ---
let sahinCatalog = [];
let currentQuoteItems = [];
let savedProposals = [];
let activeCurrency = 'TRY';
let liveCustomers = [];
let selectedCatalogItem = null;

// Büyük/küçük harf ve Türkçe karakter (İ/I/ı/i, ç/ş/ğ/ü/ö) farkı gözetmeden karşılaştırma için normalize eder.
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
  generateNewQuoteNumber();
  setDefaultDates();

  // Fetch Live ASP.NET Core SQL Database Products and Customers
  loadLiveDbCatalog();
  loadLiveDbCustomers();

  updateCalculations();
  updatePdfPreview();
  renderDashboardMetrics();

  // Canlı kur, sayfa geneli currency_sync.js tarafından yönetilir (15 dk otomatik + Canlı Kur Al butonu).
  window.addEventListener('garantiRatesUpdated', () => {
    updateCalculations();
    updatePdfPreview();
  });
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
  const dataList = document.getElementById('cust-company-list');
  if (!companyInput || liveCustomers.length === 0) return;

  if (dataList) {
    dataList.innerHTML = liveCustomers.map(c => `<option value="${escapeHtml(c.name)}"></option>`).join('');
  }

  companyInput.addEventListener('change', () => {
    const val = turkishNormalize(companyInput.value);
    const found = liveCustomers.find(c => turkishNormalize(c.name) === val) ||
                  liveCustomers.find(c => turkishNormalize(c.name).includes(val) || turkishNormalize(c.company).includes(val));
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

// Kategori değiştiğinde, o an yazılmış olan aramayı yeni kategoriyle tekrar çalıştırır.
function filterProductDropdown() {
  runProductSearch(document.getElementById('picker-product-search').value);
}

// Mikro tarzı arama: yazmaya başlayınca canlı arar. Sonuna * konması da desteklenir (kozmetik, arama zaten "içerir" mantığında).
let productSearchResults = [];
let productSearchHighlightIndex = -1;

function runProductSearch(rawQuery) {
  const resultsEl = document.getElementById('picker-product-results');
  const query = turkishNormalize((rawQuery || '').trim().replace(/\*+$/, ''));
  const category = document.getElementById('picker-category').value;

  productSearchHighlightIndex = -1;

  if (!query) {
    productSearchResults = [];
    resultsEl.innerHTML = '';
    resultsEl.classList.remove('active');
    document.getElementById('picker-product-value').value = '';
    return;
  }

  let items = category === 'ALL' ? sahinCatalog : sahinCatalog.filter(i => i.category === category);
  items = items.filter(i =>
    turkishNormalize(i.id).includes(query) ||
    turkishNormalize(i.name).includes(query)
  ).slice(0, 25);

  productSearchResults = items;

  if (items.length === 0) {
    resultsEl.innerHTML = '<div class="product-search-empty">Sonuç bulunamadı.</div>';
    resultsEl.classList.add('active');
    return;
  }

  resultsEl.innerHTML = items.map((item, idx) => `
    <div class="product-search-item" data-idx="${idx}" onclick="selectProductFromSearch('${item.id.replace(/'/g, "\\'")}')">
      <span class="item-code-badge">${item.id}</span>
      <span class="product-search-name">${escapeHtml(item.name)}</span>
      <strong>${formatMoney(item.price)} ₺</strong>
    </div>
  `).join('');
  resultsEl.classList.add('active');
}

// Yukarı/Aşağı ok tuşlarıyla listede gezinme; seçili satırı vurgular ve görünür alana kaydırır.
function moveProductSearchHighlight(delta) {
  if (productSearchResults.length === 0) return;
  productSearchHighlightIndex = Math.max(0, Math.min(productSearchResults.length - 1, productSearchHighlightIndex + delta));

  const resultsEl = document.getElementById('picker-product-results');
  resultsEl.querySelectorAll('.product-search-item').forEach(el => {
    el.classList.toggle('highlighted', Number(el.dataset.idx) === productSearchHighlightIndex);
  });
  const activeEl = resultsEl.querySelector('.product-search-item.highlighted');
  if (activeEl) activeEl.scrollIntoView({ block: 'nearest' });
}

function selectProductFromSearch(prodId) {
  const item = sahinCatalog.find(i => i.id === prodId);
  if (!item) return;

  selectedCatalogItem = item;
  document.getElementById('picker-product-value').value = item.id;
  document.getElementById('picker-product-search').value = `${item.id} - ${item.name}`;
  document.getElementById('item-price').value = item.price;
  document.getElementById('item-unit').value = item.unit || 'Adet';
  document.getElementById('item-kdv').value = item.kdv || 20;

  const resultsEl = document.getElementById('picker-product-results');
  resultsEl.innerHTML = '';
  resultsEl.classList.remove('active');
  productSearchResults = [];
  productSearchHighlightIndex = -1;

  const qtyInput = document.getElementById('item-qty');
  qtyInput.focus();
  qtyInput.select();
}

// F9 (Mikro tarzı arama tuşu) veya Enter ile arama tetiklenir. Yukarı/Aşağı ok tuşlarıyla
// listede gezinilir; Enter, vurgulanan satırı (yoksa tek sonucu) seçer.
function handleProductSearchKeydown(e, input) {
  if (e.key === 'F9') {
    e.preventDefault();
    openFullProductLookup(input.value);
    return;
  }
  if (e.key === 'ArrowDown') {
    e.preventDefault();
    if (productSearchResults.length === 0) {
      runProductSearch(input.value);
    }
    moveProductSearchHighlight(1);
    return;
  }
  if (e.key === 'ArrowUp') {
    e.preventDefault();
    moveProductSearchHighlight(-1);
    return;
  }
  if (e.key === 'Enter') {
    e.preventDefault();
    if (productSearchHighlightIndex >= 0 && productSearchResults[productSearchHighlightIndex]) {
      selectProductFromSearch(productSearchResults[productSearchHighlightIndex].id);
      return;
    }
    runProductSearch(input.value);
    if (productSearchResults.length === 1) {
      selectProductFromSearch(productSearchResults[0].id);
    }
  }
}

// F9: Mikro tarzı tam ekran arama penceresi (sitenin genelindeki paylaşılan arama modalı).
// Şu ana kadar yazılmış olan metni de modaldaki arama kutusuna aktarır.
function openFullProductLookup(currentText) {
  const trigger = document.getElementById('picker-product-lookup-trigger');
  if (!trigger) return;

  document.getElementById('picker-product-results').classList.remove('active');
  trigger.click();

  const query = (currentText || '').trim().replace(/\*+$/, '');
  setTimeout(() => {
    const modalSearch = document.querySelector('#lookupModal .lookup-modal-search');
    if (modalSearch) {
      modalSearch.value = query;
      modalSearch.dispatchEvent(new Event('input', { bubbles: true }));
      modalSearch.focus();
    }
  }, 50);
}

// Paylaşılan arama penceresinden (F9) bir ürün seçildiğinde, fiyat/birim/KDV alanlarını doldurur.
document.addEventListener('lookup:selected', function (e) {
  if (!e.detail || !e.detail.hiddenEl || e.detail.hiddenEl.id !== 'picker-product-value') {
    return;
  }
  const item = e.detail.item;
  selectedCatalogItem = {
    id: item.code,
    dbId: item.id,
    name: item.name,
    unit: item.unit || 'Adet',
    price: item.salePrice,
    kdv: item.taxRate
  };
  document.getElementById('item-price').value = selectedCatalogItem.price;
  document.getElementById('item-unit').value = selectedCatalogItem.unit;
  document.getElementById('item-kdv').value = selectedCatalogItem.kdv;
  document.getElementById('picker-product-value').value = selectedCatalogItem.id;
  document.getElementById('picker-product-search').value = `${selectedCatalogItem.id} - ${selectedCatalogItem.name}`;

  const qtyInput = document.getElementById('item-qty');
  qtyInput.focus();
  qtyInput.select();
});

// Miktar/Birim/Fiyat/KDV/İskonto alanlarında Enter, sıradaki alana geçer.
function focusNextOnEnter(e, nextId) {
  if (e.key !== 'Enter') return;
  e.preventDefault();
  const nextEl = document.getElementById(nextId);
  if (!nextEl) return;
  nextEl.focus();
  if (nextEl.select) nextEl.select();
}

// İskonto alanında Enter, satırı teklife ekler ve yeni ürün aramaya geri döner.
function handleDiscountKeydown(e) {
  if (e.key !== 'Enter') return;
  e.preventDefault();
  addItemToQuote();
  document.getElementById('picker-product-search').focus();
}

document.addEventListener('click', function (e) {
  if (!e.target.closest('#picker-product-search') && !e.target.closest('#picker-product-results')) {
    const resultsEl = document.getElementById('picker-product-results');
    if (resultsEl) {
      resultsEl.innerHTML = '';
      resultsEl.classList.remove('active');
      productSearchResults = [];
      productSearchHighlightIndex = -1;
    }
  }
});

// --- 3. ITEM ADDITION & TABLE MANAGEMENT ---
function addItemToQuote() {
  const prodId = document.getElementById('picker-product-value').value;
  const qty = parseFloat(document.getElementById('item-qty').value) || 1;
  const price = parseFloat(document.getElementById('item-price').value) || 0;
  const unit = document.getElementById('item-unit').value;
  const kdv = parseFloat(document.getElementById('item-kdv').value) || 20;
  const discount = parseFloat(document.getElementById('item-discount').value) || 0;

  if (!prodId || !selectedCatalogItem || selectedCatalogItem.id !== prodId) {
    alert('Lütfen teklife eklemek için önce bir ürün seçiniz (yazarak veya F9 ile arayarak).');
    return;
  }

  currentQuoteItems.push({
    id: selectedCatalogItem.id,
    dbId: selectedCatalogItem.dbId,
    code: selectedCatalogItem.id,
    name: selectedCatalogItem.name,
    unit: unit,
    qty: qty,
    price: price,
    kdv: kdv,
    discount: discount
  });

  // Reset picker inputs
  selectedCatalogItem = null;
  productSearchResults = [];
  productSearchHighlightIndex = -1;
  document.getElementById('picker-product-value').value = '';
  document.getElementById('picker-product-search').value = '';
  document.getElementById('picker-product-results').innerHTML = '';
  document.getElementById('picker-product-results').classList.remove('active');
  document.getElementById('item-price').value = '';
  document.getElementById('item-qty').value = '1';
  document.getElementById('item-discount').value = '0';

  updateCalculations();
  updatePdfPreview();
}

async function addCustomLineItem() {
  const name = prompt('Özel Kalem / Hizmet Açıklamasını Giriniz:', 'Özel Yazılım Entegrasyonu & Saha Montaj Hizmeti');
  if (!name) return;

  const priceStr = prompt('Birim Fiyatı Giriniz (TL):', '2500');
  const price = parseFloat(priceStr) || 0;

  let newProduct = null;
  try {
    const res = await fetch('/Quotes/CreateCustomProductApi', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name: name, price: price })
    });
    if (res.ok) {
      const data = await res.json();
      if (data && data.success) {
        newProduct = data;
      } else {
        console.warn('Otomatik stok kartı oluşturulamadı:', data && data.message);
      }
    }
  } catch (err) {
    console.warn('Otomatik stok kartı oluşturma uyarısı:', err);
  }

  const catalogEntry = {
    id: newProduct ? newProduct.code : ('CUST-' + Math.floor(Math.random() * 1000)),
    dbId: newProduct ? newProduct.id : null,
    category: 'Yazılımlar',
    unit: newProduct ? newProduct.unit : 'Hizmet',
    price: price,
    stock: 0,
    kdv: newProduct ? newProduct.taxRate : 20,
    name: name
  };

  // Yeni oluşan stok kartını kataloğa da ekle, sonraki tekliflerde aramada çıksın.
  if (newProduct) {
    sahinCatalog.push(catalogEntry);
    populateCategoryDropdowns();
  }

  currentQuoteItems.push({
    id: catalogEntry.id,
    dbId: catalogEntry.dbId,
    code: catalogEntry.id,
    name: name,
    unit: catalogEntry.unit,
    qty: 1,
    price: price,
    kdv: catalogEntry.kdv,
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
  if (activeCurrency === 'USD') return priceInTL / window.globalExchangeRates.USD;
  if (activeCurrency === 'EUR') return priceInTL / window.globalExchangeRates.EUR;
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
        <button type="button" class="btn btn-sm btn-outline" onclick="removeQuoteItem(${idx})" title="Kalemi Sil" style="color: var(--accent-red); border-color: transparent;">
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
  const company = document.getElementById('cust-company').value;
  const contact = document.getElementById('cust-contact').value;
  const phone = document.getElementById('cust-phone').value;
  const email = document.getElementById('cust-email').value;
  const tax = document.getElementById('cust-tax-office').value;
  const address = document.getElementById('cust-address').value;

  const quoteNo = document.getElementById('quote-no').value;
  const rawDate = document.getElementById('quote-date').value;
  const formattedDate = rawDate ? new Date(rawDate).toLocaleDateString('tr-TR') : new Date().toLocaleDateString('tr-TR');
  const validity = document.getElementById('quote-validity').value;

  const payment = document.getElementById('terms-payment').value;
  const delivery = document.getElementById('terms-delivery').value;
  const note = document.getElementById('quote-note').value;

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

  const quoteNo = document.getElementById('quote-no').value;
  const sanitizedCompany = company.replace(/[^a-zA-Z0-9]/g, '_');
  const filename = `Sahin_Bilisim_Teklif_${quoteNo}_${sanitizedCompany}.pdf`;

  exportElementToPdf('pdf-template', filename).then(() => {
    alert(`"${filename}" başarıyla oluşturuldu, SQL veritabanına kaydedildi ve cihazınıza indirildi!`);
    renderDashboardMetrics();
  }).catch(err => {
    console.error('PDF Generation Error:', err);
    alert('PDF oluşturulurken bir uyarı oluştu. Doğrudan yazdır butonu ile PDF olarak yazdırabilirsiniz.');
  });
}

function saveAsDraft() {
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

  saveProposalToHistory('Taslak');
  alert('Teklifiniz başarıyla SQL veritabanına taslak olarak kaydedildi!');
  renderDashboardMetrics();
}

function saveProposalToHistory(status = 'Taslak') {
  const quoteNo = document.getElementById('quote-no').value;
  const company = document.getElementById('cust-company').value;
  const contact = document.getElementById('cust-contact').value;
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
    exchangeRate: window.globalExchangeRates[activeCurrency] || 1,
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
      if (res.quoteNumber) {
        document.getElementById('quote-no').value = res.quoteNumber;
        document.getElementById('pdf-val-no').textContent = res.quoteNumber;
        proposalObj.no = res.quoteNumber;
        proposalObj.quoteNumber = res.quoteNumber;
        const idx = savedProposals.findIndex(p => p === proposalObj || p.savedAt === proposalObj.savedAt);
        if (idx >= 0) {
          savedProposals[idx].no = res.quoteNumber;
          savedProposals[idx].quoteNumber = res.quoteNumber;
          localStorage.setItem('sahin_admin_proposals', JSON.stringify(savedProposals));
        }
      }
      if (res.id && confirm(`Teklif ${res.quoteNumber || ''} kaydedildi. Teklif detayına gidip satış faturasına dönüştürmek ister misiniz?`)) {
        window.location.href = '/Quotes/Details/' + res.id;
      }
    }
  }).catch(e => console.warn('SQL Veritabanı kayıt uyarısı:', e));
}

// --- 7. DASHBOARD METRICS ---
function renderDashboardMetrics() {
  document.getElementById('metric-total-proposals').textContent = savedProposals.length;
  document.getElementById('saved-count').textContent = savedProposals.length;

  let totalVolTL = 0;
  let approvedCount = 0;

  savedProposals.forEach(p => {
    let amtTL = p.grandTotal;
    if (p.currency === 'USD') amtTL *= window.globalExchangeRates.USD;
    if (p.currency === 'EUR') amtTL *= window.globalExchangeRates.EUR;

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
  const query = turkishNormalize(document.getElementById('stock-search-input').value);

  const filtered = sahinCatalog.filter(i => 
    turkishNormalize(i.name).includes(query) || 
    turkishNormalize(i.id).includes(query) ||
    turkishNormalize(i.category).includes(query)
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
      <td>-</td>
    `;
    tbody.appendChild(tr);
  });
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
  const query = turkishNormalize(document.getElementById('history-search-input').value);

  const filtered = savedProposals.filter(p => 
    turkishNormalize(p.no).includes(query) || 
    turkishNormalize(p.company).includes(query) ||
    turkishNormalize(p.contact).includes(query)
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
