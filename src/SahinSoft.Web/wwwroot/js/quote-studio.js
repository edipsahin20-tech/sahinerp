/* ==========================================================================
   admin.sahinbilisim.com.tr - TEKLİF & PDF PORTALI CORE ENGINE (app.js)
   Garanti BBVA & Live ASP.NET Core SQL Database Integration Engine
   ========================================================================== */

// --- GLOBAL STATE ---
let sahinCatalog = [];
let currentQuoteItems = [];
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

// INITIALIZE APP ON DOM READY
document.addEventListener('DOMContentLoaded', () => {
  populateCategoryDropdowns();
  generateNewQuoteNumber();
  setDefaultDates();

  // Fetch Live ASP.NET Core SQL Database Products and Customers
  loadLiveDbCatalog();
  loadLiveDbCustomers();
  refreshDashboardMetrics();

  updateCalculations();
  updatePdfPreview();

  // Canlı kur, sayfa geneli currency_sync.js tarafından yönetilir (15 dk otomatik + Canlı Kur Al butonu).
  window.addEventListener('garantiRatesUpdated', () => {
    updateCalculations();
    updatePdfPreview();
  });
});

// --- LIVE ASP.NET CORE SQL DATABASE FETCH ---
// Kataloğu her zaman canlı veritabanından çeker; sahte/örnek veriye asla düşmez.
async function loadLiveDbCatalog() {
  try {
    const res = await fetch('/Quotes/GetCatalogDataApi');
    const data = res.ok ? await res.json() : null;
    sahinCatalog = (data && data.products) || [];
  } catch (err) {
    sahinCatalog = [];
    console.error('Stok kataloğu yüklenemedi:', err);
  }
  populateCategoryDropdowns();
  renderStockManagementTable();
}

async function refreshDashboardMetrics() {
  try {
    const res = await fetch('/Quotes/GetQuoteMetricsApi');
    if (!res.ok) return;
    const data = await res.json();
    document.getElementById('metric-total-proposals').textContent = data.totalCount;
    document.getElementById('metric-total-volume').textContent = `${formatMoney(data.totalVolume)} ₺`;
    document.getElementById('metric-approved-count').textContent = data.approvedCount;
  } catch (err) {
    console.error('Teklif metrikleri yüklenemedi:', err);
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
    if (found) applySelectedCustomer(found);
  });
}

