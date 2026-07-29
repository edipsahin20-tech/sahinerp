using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class QuotesController(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator,
    BarcodeGeneratorService barcodeGenerator) : Controller
{
    public async Task<IActionResult> Index(QuoteStatus? status, string? search)
    {
        ViewBag.Status = status;
        ViewBag.Search = search;
        ViewBag.Warehouses = new List<SelectListItem>();

        try
        {
            var query = dbContext.Quotes
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Invoices)
                .OrderByDescending(x => x.QuoteDateUtc)
                .ThenByDescending(x => x.Id)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.QuoteNumber.Contains(search) || x.Customer.Name.Contains(search));
            }

            var quotes = await query.ToListAsync();

            ViewBag.Warehouses = await dbContext.Warehouses
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
                .ToListAsync();

            return View(quotes);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Teklif listesi yüklenirken hata oluştu: " + (ex.InnerException?.Message ?? ex.Message);
            return View(new List<Quote>());
        }
    }

    public async Task<IActionResult> Studio(int? id)
    {
        var settings = await dbContext.CompanySettings.AsNoTracking().SingleAsync(x => x.Id == 1);
        var model = new QuoteStudioViewModel
        {
            CompanyName = settings.CompanyName,
            Website = settings.Website,
            Email = settings.Email,
            Phone = settings.Phone,
            BankName = settings.BankName,
            Iban = settings.Iban
        };

        if (id is int quoteId)
        {
            var quote = await dbContext.Quotes
                .AsNoTracking()
                .Include(x => x.Lines)
                .Include(x => x.Customer).ThenInclude(x => x.Contacts)
                .SingleOrDefaultAsync(x => x.Id == quoteId);

            if (quote is null)
            {
                return NotFound();
            }

            if (quote.Status != QuoteStatus.Draft)
            {
                TempData["Error"] = "Yalnızca taslak teklifler Teklif Stüdyosu'nda düzenlenebilir.";
                return RedirectToAction(nameof(Details), new { id = quoteId });
            }

            var primaryContact = quote.Customer.Contacts.FirstOrDefault(c => c.IsPrimary)
                ?? quote.Customer.Contacts.FirstOrDefault();

            model.Existing = new QuoteStudioExistingData
            {
                Id = quote.Id,
                QuoteNumber = quote.QuoteNumber,
                CustomerId = quote.CustomerId,
                Company = quote.Customer.Name,
                Contact = primaryContact?.FullName,
                Phone = quote.Customer.Phone,
                Email = quote.Customer.Email,
                TaxOffice = quote.Customer.TaxOffice != null && quote.Customer.TaxNumber != null
                    ? quote.Customer.TaxOffice + " " + quote.Customer.TaxNumber
                    : quote.Customer.TaxOffice ?? quote.Customer.TaxNumber,
                Address = quote.Customer.Address,
                QuoteDate = quote.QuoteDateUtc.ToString("yyyy-MM-dd"),
                CurrencyCode = quote.CurrencyCode,
                Notes = quote.Notes,
                AmountDiscount = quote.AmountDiscount,
                Items = quote.Lines
                    .OrderBy(x => x.LineNumber)
                    .Select(x => new QuoteStudioExistingItem
                    {
                        DbId = x.ProductId,
                        Code = x.ProductCodeSnapshot,
                        Name = x.ProductNameSnapshot,
                        Unit = x.UnitSnapshot,
                        Qty = x.Quantity,
                        Price = x.UnitPrice,
                        Kdv = x.TaxRate,
                        Discount = x.DiscountRate
                    })
                    .ToList()
            };
        }

        return View(model);
    }

    public async Task<IActionResult> GetCatalogDataApi(int? customerId)
    {
        // Carinin özel fiyat listesinde bu ürün için bir kayıt varsa, stok kartındaki genel
        // fiyat yerine o kullanılır (stok kartı hiçbir zaman değişmez, sadece öneri kaynağı değişir).
        // Bir ürün için birden fazla tarihli kayıt olabileceğinden (fiyat geçmişi), en güncel (son) kayıt seçilir.
        var specialPrices = customerId.HasValue
            ? await dbContext.SalesPriceListItems
                .AsNoTracking()
                .Where(x => x.SalesPriceList.CustomerId == customerId.Value && x.ProductVariantId == null && x.MinimumQuantity == 1)
                .GroupBy(x => x.ProductId)
                .Select(g => new { ProductId = g.Key, UnitPrice = g.OrderByDescending(x => x.CreatedAtUtc).Select(x => x.UnitPrice).First() })
                .ToDictionaryAsync(x => x.ProductId, x => x.UnitPrice)
            : [];

        var products = await dbContext.Products
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.Category)
            .Include(x => x.TaxRate)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                id = x.StockCode,
                dbId = x.Id,
                name = x.Name,
                category = x.Category.Name,
                unit = x.Unit,
                salePrice = x.SalePrice,
                taxRate = x.TaxRate.Rate,
                stock = x.StockQuantity
            })
            .ToListAsync();

        var result = products.Select(x =>
        {
            var hasSpecialPrice = specialPrices.TryGetValue(x.dbId, out var specialPrice);
            var grossPrice = hasSpecialPrice ? specialPrice : x.salePrice;
            return new
            {
                x.id,
                x.dbId,
                x.name,
                x.category,
                x.unit,
                // price: arama sonuçlarında gösterilen KDV dahil tutar (carinin özel fiyatı varsa o, yoksa stok kartı).
                // unitPrice: Birim Fiyat alanına yazılacak KDV hariç tutar.
                price = grossPrice,
                unitPrice = Math.Round(grossPrice / (1 + x.taxRate / 100), 3, MidpointRounding.AwayFromZero),
                stock = x.stock,
                kdv = x.taxRate,
                hasSpecialPrice
            };
        });

        return Json(new { products = result });
    }

    public async Task<IActionResult> GetCustomersApi()
    {
        var customers = await dbContext.Customers
            .AsNoTracking()
            .Where(x => x.IsActive && x.IsCustomer)
            .Include(x => x.Contacts)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                id = x.Id,
                code = x.Code,
                name = x.Name,
                company = x.Name,
                contact = x.Contacts.Where(c => c.IsPrimary).Select(c => c.FullName).FirstOrDefault()
                    ?? x.Contacts.Select(c => c.FullName).FirstOrDefault(),
                phone = x.Phone,
                email = x.Email,
                taxOffice = x.TaxOffice != null && x.TaxNumber != null
                    ? x.TaxOffice + " " + x.TaxNumber
                    : x.TaxOffice ?? x.TaxNumber,
                address = x.Address,
                debit = dbContext.CurrentAccountTransactions.Where(t => t.CustomerId == x.Id).Sum(t => (decimal?)t.Debit) ?? 0,
                credit = dbContext.CurrentAccountTransactions.Where(t => t.CustomerId == x.Id).Sum(t => (decimal?)t.Credit) ?? 0
            })
            .ToListAsync();

        return Json(customers);
    }

    public async Task<IActionResult> GetQuoteMetricsApi()
    {
        var totalCount = await dbContext.Quotes.CountAsync();
        var approvedCount = await dbContext.Quotes.CountAsync(x => x.Status == QuoteStatus.Approved || x.Status == QuoteStatus.Sent);
        var totalVolume = await dbContext.Quotes.SumAsync(x => (decimal?)x.GrandTotal) ?? 0;

        return Json(new { totalCount, approvedCount, totalVolume });
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomProductApi([FromBody] CustomProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Json(new { success = false, message = "Ürün/hizmet adı zorunludur." });
        }

        try
        {
            var category = await dbContext.ProductCategories.FirstOrDefaultAsync(x => x.Code == "YAZILIM" && x.IsActive)
                ?? await dbContext.ProductCategories.OrderBy(x => x.Id).FirstOrDefaultAsync(x => x.IsActive);
            var taxRate = await dbContext.TaxRates.FirstOrDefaultAsync(x => x.Code == "KDV20" && x.IsActive)
                ?? await dbContext.TaxRates.OrderBy(x => x.Id).FirstOrDefaultAsync(x => x.IsActive);

            if (category is null || taxRate is null)
            {
                return Json(new { success = false, message = "Varsayılan kategori veya KDV oranı bulunamadı." });
            }

            // Teklif Stüdyosu'nda "Birim Fiyat" olarak girilen tutar KDV hariçtir; ürün kartına
            // (fiyatlar her zaman KDV dahil saklanır kuralına uygun) KDV dahil hale çevrilerek kaydedilir.
            var salePriceInclTax = RoundMoney(request.Price * (1 + taxRate.Rate / 100));

            var product = new Product
            {
                StockCode = await documentNumberGenerator.GenerateAsync("STOCK"),
                Barcode = await barcodeGenerator.GenerateEan13Async(),
                Name = request.Name.Trim(),
                CategoryId = category.Id,
                TaxRateId = taxRate.Id,
                ProductType = "Hizmet",
                Unit = "Adet",
                TrackStock = false,
                SalePrice = salePriceInclTax,
                PurchasePrice = 0,
                IsActive = true
            };

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            return Json(new
            {
                success = true,
                id = product.Id,
                code = product.StockCode,
                barcode = product.Barcode,
                name = product.Name,
                salePrice = product.SalePrice,
                purchasePrice = product.PurchasePrice,
                taxRate = taxRate.Rate,
                unit = product.Unit
            });
        }
        catch (Exception ex)
        {
            var detail = ex.InnerException?.Message ?? ex.Message;
            return Json(new { success = false, message = "Sunucu hatası: " + detail });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveQuoteApi([FromBody] QuoteStudioSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Company) || request.Items.Count == 0)
        {
            return Json(new { success = false, message = "Firma unvanı ve en az bir kalem zorunludur." });
        }

        try
        {
            Customer? customer = request.CustomerId is int customerId
                ? await dbContext.Customers.SingleOrDefaultAsync(x => x.Id == customerId)
                : null;

            // Aynı ünvanla daha önce otomatik kaydedilmiş bir cari varsa, mükerrer kayıt açmak yerine onu kullan.
            var companyName = request.Company.Trim();
            customer ??= await dbContext.Customers
                .FirstOrDefaultAsync(x => EF.Functions.Collate(x.Name, "Turkish_CI_AI") == companyName);

            if (customer is null)
            {
                customer = new Customer
                {
                    Code = await documentNumberGenerator.GenerateAsync("CUSTOMER"),
                    Name = request.Company.Trim(),
                    Phone = request.Phone,
                    Email = request.Email,
                    Address = request.Address,
                    TaxOffice = request.TaxOffice,
                    IsCustomer = true,
                    IsActive = true
                };
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var quoteDateUtc = request.QuoteDate.HasValue
                ? DateTime.SpecifyKind(request.QuoteDate.Value, DateTimeKind.Utc)
                : DateTime.UtcNow;

            // Zaten kaydedilmiş bir taslak tekrar "Kaydet" ile güncelleniyorsa, yeni bir teklif
            // açmak yerine mevcut kaydı ve kalemlerini günceller (aynı taslağa tekrar kaydet mükerrer kayıt açmasın).
            Quote? quote = null;
            if (request.Id is int existingId)
            {
                quote = await dbContext.Quotes
                    .Include(x => x.Lines)
                    .SingleOrDefaultAsync(x => x.Id == existingId && x.Status == QuoteStatus.Draft);
                if (quote is not null)
                {
                    dbContext.QuoteLines.RemoveRange(quote.Lines);
                    quote.Lines.Clear();
                }
            }

            var isNew = quote is null;
            quote ??= new Quote
            {
                QuoteNumber = await documentNumberGenerator.GenerateAsync("QUOTE"),
                CreatedByUserId = userId
            };

            quote.QuoteDateUtc = quoteDateUtc;
            quote.ValidUntilUtc = quoteDateUtc.AddDays(15);
            quote.Status = request.Status.Contains("Onay", StringComparison.OrdinalIgnoreCase) ? QuoteStatus.Sent : QuoteStatus.Draft;
            quote.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            quote.ExchangeRate = request.ExchangeRate;
            quote.Notes = request.Notes?.Trim();
            quote.CustomerId = customer.Id;

            // 1. geçiş: satır bazlı (%) iskonto sonrası net tutarlar hesaplanır; genel tutar
            // iskontosunun satırlara oranlı dağıtımı için bu netlerin toplamına ihtiyaç var.
            var lineCalc = request.Items
                .Select(item =>
                {
                    var gross = RoundMoney(item.Qty * item.Price);
                    var lineDiscAmount = RoundMoney(gross * item.Discount / 100);
                    var netAfterLineDiscount = gross - lineDiscAmount;
                    return (item, gross, lineDiscAmount, netAfterLineDiscount);
                })
                .ToList();

            var netAfterLineDiscountTotal = lineCalc.Sum(x => x.netAfterLineDiscount);
            // Tutar iskontosu, satır iskontoları sonrası net toplamdan fazla olamaz (negatif matrah oluşmasın).
            var amountDiscount = Math.Clamp(request.AmountDiscount, 0, Math.Max(netAfterLineDiscountTotal, 0));

            var lineNumber = 1;
            decimal subtotal = 0, discountTotal = 0, taxTotal = 0, grandTotal = 0, allocatedAmountDiscount = 0;
            for (var i = 0; i < lineCalc.Count; i++)
            {
                var (item, gross, lineDiscAmount, netAfterLineDiscount) = lineCalc[i];

                // Genel tutar iskontosu, KDV matrahını doğru düşürmek için her satırın net tutar
                // payına göre orantılı dağıtılır; son satır yuvarlama farkını emer.
                decimal amountDiscShare;
                if (i == lineCalc.Count - 1)
                {
                    amountDiscShare = RoundMoney(amountDiscount - allocatedAmountDiscount);
                }
                else
                {
                    var ratio = netAfterLineDiscountTotal > 0 ? netAfterLineDiscount / netAfterLineDiscountTotal : 0;
                    amountDiscShare = RoundMoney(amountDiscount * ratio);
                }
                allocatedAmountDiscount += amountDiscShare;

                var totalDiscAmount = lineDiscAmount + amountDiscShare;
                var net = gross - totalDiscAmount;
                var taxAmount = RoundMoney(net * item.Kdv / 100);
                var lineTotal = net + taxAmount;

                subtotal += gross;
                discountTotal += totalDiscAmount;
                taxTotal += taxAmount;
                grandTotal += lineTotal;

                quote.Lines.Add(new QuoteLine
                {
                    LineNumber = lineNumber++,
                    ProductId = item.DbId,
                    ProductCodeSnapshot = item.Code ?? string.Empty,
                    ProductNameSnapshot = item.Name,
                    UnitSnapshot = item.Unit,
                    Quantity = item.Qty,
                    UnitPrice = item.Price,
                    DiscountRate = item.Discount,
                    DiscountAmount = totalDiscAmount,
                    TaxRate = item.Kdv,
                    TaxAmount = taxAmount,
                    LineTotal = lineTotal
                });
            }

            quote.Subtotal = RoundMoney(subtotal);
            quote.DiscountTotal = RoundMoney(discountTotal);
            quote.AmountDiscount = amountDiscount;
            quote.TaxTotal = RoundMoney(taxTotal);
            quote.GrandTotal = RoundMoney(grandTotal);

            if (isNew)
            {
                dbContext.Quotes.Add(quote);
            }

            // Bu caride kullanılan (KDV dahil, tutar iskontosundan bağımsız birim liste fiyatı) stok
            // kartını hiç etkilemeden carinin özel fiyat listesine (tarihiyle birlikte) yeni bir
            // geçmiş kaydı olarak eklenir — fiyat bir öncekiyle aynıysa tekrar satır açılmaz.
            var linkedItems = request.Items.Where(x => x.DbId.HasValue).ToList();
            if (linkedItems.Count > 0)
            {
                var customerPriceList = await GetOrCreateCustomerPriceListAsync(customer.Id);
                foreach (var item in linkedItems)
                {
                    var productId = item.DbId!.Value;
                    var priceInclTax = RoundMoney(item.Price * (1 + item.Kdv / 100));
                    var latestPriceItem = customerPriceList.Items
                        .Where(x => x.ProductId == productId && x.ProductVariantId == null && x.MinimumQuantity == 1)
                        .OrderByDescending(x => x.CreatedAtUtc)
                        .FirstOrDefault();
                    if (latestPriceItem is null || latestPriceItem.UnitPrice != priceInclTax)
                    {
                        customerPriceList.Items.Add(new SalesPriceListItem
                        {
                            ProductId = productId,
                            MinimumQuantity = 1,
                            UnitPrice = priceInclTax
                        });
                    }
                }
            }

            await dbContext.SaveChangesAsync();

            return Json(new { success = true, id = quote.Id, quoteNumber = quote.QuoteNumber });
        }
        catch (Exception ex)
        {
            var detail = ex.InnerException?.Message ?? ex.Message;
            return Json(new { success = false, message = "Sunucu hatası: " + detail });
        }
    }

    public IActionResult Create()
    {
        return RedirectToAction(nameof(Studio));
    }

    // Kaydedilmiş bir taslak teklifi düzenlemek artık Teklif Stüdyosu üzerinden yapılıyor
    // (ayrı, eski bir form sayfası yerine tek, tutarlı bir düzenleme deneyimi).
    public IActionResult Edit(int id)
    {
        return RedirectToAction(nameof(Studio), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var quote = await dbContext.Quotes
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        if (quote.Status != QuoteStatus.Draft)
        {
            TempData["Error"] = "Yalnızca taslak teklifler silinebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        dbContext.Quotes.Remove(quote);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Teklif silindi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var quote = await dbContext.Quotes
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Lines)
            .Include(x => x.Invoices)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        var model = new QuoteDetailsViewModel
        {
            Id = quote.Id,
            Status = quote.Status,
            QuoteNumber = quote.QuoteNumber,
            QuoteDateUtc = quote.QuoteDateUtc,
            ValidUntilUtc = quote.ValidUntilUtc,
            CustomerName = quote.Customer.Name,
            CurrencyCode = quote.CurrencyCode,
            Subtotal = quote.Subtotal,
            DiscountTotal = quote.DiscountTotal,
            AmountDiscount = quote.AmountDiscount,
            TaxTotal = quote.TaxTotal,
            GrandTotal = quote.GrandTotal,
            Notes = quote.Notes,
            HasConvertedInvoice = quote.Invoices.Any(x => x.Status != InvoiceStatus.Cancelled),
            Lines = quote.Lines
                .OrderBy(x => x.LineNumber)
                .Select(x => new QuoteDetailsLineViewModel
                {
                    Id = x.Id,
                    ProductNameSnapshot = x.ProductNameSnapshot,
                    UnitSnapshot = x.UnitSnapshot,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    DiscountRate = x.DiscountRate,
                    TaxRate = x.TaxRate,
                    LineTotal = x.LineTotal
                })
                .ToList(),
            TaxBreakdown = quote.Lines
                .GroupBy(x => x.TaxRate)
                .OrderBy(g => g.Key)
                .Select(g => new QuoteTaxBreakdownViewModel { TaxRate = g.Key, TaxAmount = g.Sum(x => x.TaxAmount) })
                .ToList(),
            Warehouses = await dbContext.Warehouses
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
                .ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int id)
    {
        var quote = await dbContext.Quotes.SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        if (quote.Status != QuoteStatus.Draft)
        {
            TempData["Error"] = "Yalnızca taslak teklifler gönderilebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        quote.Status = QuoteStatus.Sent;
        quote.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Teklif gönderildi olarak işaretlendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, int[]? approvedLineIds)
    {
        var quote = await dbContext.Quotes.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        if (quote.Status is not (QuoteStatus.Draft or QuoteStatus.Sent))
        {
            TempData["Error"] = "Bu teklif onaylanamaz.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Müşteri sadece bazı kalemleri onayladıysa (checkbox'lardan işaretsiz bırakılanlar),
        // onaylanmayan satırlar teklften tamamen silinir ve toplamlar kalan satırlara göre
        // yeniden hesaplanır (her satırın DiscountAmount/TaxAmount/LineTotal'ı zaten kendi
        // nihai — tutar iskontosu dahil — değerini taşıdığı için basit toplam yeterli).
        if (approvedLineIds is not null)
        {
            var approvedSet = approvedLineIds.ToHashSet();
            if (approvedSet.Count == 0)
            {
                TempData["Error"] = "Onaylamak için en az bir kalem seçili olmalı.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (approvedSet.Count < quote.Lines.Count)
            {
                var linesToRemove = quote.Lines.Where(x => !approvedSet.Contains(x.Id)).ToList();
                foreach (var line in linesToRemove)
                {
                    quote.Lines.Remove(line);
                    dbContext.QuoteLines.Remove(line);
                }

                quote.Subtotal = RoundMoney(quote.Lines.Sum(x => x.Quantity * x.UnitPrice));
                quote.DiscountTotal = RoundMoney(quote.Lines.Sum(x => x.DiscountAmount));
                quote.TaxTotal = RoundMoney(quote.Lines.Sum(x => x.TaxAmount));
                quote.GrandTotal = RoundMoney(quote.Lines.Sum(x => x.LineTotal));
            }
        }

        quote.Status = QuoteStatus.Approved;
        quote.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Teklif, müşteri onayladı olarak işaretlendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? reason)
    {
        var quote = await dbContext.Quotes.SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        if (quote.Status is not (QuoteStatus.Draft or QuoteStatus.Sent))
        {
            TempData["Error"] = "Bu teklif reddedilemez.";
            return RedirectToAction(nameof(Details), new { id });
        }

        quote.Status = QuoteStatus.Rejected;
        quote.UpdatedAtUtc = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            var rejectionNote = $"Müşteri onaylamadı ({DateTime.UtcNow:dd.MM.yyyy}): {reason.Trim()}";
            quote.Notes = string.IsNullOrWhiteSpace(quote.Notes) ? rejectionNote : $"{quote.Notes}\n{rejectionNote}";
        }
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Teklif, müşteri onaylamadı olarak işaretlendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevertToDraft(int id)
    {
        var quote = await dbContext.Quotes
            .Include(x => x.Invoices)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        if (quote.Status is not (QuoteStatus.Sent or QuoteStatus.Approved or QuoteStatus.Rejected))
        {
            TempData["Error"] = "Bu teklif taslağa çevrilemez.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Faturaya dönüştürülmüş ama sonradan iptal edilmiş bir teklif, satış gerçekleşmediği
        // için tekrar taslağa çevrilip düzenlenebilmeli — sadece hâlâ geçerli (iptal edilmemiş)
        // bir fatura varsa engellenir.
        if (quote.Invoices.Any(x => x.Status != InvoiceStatus.Cancelled))
        {
            TempData["Error"] = "Bu teklif zaten faturaya dönüştürülmüş, taslağa çevrilemez.";
            return RedirectToAction(nameof(Details), new { id });
        }

        quote.Status = QuoteStatus.Draft;
        quote.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Teklif taslağa çevrildi, artık düzenlenebilir.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConvertToInvoice(int id, int warehouseId)
    {
        var quote = await dbContext.Quotes
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (quote is null)
        {
            return NotFound();
        }

        if (quote.Status != QuoteStatus.Approved)
        {
            TempData["Error"] = "Yalnızca onaylanmış teklifler faturaya dönüştürülebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var invoice = await BuildInvoiceFromQuoteAsync(quote, warehouseId);
        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Teklif satış faturasına dönüştürüldü. Taslağı gözden geçirip onaylayabilirsiniz.";
        return RedirectToAction("Details", "Invoices", new { id = invoice.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkConvertToInvoice(int[] ids, int warehouseId)
    {
        if (ids is null || ids.Length == 0)
        {
            TempData["Error"] = "Faturaya dönüştürmek için en az bir onaylı teklif seçmelisiniz.";
            return RedirectToAction(nameof(Index));
        }

        var quotes = await dbContext.Quotes
            .Include(x => x.Lines)
            .Where(x => ids.Contains(x.Id) && x.Status == QuoteStatus.Approved)
            .ToListAsync();

        var createdCount = 0;
        foreach (var quote in quotes)
        {
            var invoice = await BuildInvoiceFromQuoteAsync(quote, warehouseId);
            dbContext.Invoices.Add(invoice);
            createdCount++;
        }

        if (createdCount > 0)
        {
            await dbContext.SaveChangesAsync();
        }

        var skippedCount = ids.Length - createdCount;
        TempData["Success"] = skippedCount > 0
            ? $"{createdCount} teklif satış faturasına dönüştürüldü. {skippedCount} teklif onaylı olmadığı için atlandı."
            : $"{createdCount} teklif satış faturasına dönüştürüldü.";
        return RedirectToAction("Index", "Invoices", new { type = InvoiceType.Sales });
    }

    private async Task<Invoice> BuildInvoiceFromQuoteAsync(Quote quote, int warehouseId)
    {
        var invoice = new Invoice
        {
            InvoiceType = InvoiceType.Sales,
            Status = InvoiceStatus.Draft,
            InvoiceNumber = await documentNumberGenerator.GenerateAsync("SALES_INVOICE"),
            CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            CustomerId = quote.CustomerId,
            WarehouseId = warehouseId,
            InvoiceDateUtc = DateTime.UtcNow,
            CurrencyCode = quote.CurrencyCode,
            ExchangeRate = quote.ExchangeRate,
            QuoteId = quote.Id,
            Notes = quote.Notes
        };

        var lineNumber = 1;
        foreach (var line in quote.Lines.OrderBy(x => x.LineNumber))
        {
            // Fatura, teklifin satır iskontosu (%) alanını değil, teklifte fiilen uygulanan toplam
            // iskonto tutarını (satır iskontosu + orantılı tutar iskontosu payı) aynen üretecek
            // "etkin" bir iskonto oranı kullanır — böylece teklifteki genel tutar iskontosunun KDV
            // matrahına etkisi, Fatura şemasına yeni alan eklemeden faturaya da aynen taşınır.
            var gross = line.Quantity * line.UnitPrice;
            var effectiveDiscountRate = gross > 0
                ? Math.Round(line.DiscountAmount / gross * 100, 4, MidpointRounding.AwayFromZero)
                : 0;

            invoice.Lines.Add(new InvoiceLine
            {
                LineNumber = lineNumber++,
                ProductId = line.ProductId,
                ProductCodeSnapshot = line.ProductCodeSnapshot,
                ProductNameSnapshot = line.ProductNameSnapshot,
                UnitSnapshot = line.UnitSnapshot,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                DiscountRate = effectiveDiscountRate,
                TaxRate = line.TaxRate,
                Description = line.Description
            });
        }

        return invoice;
    }

    private async Task<SalesPriceList> GetOrCreateCustomerPriceListAsync(int customerId)
    {
        var list = await dbContext.SalesPriceLists
            .Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.CustomerId == customerId);

        if (list is null)
        {
            list = new SalesPriceList
            {
                Code = $"CFL.{customerId:D5}",
                Name = "Cari Özel Fiyat Listesi",
                CurrencyCode = "TRY",
                ValidFromUtc = DateTime.UtcNow,
                IsActive = true,
                CustomerId = customerId
            };
            dbContext.SalesPriceLists.Add(list);
            await dbContext.SaveChangesAsync();
        }

        return list;
    }

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
