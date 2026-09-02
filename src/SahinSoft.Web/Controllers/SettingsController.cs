using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public sealed class SettingsController(ApplicationDbContext dbContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Company()
    {
        var settings = await dbContext.CompanySettings.AsNoTracking().SingleAsync(x => x.Id == 1);
        return View(new CompanySettingsViewModel
        {
            CompanyName = settings.CompanyName,
            TaxOffice = settings.TaxOffice,
            TaxNumber = settings.TaxNumber,
            Address = settings.Address,
            Phone = settings.Phone,
            Email = settings.Email,
            Website = settings.Website,
            BankName = settings.BankName,
            Iban = settings.Iban,
            LogoPath = settings.LogoPath
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Company(CompanySettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var settings = await dbContext.CompanySettings.SingleAsync(x => x.Id == 1);
        settings.CompanyName = model.CompanyName.Trim();
        settings.TaxOffice = model.TaxOffice?.Trim();
        settings.TaxNumber = model.TaxNumber?.Trim();
        settings.Address = model.Address?.Trim();
        settings.Phone = model.Phone?.Trim();
        settings.Email = model.Email?.Trim();
        settings.Website = model.Website?.Trim();
        settings.BankName = model.BankName?.Trim();
        settings.Iban = model.Iban?.Trim();
        settings.LogoPath = string.IsNullOrWhiteSpace(model.LogoPath) ? settings.LogoPath : model.LogoPath.Trim();
        settings.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Şirket parametreleri kaydedildi.";
        return RedirectToAction(nameof(Company));
    }

    [HttpGet]
    public async Task<IActionResult> Inventory()
    {
        var settings = await dbContext.InventorySettings.AsNoTracking().SingleAsync(x => x.Id == 1);
        return View(Map(settings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inventory(InventorySettingsViewModel model)
    {
        if (model.DefaultBarcodeType is not ("EAN13" or "EAN8"))
        {
            ModelState.AddModelError(nameof(model.DefaultBarcodeType), "Barkod tipi EAN13 veya EAN8 olmalıdır.");
        }
        if (model.DefaultScalePrefix is not ("27" or "28" or "29"))
        {
            ModelState.AddModelError(nameof(model.DefaultScalePrefix), "Terazi ön eki 27, 28 veya 29 olmalıdır.");
        }
        if (!model.TrackStockByVariant)
        {
            model.RequireProductVariant = false;
        }
        if (model.FiscalDeviceType == FiscalDeviceType.None)
        {
            model.FiscalAgentUrl = null;
        }
        else if (string.IsNullOrWhiteSpace(model.FiscalAgentUrl))
        {
            ModelState.AddModelError(nameof(model.FiscalAgentUrl), "Yazar kasa seçiliyken Fiscal Agent adresi zorunludur.");
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var settings = await dbContext.InventorySettings.SingleAsync(x => x.Id == 1);
        settings.RequireBarcode = model.RequireBarcode;
        settings.AutoGenerateBarcode = model.AutoGenerateBarcode;
        settings.DefaultBarcodeType = model.DefaultBarcodeType;
        settings.DefaultScalePrefix = model.DefaultScalePrefix;
        settings.EnforceStockLevel = model.EnforceStockLevel;
        settings.AllowNegativeStock = model.AllowNegativeStock;
        settings.AllowSaleWhenOutOfStock = model.AllowSaleWhenOutOfStock;
        settings.EnableMinimumStockWarning = model.EnableMinimumStockWarning;
        settings.RequireTransferApproval = model.RequireTransferApproval;
        settings.TrackStockByVariant = model.TrackStockByVariant;
        settings.RequireProductVariant = model.RequireProductVariant;
        settings.AllowSaleBelowCost = model.AllowSaleBelowCost;
        settings.IsRestaurantModuleEnabled = model.IsRestaurantModuleEnabled;
        settings.FiscalDeviceType = model.FiscalDeviceType;
        settings.FiscalAgentUrl = model.FiscalAgentUrl;
        settings.OrderToDispatchPurchaseAutoApprove = model.OrderToDispatchPurchaseAutoApprove;
        settings.OrderToDispatchSalesAutoApprove = model.OrderToDispatchSalesAutoApprove;
        settings.OrderToInvoicePurchaseAutoApprove = model.OrderToInvoicePurchaseAutoApprove;
        settings.OrderToInvoiceSalesAutoApprove = model.OrderToInvoiceSalesAutoApprove;
        settings.DispatchToInvoicePurchaseAutoApprove = model.DispatchToInvoicePurchaseAutoApprove;
        settings.DispatchToInvoiceSalesAutoApprove = model.DispatchToInvoiceSalesAutoApprove;
        settings.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Stok parametreleri kaydedildi ve işlem servislerine uygulandı.";
        return RedirectToAction(nameof(Inventory));
    }

    private static InventorySettingsViewModel Map(SahinSoft.Domain.Entities.InventorySettings settings) => new()
    {
        RequireBarcode = settings.RequireBarcode,
        AutoGenerateBarcode = settings.AutoGenerateBarcode,
        DefaultBarcodeType = settings.DefaultBarcodeType,
        DefaultScalePrefix = settings.DefaultScalePrefix,
        EnforceStockLevel = settings.EnforceStockLevel,
        AllowNegativeStock = settings.AllowNegativeStock,
        AllowSaleWhenOutOfStock = settings.AllowSaleWhenOutOfStock,
        EnableMinimumStockWarning = settings.EnableMinimumStockWarning,
        RequireTransferApproval = settings.RequireTransferApproval,
        TrackStockByVariant = settings.TrackStockByVariant,
        RequireProductVariant = settings.RequireProductVariant,
        AllowSaleBelowCost = settings.AllowSaleBelowCost,
        IsRestaurantModuleEnabled = settings.IsRestaurantModuleEnabled,
        FiscalDeviceType = settings.FiscalDeviceType,
        FiscalAgentUrl = settings.FiscalAgentUrl,
        OrderToDispatchPurchaseAutoApprove = settings.OrderToDispatchPurchaseAutoApprove,
        OrderToDispatchSalesAutoApprove = settings.OrderToDispatchSalesAutoApprove,
        OrderToInvoicePurchaseAutoApprove = settings.OrderToInvoicePurchaseAutoApprove,
        OrderToInvoiceSalesAutoApprove = settings.OrderToInvoiceSalesAutoApprove,
        DispatchToInvoicePurchaseAutoApprove = settings.DispatchToInvoicePurchaseAutoApprove,
        DispatchToInvoiceSalesAutoApprove = settings.DispatchToInvoiceSalesAutoApprove
    };
}
