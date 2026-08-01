using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Common;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize]
public sealed class CustomersController(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator) : Controller
{
    public async Task<IActionResult> Index(string? search, bool? isActive)
    {
        var query = dbContext.Customers
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Code.Contains(search) ||
                x.Name.Contains(search) ||
                (x.TaxNumber != null && x.TaxNumber.Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var customers = await query.ToListAsync();

        // Borç/Alacak, cari kartında ayrıca saklanmaz — her zaman hareket tablosundan (ledger'dan)
        // canlı hesaplanır, böylece stok miktarındaki gibi bir senkron kopması riski olmaz.
        var balances = await dbContext.CurrentAccountTransactions
            .AsNoTracking()
            .GroupBy(x => x.CustomerId)
            .Select(g => new { CustomerId = g.Key, Debit = g.Sum(x => x.Debit), Credit = g.Sum(x => x.Credit) })
            .ToDictionaryAsync(x => x.CustomerId);

        var model = customers.Select(x => new CustomerListItemViewModel
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            City = x.City,
            District = x.District,
            TaxNumber = x.TaxNumber,
            Phone = x.Phone,
            IsCustomer = x.IsCustomer,
            IsSupplier = x.IsSupplier,
            IsActive = x.IsActive,
            Debit = balances.TryGetValue(x.Id, out var b) ? b.Debit : 0,
            Credit = balances.TryGetValue(x.Id, out var b2) ? b2.Credit : 0
        }).ToList();

        ViewBag.Search = search;
        ViewBag.IsActive = isActive;
        return View(model);
    }

