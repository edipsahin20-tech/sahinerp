using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Constants;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models;
using SahinSoft.Web.Services;

namespace SahinSoft.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public sealed class SettingsController(ApplicationDbContext dbContext, SahinSoft.Web.Services.BranchSelectionService branchSelection) : Controller
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
            LogoPath = settings.LogoPath,
            DefaultBranchId = settings.DefaultBranchId,
            DefaultWarehouseId = settings.DefaultWarehouseId,
            BranchOptions = await branchSelection.OptionsAsync(settings.DefaultBranchId),
            WarehouseOptions = await branchSelection.WarehouseSelectItemsAsync(settings.DefaultWarehouseId)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Company(CompanySettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.BranchOptions = await branchSelection.OptionsAsync(model.DefaultBranchId);
            model.WarehouseOptions = await branchSelection.WarehouseSelectItemsAsync(model.DefaultWarehouseId);
            return View(model);
        }

        var settings = await dbContext.CompanySettings.SingleAsync(x => x.Id == 1);
        settings.DefaultBranchId = model.DefaultBranchId;
        settings.DefaultWarehouseId = model.DefaultWarehouseId;
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
        settings.DailyRevenueTarget = model.DailyRevenueTarget;
        settings.RequireOpenShiftForSales = model.RequireOpenShiftForSales;
        settings.IsKitchenTrackingEnabled = model.IsKitchenTrackingEnabled;
        settings.KitchenAutoReadyMinutes = model.KitchenAutoReadyMinutes;
        settings.RequireCancellationReason = model.RequireCancellationReason;
        settings.CancellationReasonPresets = model.CancellationReasonPresets;
        settings.QuickNotePresets = model.QuickNotePresets;
        settings.RequireSecondApprovalForCancelOrderLine = model.RequireSecondApprovalForCancelOrderLine;
        settings.RequireSecondApprovalForCancelReceipt = model.RequireSecondApprovalForCancelReceipt;
        settings.RequireSecondApprovalForDiscount = model.RequireSecondApprovalForDiscount;
        settings.RequireSecondApprovalForComplimentary = model.RequireSecondApprovalForComplimentary;
        settings.RequireSecondApprovalForEditKitchenSentLines = model.RequireSecondApprovalForEditKitchenSentLines;
        settings.RequireSecondApprovalForAddNote = model.RequireSecondApprovalForAddNote;
        settings.RequireReceiptPromptAfterQuickPay = model.RequireReceiptPromptAfterQuickPay;
        settings.ShowUnpaidPaymentType = model.ShowUnpaidPaymentType;
        settings.AutoZEnabled = model.AutoZEnabled;
        settings.AutoZTimeLocal = model.AutoZTimeLocal;
        settings.EnableTicketNoteButton = model.EnableTicketNoteButton;
        settings.EnableTableTransferButton = model.EnableTableTransferButton;
        settings.EnableSendToKitchenButton = model.EnableSendToKitchenButton;
        settings.EnablePriceCheckButton = model.EnablePriceCheckButton;
        settings.EnableKeyboardButton = model.EnableKeyboardButton;
        settings.EnableHoldReceiptButton = model.EnableHoldReceiptButton;
        settings.EnableHeldReceiptsButton = model.EnableHeldReceiptsButton;
        settings.EnableProductListButton = model.EnableProductListButton;
        settings.EnableComplimentaryReceiptButton = model.EnableComplimentaryReceiptButton;
        settings.EnableReceiptListButton = model.EnableReceiptListButton;
        settings.EnableClearOrderButton = model.EnableClearOrderButton;
        settings.AskGuestCountOnTableOpen = model.AskGuestCountOnTableOpen;
        settings.ShowTableSaleNav = model.ShowTableSaleNav;
        settings.ShowSelfSaleNav = model.ShowSelfSaleNav;
        settings.ShowPackageNav = model.ShowPackageNav;
        settings.FiscalDeviceType = model.FiscalDeviceType;
        settings.FiscalAgentUrl = model.FiscalAgentUrl;
        settings.OrderToDispatchPurchaseAutoApprove = model.OrderToDispatchPurchaseAutoApprove;
        settings.OrderToDispatchSalesAutoApprove = model.OrderToDispatchSalesAutoApprove;
        settings.OrderToInvoicePurchaseAutoApprove = model.OrderToInvoicePurchaseAutoApprove;
        settings.OrderToInvoiceSalesAutoApprove = model.OrderToInvoiceSalesAutoApprove;
        settings.DispatchToInvoicePurchaseAutoApprove = model.DispatchToInvoicePurchaseAutoApprove;
        settings.DispatchToInvoiceSalesAutoApprove = model.DispatchToInvoiceSalesAutoApprove;
        settings.PurchaseInvoiceAutoApproveOnSave = model.PurchaseInvoiceAutoApproveOnSave;
        settings.SalesInvoiceAutoApproveOnSave = model.SalesInvoiceAutoApproveOnSave;
        settings.StockMovementForServiceItems = model.StockMovementForServiceItems;
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
        DailyRevenueTarget = settings.DailyRevenueTarget,
        RequireOpenShiftForSales = settings.RequireOpenShiftForSales,
        IsKitchenTrackingEnabled = settings.IsKitchenTrackingEnabled,
        KitchenAutoReadyMinutes = settings.KitchenAutoReadyMinutes,
        // Bu üç alan daha önce burada HİÇ set edilmiyordu (gerçek bug, 2026-09-04 bulundu) -
        // /Settings/Inventory formu her açıldığında bunlar boş/kapalı görünüyordu, formu
        // farkında olmadan kaydeden bir admin DB'deki değeri sessizce sıfırlıyordu.
        RequireCancellationReason = settings.RequireCancellationReason,
        CancellationReasonPresets = settings.CancellationReasonPresets,
        QuickNotePresets = settings.QuickNotePresets,
        RequireSecondApprovalForCancelOrderLine = settings.RequireSecondApprovalForCancelOrderLine,
        RequireSecondApprovalForCancelReceipt = settings.RequireSecondApprovalForCancelReceipt,
        RequireSecondApprovalForDiscount = settings.RequireSecondApprovalForDiscount,
        RequireSecondApprovalForComplimentary = settings.RequireSecondApprovalForComplimentary,
        RequireSecondApprovalForEditKitchenSentLines = settings.RequireSecondApprovalForEditKitchenSentLines,
        RequireSecondApprovalForAddNote = settings.RequireSecondApprovalForAddNote,
        RequireReceiptPromptAfterQuickPay = settings.RequireReceiptPromptAfterQuickPay,
        ShowUnpaidPaymentType = settings.ShowUnpaidPaymentType,
        AutoZEnabled = settings.AutoZEnabled,
        AutoZTimeLocal = settings.AutoZTimeLocal,
        EnableTicketNoteButton = settings.EnableTicketNoteButton,
        EnableTableTransferButton = settings.EnableTableTransferButton,
        EnableSendToKitchenButton = settings.EnableSendToKitchenButton,
        EnablePriceCheckButton = settings.EnablePriceCheckButton,
        EnableKeyboardButton = settings.EnableKeyboardButton,
        EnableHoldReceiptButton = settings.EnableHoldReceiptButton,
        EnableHeldReceiptsButton = settings.EnableHeldReceiptsButton,
        EnableProductListButton = settings.EnableProductListButton,
        EnableComplimentaryReceiptButton = settings.EnableComplimentaryReceiptButton,
        EnableReceiptListButton = settings.EnableReceiptListButton,
        EnableClearOrderButton = settings.EnableClearOrderButton,
        AskGuestCountOnTableOpen = settings.AskGuestCountOnTableOpen,
        ShowTableSaleNav = settings.ShowTableSaleNav,
        ShowSelfSaleNav = settings.ShowSelfSaleNav,
        ShowPackageNav = settings.ShowPackageNav,
        FiscalDeviceType = settings.FiscalDeviceType,
        FiscalAgentUrl = settings.FiscalAgentUrl,
        OrderToDispatchPurchaseAutoApprove = settings.OrderToDispatchPurchaseAutoApprove,
        OrderToDispatchSalesAutoApprove = settings.OrderToDispatchSalesAutoApprove,
        OrderToInvoicePurchaseAutoApprove = settings.OrderToInvoicePurchaseAutoApprove,
        OrderToInvoiceSalesAutoApprove = settings.OrderToInvoiceSalesAutoApprove,
        DispatchToInvoicePurchaseAutoApprove = settings.DispatchToInvoicePurchaseAutoApprove,
        DispatchToInvoiceSalesAutoApprove = settings.DispatchToInvoiceSalesAutoApprove,
        PurchaseInvoiceAutoApproveOnSave = settings.PurchaseInvoiceAutoApproveOnSave,
        SalesInvoiceAutoApproveOnSave = settings.SalesInvoiceAutoApproveOnSave,
        StockMovementForServiceItems = settings.StockMovementForServiceItems
    };
}