// Bulunan/seçilen cariyi Müşteri & Firma Bilgileri alanlarına doldurur (datalist ve F9 arama penceresi ortak kullanır).
function applySelectedCustomer(found) {
  document.getElementById('cust-company').value = found.company || found.name;
  document.getElementById('cust-contact').value = found.contact || found.name;
  document.getElementById('cust-phone').value = found.phone || '';
  document.getElementById('cust-email').value = found.email || '';
  document.getElementById('cust-tax-office').value = found.taxOffice || '';
  document.getElementById('cust-address').value = found.address || '';
  document.getElementById('cust-company').dataset.customerId = found.id;
  updatePdfPreview();
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
    openQuickSearchModal('product', input.value);
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

// F9: küçük, kompakt arama penceresi (Mikro tarzı). Ürün ve cari aramasında ortak kullanılır.
// Boş alanda da açılır (o an seçili kategori/tüm liste taranır); ok tuşlarıyla gezinilir.
let quickSearchMode = 'product';
let quickSearchResults = [];
let quickSearchHighlightIndex = -1;

function openQuickSearchModal(mode, initialQuery) {
  quickSearchMode = mode;
  const modal = document.getElementById('quick-search-modal');
  const title = document.getElementById('quick-search-title');
  const thead = document.getElementById('quick-search-thead');
  const input = document.getElementById('quick-search-input');
  if (!modal || !input) return;

  document.getElementById('picker-product-results')?.classList.remove('active');

  if (mode === 'customer') {
    title.textContent = 'Cari Ara (F9)';
    thead.innerHTML = '<tr><th>Cari Kodu</th><th>Cari İsmi</th><th class="text-end">Borç</th><th class="text-end">Alacak</th></tr>';
  } else {
    title.textContent = 'Ürün / Stok Ara (F9)';
    thead.innerHTML = '<tr><th>Stok Kodu</th><th>Stok İsmi</th><th>Kategori</th><th class="text-end">Fiyat</th><th class="text-end">Miktar</th></tr>';
  }

  modal.classList.add('active');
  input.value = (initialQuery || '').trim().replace(/\*+$/, '');
  runQuickSearch(input.value);
  setTimeout(() => { input.focus(); input.select(); }, 30);
}

function closeQuickSearchModal() {
  document.getElementById('quick-search-modal')?.classList.remove('active');
  quickSearchResults = [];
  quickSearchHighlightIndex = -1;
}

function runQuickSearch(rawQuery) {
  const query = turkishNormalize((rawQuery || '').trim());
  const tbody = document.getElementById('quick-search-tbody');
  let rowsHtml = '';

  if (quickSearchMode === 'customer') {
    quickSearchResults = liveCustomers.filter(c =>
      !query || turkishNormalize(c.code || '').includes(query) || turkishNormalize(c.name).includes(query)
    ).slice(0, 100);

    rowsHtml = quickSearchResults.map((c, idx) => `
      <tr data-idx="${idx}" onclick="selectQuickSearchItem(${idx})">
        <td><span class="item-code-badge">${escapeHtml(c.code || '')}</span></td>
        <td>${escapeHtml(c.name)}</td>
        <td class="text-end">${formatMoney(Math.max(c.debit - c.credit, 0))} ₺</td>
        <td class="text-end">${formatMoney(Math.max(c.credit - c.debit, 0))} ₺</td>
      </tr>`).join('');
  } else {
    const category = document.getElementById('picker-category').value;
    const pool = category === 'ALL' ? sahinCatalog : sahinCatalog.filter(i => i.category === category);
    quickSearchResults = pool.filter(i =>
      !query || turkishNormalize(i.id).includes(query) || turkishNormalize(i.name).includes(query)
    ).slice(0, 100);

    rowsHtml = quickSearchResults.map((i, idx) => `
      <tr data-idx="${idx}" onclick="selectQuickSearchItem(${idx})">
        <td><span class="item-code-badge">${escapeHtml(i.id)}</span></td>
        <td>${escapeHtml(i.name)}</td>
        <td>${escapeHtml(i.category || '')}</td>
        <td class="text-end">${formatMoney(i.price)} ₺</td>
        <td class="text-end">${formatMoney(i.stock)}</td>
      </tr>`).join('');
  }

  const colCount = quickSearchMode === 'customer' ? 4 : 5;
  tbody.innerHTML = rowsHtml || `<tr><td colspan="${colCount}" class="text-center text-secondary py-3">Sonuç bulunamadı.</td></tr>`;
  quickSearchHighlightIndex = quickSearchResults.length > 0 ? 0 : -1;
  highlightQuickSearchRow();
}

function highlightQuickSearchRow() {
  const tbody = document.getElementById('quick-search-tbody');
  if (!tbody) return;
  tbody.querySelectorAll('tr[data-idx]').forEach(row => {
    row.classList.toggle('highlighted', Number(row.dataset.idx) === quickSearchHighlightIndex);
  });
  tbody.querySelector('tr.highlighted')?.scrollIntoView({ block: 'nearest' });
}

function moveQuickSearchHighlight(delta) {
  if (quickSearchResults.length === 0) return;
  quickSearchHighlightIndex = Math.max(0, Math.min(quickSearchResults.length - 1, quickSearchHighlightIndex + delta));
  highlightQuickSearchRow();
}

function selectQuickSearchItem(idx) {
  const item = quickSearchResults[idx];
  if (!item) return;
  if (quickSearchMode === 'customer') {
    applySelectedCustomer(item);
  } else {
    selectProductFromSearch(item.id);
  }
  closeQuickSearchModal();
}

function handleQuickSearchKeydown(e) {
  if (e.key === 'Escape') { e.preventDefault(); closeQuickSearchModal(); return; }
  if (e.key === 'ArrowDown') { e.preventDefault(); moveQuickSearchHighlight(1); return; }
  if (e.key === 'ArrowUp') { e.preventDefault(); moveQuickSearchHighlight(-1); return; }
  if (e.key === 'Enter') {
    e.preventDefault();
    if (quickSearchHighlightIndex >= 0) selectQuickSearchItem(quickSearchHighlightIndex);
  }
}

// Müşteri & Firma alanlarında F9 ile cari arama penceresi açılır; Enter sıradaki alana geçer.
function handleCustomerFieldKeydown(e, input, nextId) {
  if (e.key === 'F9') {
    e.preventDefault();
    openQuickSearchModal('customer', input.value);
    return;
  }
  focusNextOnEnter(e, nextId);
}

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
    const data = await res.json().catch(() => null);
    if (res.ok && data && data.success) {
      newProduct = data;
    } else {
      alert('Özel kalem stok kartı olarak oluşturulamadı: ' + ((data && data.message) || 'Sunucu hatası. Lütfen tekrar deneyin.'));
      return;
    }
  } catch (err) {
    alert('Özel kalem stok kartı olarak oluşturulamadı: bağlantı hatası. Lütfen tekrar deneyin.');
    return;
  }

  const catalogEntry = {
    id: newProduct.code,
    dbId: newProduct.id,
    category: 'Yazılımlar',
    unit: newProduct.unit,
    price: price,
    stock: 0,
    kdv: newProduct.taxRate,
    name: name
  };

  // Yeni oluşan stok kartını kataloğa da ekle, sonraki tekliflerde aramada çıksın.
  sahinCatalog.push(catalogEntry);
  populateCategoryDropdowns();

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
function validateQuoteBeforeSave() {
  const company = document.getElementById('cust-company').value;
  const contact = document.getElementById('cust-contact').value;

  if (!company || !contact) {
    alert('Lütfen teklif oluşturmak için Firma Unvanı ve İlgili Kişi alanlarını doldurunuz.');
    return false;
  }

  if (currentQuoteItems.length === 0) {
    alert('Teklif oluşturmak için en az 1 adet ürün eklemelisiniz.');
    return false;
  }

  return true;
}