    public IActionResult Create()
    {
        ViewBag.Toolbar = new EvrakToolbarViewModel { Controller = "Customers" };
        return View("Form", new CustomerFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerFormViewModel form)
    {
        if (string.IsNullOrWhiteSpace(form.Code))
        {
            form.Code = await documentNumberGenerator.GenerateAsync("CUSTOMER");
        }
        await ValidateUniqueCodeAsync(form);
        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var customer = new Customer();
        Map(form, customer);
        dbContext.Customers.Add(customer);

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        // Ürün kartındaki açılış stok hareketi ile aynı desen: bakiye Customer üzerinde
        // ayrıca saklanmaz (her zaman CurrentAccountTransactions'tan hesaplanır), sadece
        // açılışta tek seferlik bir "Açılış" hareketi yazılır.
        if (form.OpeningBalance != 0)
        {
            dbContext.CurrentAccountTransactions.Add(new CurrentAccountTransaction
            {
                TransactionDateUtc = DateTime.UtcNow,
                TransactionType = CurrentAccountTransactionType.Opening,
                DocumentNumber = $"ACILIS-{customer.Code}",
                Debit = form.OpeningBalance > 0 ? form.OpeningBalance : 0,
                Credit = form.OpeningBalance < 0 ? -form.OpeningBalance : 0,
                Description = "Cari kartı açılış bakiyesi",
                CustomerId = customer.Id
            });
            await dbContext.SaveChangesAsync();
        }

        TempData["Success"] = "Cari kaydedildi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var customer = await dbContext.Customers.SingleOrDefaultAsync(x => x.Id == id);
        if (customer is null)
        {
            return NotFound();
        }

        var model = new CustomerFormViewModel
        {
            Id = customer.Id,
            Code = customer.Code,
            Name = customer.Name,
            AccountType = customer.AccountType,
            TaxOffice = customer.TaxOffice,
            TaxNumber = customer.TaxNumber,
            IdentityNumber = customer.IdentityNumber,
            CustomerGroup = customer.CustomerGroup,
            RiskLimit = customer.RiskLimit,
            DefaultPaymentTermDays = customer.DefaultPaymentTermDays,
            AuthorizedPerson = customer.AuthorizedPerson,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            City = customer.City,
            District = customer.District,
            Notes = customer.Notes,
            IsCustomer = customer.IsCustomer,
            IsSupplier = customer.IsSupplier,
            IsActive = customer.IsActive
        };

        await SetToolbarAsync(id);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await dbContext.Customers.SingleOrDefaultAsync(x => x.Id == id);
        if (customer is null)
        {
            return NotFound();
        }

        var hasMovements = await dbContext.CurrentAccountTransactions.AnyAsync(x => x.CustomerId == id);
        if (hasMovements)
        {
            TempData["Error"] = "Bu cariye ait hareketler var, silinemez.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        dbContext.Customers.Remove(customer);
        try
        {
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Cari silindi.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Bu cariye bağlı kayıtlar var, silinemez.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SetToolbarAsync(int id)
    {
        var previousId = await dbContext.Customers.Where(x => x.Id < id).OrderByDescending(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var nextId = await dbContext.Customers.Where(x => x.Id > id).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        var hasMovements = await dbContext.CurrentAccountTransactions.AnyAsync(x => x.CustomerId == id);

        ViewBag.Toolbar = new EvrakToolbarViewModel
        {
            Id = id,
            Controller = "Customers",
            PreviousId = previousId,
            NextId = nextId,
            CanDelete = !hasMovements,
            DeleteBlockedReason = hasMovements ? "Bu cariye ait hareketler var, silinemez." : null
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CustomerFormViewModel form)
    {
        if (id != form.Id)
        {
            return BadRequest();
        }

        await ValidateUniqueCodeAsync(form);
        if (!ModelState.IsValid)
        {
            return View("Form", form);
        }

        var customer = await dbContext.Customers.SingleOrDefaultAsync(x => x.Id == id);
        if (customer is null)
        {
            return NotFound();
        }

        Map(form, customer);
        customer.UpdatedAtUtc = DateTime.UtcNow;

        if (!await TrySaveAsync())
        {
            return View("Form", form);
        }

        TempData["Success"] = "Cari güncellendi.";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> Statement(int id, DateTime? from, DateTime? to)
    {
        var customer = await dbContext.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);
        if (customer is null)
        {
            return NotFound();
        }

        var transactionsQuery = dbContext.CurrentAccountTransactions
            .AsNoTracking()
            .Where(x => x.CustomerId == id);

        var openingBalance = from.HasValue
            ? await transactionsQuery
                .Where(x => x.TransactionDateUtc < from.Value)
                .SumAsync(x => (decimal?)(x.Debit - x.Credit)) ?? 0
            : 0;

        var rangeQuery = transactionsQuery;
        if (from.HasValue)
        {
            rangeQuery = rangeQuery.Where(x => x.TransactionDateUtc >= from.Value);
        }
        if (to.HasValue)
        {
            rangeQuery = rangeQuery.Where(x => x.TransactionDateUtc < to.Value.AddDays(1));
        }

        var transactions = await rangeQuery
            .OrderBy(x => x.TransactionDateUtc)
            .ThenBy(x => x.Id)
            .ToListAsync();

        var runningBalance = openingBalance;
        var lines = new List<CustomerStatementLineViewModel>();
        foreach (var transaction in transactions)
        {
            runningBalance += transaction.Debit - transaction.Credit;
            lines.Add(new CustomerStatementLineViewModel
            {
                TransactionDateUtc = transaction.TransactionDateUtc,
                TransactionType = transaction.TransactionType.GetDisplayName(),
                DocumentNumber = transaction.DocumentNumber,
                Description = transaction.Description,
                Debit = transaction.Debit,
                Credit = transaction.Credit,
                RunningBalance = runningBalance
            });
        }

        var model = new CustomerStatementViewModel
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerCode = customer.Code,
            From = from,
            To = to,
            OpeningBalance = openingBalance,
            ClosingBalance = runningBalance,
            Lines = lines
        };

        return View(model);
    }

    public async Task<IActionResult> PriceList(int id)
    {
        var customer = await dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (customer is null)
        {
            return NotFound();
        }

        var rawItems = await dbContext.SalesPriceListItems
            .AsNoTracking()
            .Include(x => x.Product)
            .Where(x => x.SalesPriceList.CustomerId == id)
            .ToListAsync();

        var latestIdsByProduct = rawItems
            .GroupBy(x => x.ProductId)
            .Select(g => g.OrderByDescending(x => x.CreatedAtUtc).First().Id)
            .ToHashSet();

        var items = rawItems
            .OrderBy(x => x.Product.Name)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Select(x => new CustomerPriceListItemViewModel
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductCode = x.Product.StockCode,
                ProductName = x.Product.Name,
                UnitPrice = x.UnitPrice,
                UpdatedAtUtc = x.CreatedAtUtc,
                IsLatest = latestIdsByProduct.Contains(x.Id)
            })
            .ToList();

        var model = new CustomerPriceListViewModel
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerCode = customer.Code,
            Items = items
        };

        return View(model);
    }

    public async Task<IActionResult> PriceLists(string? search)
    {
        var rawItems = await dbContext.SalesPriceListItems
            .AsNoTracking()
            .Include(x => x.Product)
            .Include(x => x.SalesPriceList).ThenInclude(x => x.Customer)
            .Where(x => x.SalesPriceList.CustomerId != null)
            .ToListAsync();

        var latestIdsByCustomerProduct = rawItems
            .GroupBy(x => new { x.SalesPriceList.CustomerId, x.ProductId })
            .Select(g => g.OrderByDescending(x => x.CreatedAtUtc).First().Id)
            .ToHashSet();

        var items = rawItems
            .Select(x => new AllCustomerPriceListItemViewModel
            {
                Id = x.Id,
                CustomerId = x.SalesPriceList.CustomerId!.Value,
                CustomerCode = x.SalesPriceList.Customer!.Code,
                CustomerName = x.SalesPriceList.Customer!.Name,
                ProductId = x.ProductId,
                ProductCode = x.Product.StockCode,
                ProductName = x.Product.Name,
                UnitPrice = x.UnitPrice,
                UpdatedAtUtc = x.CreatedAtUtc,
                IsLatest = latestIdsByCustomerProduct.Contains(x.Id)
            })
            .Where(x => x.IsLatest)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim();
            items = items.Where(x =>
                x.CustomerName.Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
                x.CustomerCode.Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
                x.ProductName.Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
                x.ProductCode.Contains(normalized, StringComparison.OrdinalIgnoreCase));
        }

        ViewBag.Search = search;
        return View(items.ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPriceListItem(int customerId, int productId, decimal unitPrice)
    {
        if (unitPrice < 0)
        {
            TempData["Error"] = "Fiyat sıfırdan küçük olamaz.";
            return RedirectToAction(nameof(PriceList), new { id = customerId });
        }

        var product = await dbContext.Products.SingleOrDefaultAsync(x => x.Id == productId);
        if (product is null)
        {
            TempData["Error"] = "Ürün bulunamadı.";
            return RedirectToAction(nameof(PriceList), new { id = customerId });
        }

        // Her ekleme yeni bir geçmiş kaydı oluşturur (önceki fiyatın üzerine yazmaz) — böylece
        // bir ürün için zaman içindeki fiyat değişimleri (ör. döviz kuru dalgalanması) görülebilir.
        var priceList = await GetOrCreateCustomerPriceListAsync(customerId);
        priceList.Items.Add(new SalesPriceListItem
        {
            ProductId = productId,
            MinimumQuantity = 1,
            UnitPrice = unitPrice
        });

        await dbContext.SaveChangesAsync();
        TempData["Success"] = $"{product.Name} için özel fiyat kaydedildi.";
        return RedirectToAction(nameof(PriceList), new { id = customerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePriceListItem(int id, int customerId)
    {
        var item = await dbContext.SalesPriceListItems.SingleOrDefaultAsync(x => x.Id == id);
        if (item is not null)
        {
            dbContext.SalesPriceListItems.Remove(item);
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Özel fiyat kaldırıldı.";
        }

        return RedirectToAction(nameof(PriceList), new { id = customerId });
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

    private async Task ValidateUniqueCodeAsync(CustomerFormViewModel model)
    {
        if (await dbContext.Customers.AnyAsync(x => x.Code == model.Code && x.Id != model.Id))
        {
            ModelState.AddModelError(nameof(model.Code), "Bu cari kodu zaten kullanılıyor.");
        }
    }

    private static void Map(CustomerFormViewModel source, Customer target)
    {
        target.Code = source.Code.Trim();
        target.Name = source.Name.Trim();
        target.AccountType = source.AccountType;
        target.TaxOffice = source.TaxOffice?.Trim();
        target.TaxNumber = source.TaxNumber?.Trim();
        target.IdentityNumber = source.IdentityNumber?.Trim();
        target.CustomerGroup = source.CustomerGroup?.Trim();
        target.RiskLimit = source.RiskLimit;
        target.DefaultPaymentTermDays = source.DefaultPaymentTermDays;
        target.AuthorizedPerson = source.AuthorizedPerson?.Trim();
        target.Phone = source.Phone?.Trim();
        target.Email = source.Email?.Trim();
        target.Address = source.Address?.Trim();
        target.City = source.City?.Trim();
        target.District = source.District?.Trim();
        target.Notes = source.Notes?.Trim();
        target.IsCustomer = source.IsCustomer;
        target.IsSupplier = source.IsSupplier;
        target.IsActive = source.IsActive;
    }

    private async Task<bool> TrySaveAsync()
    {
        try
        {
            await dbContext.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(CustomerFormViewModel.Code), "Bu cari kodu başka bir kayıtta kullanılıyor.");
            return false;
        }
    }
}