async function saveAndExportPdf() {
  if (!validateQuoteBeforeSave()) return;

  updatePdfPreview();

  let result;
  try {
    result = await saveQuoteToDatabase('Onaylandı / PDF İndirildi');
  } catch (err) {
    alert('Teklif veritabanına kaydedilemedi: ' + err.message);
    return;
  }

  const quoteNo = document.getElementById('quote-no').value;
  const company = document.getElementById('cust-company').value;
  const sanitizedCompany = company.replace(/[^a-zA-Z0-9]/g, '_');
  const filename = `Sahin_Bilisim_Teklif_${quoteNo}_${sanitizedCompany}.pdf`;

  try {
    await exportElementToPdf('pdf-template', filename);
    alert(`"${filename}" başarıyla oluşturuldu, veritabanına kaydedildi ve cihazınıza indirildi!`);
  } catch (err) {
    console.error('PDF Generation Error:', err);
    alert('Teklif veritabanına kaydedildi, ancak PDF oluşturulurken bir uyarı oluştu. "Doğrudan Yazdır" butonuyla yazdırabilirsiniz.');
  }

  refreshDashboardMetrics();
  offerInvoiceConversion(result);
}

async function saveAsDraft() {
  if (!validateQuoteBeforeSave()) return;

  let result;
  try {
    result = await saveQuoteToDatabase('Taslak');
  } catch (err) {
    alert('Teklif veritabanına kaydedilemedi: ' + err.message);
    return;
  }

  alert('Teklifiniz başarıyla veritabanına taslak olarak kaydedildi!');
  refreshDashboardMetrics();
  offerInvoiceConversion(result);
}

async function saveAndPrint() {
  if (!validateQuoteBeforeSave()) return;

  updatePdfPreview();

  try {
    await saveQuoteToDatabase('Onaylandı / Yazdırıldı');
  } catch (err) {
    alert('Teklif kaydedilemediği için yazdırma iptal edildi: ' + err.message);
    return;
  }

  refreshDashboardMetrics();
  window.print();
}

function offerInvoiceConversion(result) {
  if (result && result.id && confirm(`Teklif ${result.quoteNumber || ''} kaydedildi. Teklif detayına gidip satış faturasına dönüştürmek ister misiniz?`)) {
    window.location.href = '/Quotes/Details/' + result.id;
  }
}

// Teklifi gerçek SQL veritabanına kaydeder (SaveQuoteApi). Başarısız olursa hatayı fırlatır,
// hiçbir zaman sessizce başarılıymış gibi davranmaz.
async function saveQuoteToDatabase(status) {
  const quoteNo = document.getElementById('quote-no').value;
  const company = document.getElementById('cust-company').value;
  const contact = document.getElementById('cust-contact').value;
  const date = document.getElementById('quote-date').value;
  const companyInput = document.getElementById('cust-company');
  const customerId = companyInput ? parseInt(companyInput.dataset.customerId || '0') : 0;

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
    quoteNumber: quoteNo,
    customerId: customerId > 0 ? customerId : null,
    company: company,
    contact: contact,
    phone: document.getElementById('cust-phone').value || '',
    email: document.getElementById('cust-email').value || '',
    taxOffice: document.getElementById('cust-tax-office').value || '',
    address: document.getElementById('cust-address').value || '',
    quoteDate: date ? new Date(date).toISOString() : new Date().toISOString(),
    currencyCode: activeCurrency,
    exchangeRate: window.globalExchangeRates[activeCurrency] || 1,
    notes: document.getElementById('quote-note').value || '',
    grandTotal: grandTotal,
    status: status,
    items: JSON.parse(JSON.stringify(currentQuoteItems))
  };

  let res;
  try {
    res = await fetch('/Quotes/SaveQuoteApi', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(proposalObj)
    });
  } catch (err) {
    throw new Error('Sunucuya bağlanılamadı. İnternet bağlantınızı kontrol edin.');
  }

  const data = await res.json().catch(() => null);
  if (!res.ok || !data || !data.success) {
    throw new Error((data && data.message) || 'Sunucu hatası (oturumunuzun süresi dolmuş olabilir, sayfayı yenileyip tekrar deneyin).');
  }

  if (data.quoteNumber) {
    document.getElementById('quote-no').value = data.quoteNumber;
    document.getElementById('pdf-val-no').textContent = data.quoteNumber;
  }

  return data;
}

// --- 7. MODAL HANDLERS & TABLES ---
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
