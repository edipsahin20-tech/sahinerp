using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SahinSoft.Domain.Common;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteLine> QuoteLines => Set<QuoteLine>();
    public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PurchasePriceList> PurchasePriceLists => Set<PurchasePriceList>();
    public DbSet<PurchasePriceListItem> PurchasePriceListItems => Set<PurchasePriceListItem>();
    public DbSet<CurrentAccountTransaction> CurrentAccountTransactions => Set<CurrentAccountTransaction>();
    public DbSet<FinancialAccount> FinancialAccounts => Set<FinancialAccount>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<BusinessProject> BusinessProjects => Set<BusinessProject>();
    public DbSet<PaymentReceipt> PaymentReceipts => Set<PaymentReceipt>();
    public DbSet<PaymentReceiptLine> PaymentReceiptLines => Set<PaymentReceiptLine>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<ProductColor> ProductColors => Set<ProductColor>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductBarcode> ProductBarcodes => Set<ProductBarcode>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferLine> StockTransferLines => Set<StockTransferLine>();
    public DbSet<ScaleProductSettings> ScaleProductSettings => Set<ScaleProductSettings>();
    public DbSet<InventorySettings> InventorySettings => Set<InventorySettings>();
    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<ProductUnitConversion> ProductUnitConversions => Set<ProductUnitConversion>();
    public DbSet<SalesPriceList> SalesPriceLists => Set<SalesPriceList>();
    public DbSet<SalesPriceListItem> SalesPriceListItems => Set<SalesPriceListItem>();
    public DbSet<ProductSerialNumber> ProductSerialNumbers => Set<ProductSerialNumber>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<InventoryCount> InventoryCounts => Set<InventoryCount>();
    public DbSet<InventoryCountLine> InventoryCountLines => Set<InventoryCountLine>();
    public DbSet<ExternalRecordMapping> ExternalRecordMappings => Set<ExternalRecordMapping>();
    public DbSet<IntegrationOutboxMessage> IntegrationOutboxMessages => Set<IntegrationOutboxMessage>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<BusinessOrder> BusinessOrders => Set<BusinessOrder>();
    public DbSet<BusinessOrderLine> BusinessOrderLines => Set<BusinessOrderLine>();
    public DbSet<DispatchNote> DispatchNotes => Set<DispatchNote>();
    public DbSet<DispatchNoteLine> DispatchNoteLines => Set<DispatchNoteLine>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<NegotiableInstrument> NegotiableInstruments => Set<NegotiableInstrument>();
    public DbSet<InvoicePaymentSchedule> InvoicePaymentSchedules => Set<InvoicePaymentSchedule>();
    public DbSet<StockSlip> StockSlips => Set<StockSlip>();
    public DbSet<StockSlipLine> StockSlipLines => Set<StockSlipLine>();

    // Restoran Modülü Faz 1 (bkz. CLEAN_ROOM_DEVELOPMENT.md)
    public DbSet<RestaurantSection> RestaurantSections => Set<RestaurantSection>();
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
    public DbSet<RestaurantTableSession> RestaurantTableSessions => Set<RestaurantTableSession>();
    public DbSet<RestaurantTableSessionMove> RestaurantTableSessionMoves => Set<RestaurantTableSessionMove>();
    public DbSet<RestaurantCheck> RestaurantChecks => Set<RestaurantCheck>();
    public DbSet<RestaurantOrder> RestaurantOrders => Set<RestaurantOrder>();
    public DbSet<RestaurantOrderLine> RestaurantOrderLines => Set<RestaurantOrderLine>();
    public DbSet<RestaurantOrderLineModifier> RestaurantOrderLineModifiers => Set<RestaurantOrderLineModifier>();
    public DbSet<ProductPortion> ProductPortions => Set<ProductPortion>();
    public DbSet<ProductRecipeHeader> ProductRecipeHeaders => Set<ProductRecipeHeader>();
    public DbSet<ProductRecipeLine> ProductRecipeLines => Set<ProductRecipeLine>();
    public DbSet<KitchenStation> KitchenStations => Set<KitchenStation>();
    public DbSet<KitchenTicket> KitchenTickets => Set<KitchenTicket>();
    public DbSet<KitchenTicketLine> KitchenTicketLines => Set<KitchenTicketLine>();
    public DbSet<RestaurantPayment> RestaurantPayments => Set<RestaurantPayment>();
    public DbSet<RestaurantCashShift> RestaurantCashShifts => Set<RestaurantCashShift>();
    public DbSet<RetailSale> RetailSales => Set<RetailSale>();
    public DbSet<RetailSaleLine> RetailSaleLines => Set<RetailSaleLine>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().Property(x => x.Id).HasMaxLength(128);
        builder.Entity<ApplicationUser>().Property(x => x.Salary).HasPrecision(18, 2);
        builder.Entity<ApplicationUser>().Property(x => x.Deduction).HasPrecision(18, 2);
        builder.Entity<ApplicationUser>().Property(x => x.CommissionRate).HasPrecision(5, 2);
        builder.Entity<ApplicationUser>()
            .HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<ApplicationUser>()
            .HasOne(x => x.DefaultFinancialAccount)
            .WithMany()
            .HasForeignKey(x => x.DefaultFinancialAccountId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<ApplicationUser>()
            .HasOne(x => x.DefaultPriceList)
            .WithMany()
            .HasForeignKey(x => x.DefaultPriceListId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<IdentityRole>().Property(x => x.Id).HasMaxLength(128);
        builder.Entity<IdentityUserRole<string>>().Property(x => x.UserId).HasMaxLength(128);
        builder.Entity<IdentityUserRole<string>>().Property(x => x.RoleId).HasMaxLength(128);
        builder.Entity<IdentityUserClaim<string>>().Property(x => x.UserId).HasMaxLength(128);
        builder.Entity<IdentityRoleClaim<string>>().Property(x => x.RoleId).HasMaxLength(128);
        builder.Entity<IdentityUserLogin<string>>().Property(x => x.UserId).HasMaxLength(128);
        builder.Entity<IdentityUserLogin<string>>().Property(x => x.LoginProvider).HasMaxLength(128);
        builder.Entity<IdentityUserLogin<string>>().Property(x => x.ProviderKey).HasMaxLength(128);
        builder.Entity<IdentityUserToken<string>>().Property(x => x.UserId).HasMaxLength(128);
        builder.Entity<IdentityUserToken<string>>().Property(x => x.LoginProvider).HasMaxLength(128);
        builder.Entity<IdentityUserToken<string>>().Property(x => x.Name).HasMaxLength(128);

        builder.Entity<ProductCategory>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.WebsitePath).HasMaxLength(250);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name);
        });

        builder.Entity<TaxRate>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Rate).HasPrecision(5, 2);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name);
        });

        builder.Entity<PriceList>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name);
        });

        builder.Entity<Product>(entity =>
        {
            entity.Property(x => x.StockCode).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Brand).HasMaxLength(100);
            entity.Property(x => x.Model).HasMaxLength(100);
            entity.Property(x => x.Barcode).HasMaxLength(50);
            entity.Property(x => x.Unit).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ProductType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.PurchasePrice).HasPrecision(18, 2);
            entity.Property(x => x.SalePrice).HasPrecision(18, 2);
            entity.Property(x => x.StockQuantity).HasPrecision(18, 3);
            entity.Property(x => x.MinimumStockQuantity).HasPrecision(18, 3);
            entity.Property(x => x.ImagePath).HasMaxLength(500);
            entity.Property(x => x.WebsitePath).HasMaxLength(250);
            entity.Property(x => x.AlternateName).HasMaxLength(200);
            entity.Property(x => x.CountryOfOrigin).HasMaxLength(100);
            entity.HasIndex(x => x.StockCode).IsUnique();
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.Barcode)
                .IsUnique()
                .HasFilter("[Barcode] IS NOT NULL");
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TaxRate)
                .WithMany()
                .HasForeignKey(x => x.TaxRateId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(x => x.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Products_Prices", "[PurchasePrice] >= 0 AND [SalePrice] >= 0");
                table.HasCheckConstraint("CK_Products_Quantities", "[MinimumStockQuantity] >= 0");
            });
        });

        builder.Entity<Customer>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.TaxOffice).HasMaxLength(100);
            entity.Property(x => x.TaxNumber).HasMaxLength(11);
            entity.Property(x => x.IdentityNumber).HasMaxLength(11);
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.District).HasMaxLength(100);
            entity.Property(x => x.CustomerGroup).HasMaxLength(100);
            entity.Property(x => x.AuthorizedPerson).HasMaxLength(150);
            entity.Property(x => x.RiskLimit).HasPrecision(18, 2);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.TaxNumber);
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => new { x.CreatedByUserId, x.SubmissionKey })
                .IsUnique()
                .HasFilter("[SubmissionKey] IS NOT NULL");
        });

        builder.Entity<Warehouse>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.BranchId, x.Name });
            entity.HasOne(x => x.Branch)
                .WithMany(x => x.Warehouses)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StockMovement>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitCost).HasPrecision(18, 4);
            entity.Property(x => x.DocumentNumber).HasMaxLength(50);
            entity.HasIndex(x => new { x.ProductId, x.WarehouseId, x.MovementDateUtc });
            entity.HasIndex(x => x.DocumentNumber);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.StockMovements)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Warehouse)
                .WithMany(x => x.StockMovements)
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InvoiceLine)
                .WithMany(x => x.StockMovements)
                .HasForeignKey(x => x.InvoiceLineId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.CostCenter)
                .WithMany()
                .HasForeignKey(x => x.CostCenterId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.BusinessProject)
                .WithMany()
                .HasForeignKey(x => x.BusinessProjectId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.StockTransferLine)
                .WithMany(x => x.StockMovements)
                .HasForeignKey(x => x.StockTransferLineId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.StockSlipLine)
                .WithMany()
                .HasForeignKey(x => x.StockSlipLineId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.InventoryCountLine)
                .WithMany()
                .HasForeignKey(x => x.InventoryCountLineId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.DispatchNoteLine)
                .WithMany()
                .HasForeignKey(x => x.DispatchNoteLineId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.ReversalOf)
                .WithMany()
                .HasForeignKey(x => x.ReversalOfId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Quote>(entity =>
        {
            entity.Property(x => x.QuoteNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.ExchangeRate).HasPrecision(18, 6);
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.DiscountTotal).HasPrecision(18, 2);
            entity.Property(x => x.AmountDiscount).HasPrecision(18, 2);
            entity.Property(x => x.TaxTotal).HasPrecision(18, 2);
            entity.Property(x => x.GrandTotal).HasPrecision(18, 2);
            entity.Property(x => x.CreatedByUserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(x => x.QuoteNumber).IsUnique();
            entity.HasIndex(x => new { x.CustomerId, x.QuoteDateUtc });
            entity.HasIndex(x => x.Status);
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Quotes)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_Quotes_Totals",
                    "[Subtotal] >= 0 AND [DiscountTotal] >= 0 AND [AmountDiscount] >= 0 AND [TaxTotal] >= 0 AND [GrandTotal] >= 0"));
        });

        builder.Entity<QuoteLine>(entity =>
        {
            entity.Property(x => x.ProductCodeSnapshot).HasMaxLength(40).IsRequired();
            entity.Property(x => x.ProductNameSnapshot).HasMaxLength(200).IsRequired();
            entity.Property(x => x.UnitSnapshot).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 4);
            entity.Property(x => x.DiscountRate).HasPrecision(5, 2);
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            entity.Property(x => x.TaxRate).HasPrecision(5, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.QuoteId, x.LineNumber }).IsUnique();
            entity.HasOne(x => x.Quote)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.QuoteLines)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<CompanySettings>(entity =>
        {
            entity.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.TaxOffice).HasMaxLength(100);
            entity.Property(x => x.TaxNumber).HasMaxLength(11);
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.Property(x => x.Website).HasMaxLength(250);
            entity.Property(x => x.BankName).HasMaxLength(150);
            entity.Property(x => x.Iban).HasMaxLength(34);
            entity.Property(x => x.LogoPath).HasMaxLength(500);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(100);
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.Property(x => x.UserAgent).HasMaxLength(500);
            entity.HasIndex(x => new { x.EntityName, x.EntityId });
            entity.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
        });

        builder.Entity<PurchasePriceList>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.CustomerId, x.ValidFromUtc, x.ValidUntilUtc });
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.PurchasePriceLists)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PurchasePriceListItem>(entity =>
        {
            entity.Property(x => x.MinimumQuantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 4);
            entity.Property(x => x.SupplierProductCode).HasMaxLength(80);
            entity.HasIndex(x => new
            {
                x.PurchasePriceListId,
                x.ProductId,
                x.MinimumQuantity
            }).IsUnique();
            entity.HasOne(x => x.PurchasePriceList)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.PurchasePriceListId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.PurchasePriceListItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CurrentAccountTransaction>(entity =>
        {
            entity.Property(x => x.DocumentNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.ExchangeRate).HasPrecision(18, 6);
            entity.Property(x => x.Debit).HasPrecision(18, 2);
            entity.Property(x => x.Credit).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.CustomerId, x.TransactionDateUtc });
            entity.HasIndex(x => x.DocumentNumber);
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.AccountTransactions)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Quote)
                .WithMany(x => x.AccountTransactions)
                .HasForeignKey(x => x.QuoteId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Invoice)
                .WithMany(x => x.AccountTransactions)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.NegotiableInstrument)
                .WithMany()
                .HasForeignKey(x => x.NegotiableInstrumentId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.ReversalOf)
                .WithMany()
                .HasForeignKey(x => x.ReversalOfId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_CurrentAccountTransactions_DebitCredit",
                    "([Debit] > 0 AND [Credit] = 0) OR ([Credit] > 0 AND [Debit] = 0)"));
        });

        builder.Entity<FinancialAccount>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.BankName).HasMaxLength(150);
            entity.Property(x => x.BranchName).HasMaxLength(150);
            entity.Property(x => x.Iban).HasMaxLength(34);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Iban);
            entity.HasIndex(x => x.Name);
        });

        builder.Entity<FinancialTransaction>(entity =>
        {
            entity.Property(x => x.DocumentNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.ExchangeRate).HasPrecision(18, 6);
            entity.HasIndex(x => new { x.FinancialAccountId, x.TransactionDateUtc });
            entity.HasIndex(x => x.DocumentNumber);
            entity.HasOne(x => x.FinancialAccount)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.FinancialAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.FinancialTransactions)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.CurrentAccountTransaction)
                .WithMany()
                .HasForeignKey(x => x.CurrentAccountTransactionId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.NegotiableInstrument)
                .WithMany()
                .HasForeignKey(x => x.NegotiableInstrumentId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.ReversalOf)
                .WithMany()
                .HasForeignKey(x => x.ReversalOfId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
                table.HasCheckConstraint("CK_FinancialTransactions_Amount", "[Amount] > 0"));
        });

        builder.Entity<Invoice>(entity =>
        {
            entity.Property(x => x.InvoiceNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.ExchangeRate).HasPrecision(18, 6);
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.DiscountTotal).HasPrecision(18, 2);
            entity.Property(x => x.AmountDiscount).HasPrecision(18, 2);
            entity.Property(x => x.TaxTotal).HasPrecision(18, 2);
            entity.Property(x => x.GrandTotal).HasPrecision(18, 2);
            entity.Property(x => x.CreatedByUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.ApprovedByUserId).HasMaxLength(450);
            entity.Property(x => x.CancelledByUserId).HasMaxLength(450);
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.Property(x => x.ReferenceNumber).HasMaxLength(50);
            entity.Property(x => x.PaymentTerm).HasMaxLength(100);
            entity.Property(x => x.TradeType).HasMaxLength(100);
            entity.Property(x => x.SalespersonUserId).HasMaxLength(450);
            entity.HasIndex(x => new { x.InvoiceType, x.InvoiceNumber }).IsUnique();
            // Çift tıklama/mükerrer POST koruması — bkz. Invoice.SubmissionKey.
            entity.HasIndex(x => x.SubmissionKey).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasIndex(x => new { x.CustomerId, x.InvoiceDateUtc });
            entity.HasIndex(x => new { x.InvoiceType, x.Status, x.InvoiceDateUtc });
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Quote)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.QuoteId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.PurchasePriceList)
                .WithMany(x => x.Invoices)
                .HasForeignKey(x => x.PurchasePriceListId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.CostCenter)
                .WithMany()
                .HasForeignKey(x => x.CostCenterId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.BusinessProject)
                .WithMany()
                .HasForeignKey(x => x.BusinessProjectId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.SettlementFinancialAccount)
                .WithMany()
                .HasForeignKey(x => x.SettlementFinancialAccountId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_Invoices_Totals",
                    "[Subtotal] >= 0 AND [DiscountTotal] >= 0 AND [TaxTotal] >= 0 AND [GrandTotal] >= 0"));
        });

        builder.Entity<InvoiceLine>(entity =>
        {
            entity.Property(x => x.ProductCodeSnapshot).HasMaxLength(40).IsRequired();
            entity.Property(x => x.ProductNameSnapshot).HasMaxLength(200).IsRequired();
            entity.Property(x => x.UnitSnapshot).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 4);
            entity.Property(x => x.DiscountRate).HasPrecision(5, 2);
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            entity.Property(x => x.TaxRate).HasPrecision(5, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.InvoiceId, x.LineNumber }).IsUnique();
            entity.HasOne(x => x.Invoice)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.InvoiceLines)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DispatchNoteLine)
                .WithMany(x => x.InvoiceLines)
                .HasForeignKey(x => x.DispatchNoteLineId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BusinessOrderLine)
                .WithMany(x => x.InvoiceLines)
                .HasForeignKey(x => x.BusinessOrderLineId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CustomerAddress>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(100).IsRequired();
            entity.Property(x => x.AddressType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.AddressLine).HasMaxLength(500).IsRequired();
            entity.Property(x => x.District).HasMaxLength(100);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.PostalCode).HasMaxLength(20);
            entity.Property(x => x.CountryCode).HasMaxLength(2).IsRequired();
            entity.HasIndex(x => new { x.CustomerId, x.AddressType });
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Addresses)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CustomerContact>(entity =>
        {
            entity.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(100);
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.HasIndex(x => x.CustomerId);
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Contacts)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CostCenter>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<BusinessProject>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<PaymentReceipt>(entity =>
        {
            entity.Property(x => x.ReceiptNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.DocumentNumber).HasMaxLength(50);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.ExchangeRate).HasPrecision(18, 6);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.CreatedByUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.ApprovedByUserId).HasMaxLength(450);
            entity.Property(x => x.CancelledByUserId).HasMaxLength(450);
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.HasIndex(x => new { x.ReceiptType, x.ReceiptNumber }).IsUnique();
            // Çift tıklama/mükerrer POST koruması — bkz. PaymentReceipt.SubmissionKey.
            entity.HasIndex(x => new { x.CreatedByUserId, x.SubmissionKey }).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasIndex(x => new { x.CustomerId, x.ReceiptDateUtc });
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.PaymentReceipts)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CostCenter)
                .WithMany()
                .HasForeignKey(x => x.CostCenterId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.BusinessProject)
                .WithMany()
                .HasForeignKey(x => x.BusinessProjectId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Invoice)
                .WithMany(x => x.PaymentReceipts)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<PaymentReceiptLine>(entity =>
        {
            entity.Property(x => x.ReferenceNumber).HasMaxLength(80);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.PaymentReceiptId, x.LineNumber }).IsUnique();
            entity.HasOne(x => x.PaymentReceipt)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.PaymentReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.FinancialAccount)
                .WithMany(x => x.PaymentReceiptLines)
                .HasForeignKey(x => x.FinancialAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CurrentAccountTransaction)
                .WithMany(x => x.PaymentReceiptLines)
                .HasForeignKey(x => x.CurrentAccountTransactionId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.FinancialTransaction)
                .WithMany(x => x.PaymentReceiptLines)
                .HasForeignKey(x => x.FinancialTransactionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Branch>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name);
        });

        builder.Entity<ProductColor>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.HexCode).HasMaxLength(7);
            entity.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<ProductVariant>(entity =>
        {
            entity.Property(x => x.VariantCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.VariantName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.AdditionalPurchasePrice).HasPrecision(18, 2);
            entity.Property(x => x.AdditionalSalePrice).HasPrecision(18, 2);
            entity.HasIndex(x => x.VariantCode).IsUnique();
            entity.HasIndex(x => new { x.ProductId, x.ColorId });
            entity.HasOne(x => x.Product)
                .WithMany(x => x.Variants)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Color)
                .WithMany(x => x.Variants)
                .HasForeignKey(x => x.ColorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ProductBarcode>(entity =>
        {
            entity.Property(x => x.Barcode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.BarcodeType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.UnitMultiplier).HasPrecision(18, 3);
            entity.HasIndex(x => x.Barcode).IsUnique();
            entity.HasIndex(x => new { x.ProductId, x.ProductVariantId });
            entity.HasOne(x => x.Product)
                .WithMany(x => x.Barcodes)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProductVariant)
                .WithMany(x => x.Barcodes)
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_ProductBarcodes_Length",
                    "([BarcodeType] = N'EAN13' AND LEN([Barcode]) = 13) OR " +
                    "([BarcodeType] = N'EAN8' AND LEN([Barcode]) = 8) OR " +
                    "([BarcodeType] = N'SCALE' AND LEN([Barcode]) = 7) OR " +
                    "[BarcodeType] = N'OTHER'");
                table.HasCheckConstraint(
                    "CK_ProductBarcodes_Numeric",
                    "[BarcodeType] = N'OTHER' OR [Barcode] NOT LIKE '%[^0-9]%'");
            });
        });

        builder.Entity<ScaleProductSettings>(entity =>
        {
            entity.Property(x => x.Prefix).HasMaxLength(2).IsRequired();
            entity.Property(x => x.PluCode).HasMaxLength(5).IsRequired();
            entity.HasIndex(x => x.ProductId).IsUnique();
            entity.HasIndex(x => new { x.Prefix, x.PluCode }).IsUnique();
            entity.HasOne(x => x.Product)
                .WithOne(x => x.ScaleSettings)
                .HasForeignKey<ScaleProductSettings>(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint(
                    "CK_ScaleProductSettings_Prefix",
                    "[Prefix] IN (N'27', N'28', N'29')");
                table.HasCheckConstraint(
                    "CK_ScaleProductSettings_PluLength",
                    "LEN([PluCode]) = 5 AND [PluCode] NOT LIKE '%[^0-9]%'");
            });
        });

        builder.Entity<InventorySettings>(entity =>
        {
            entity.Property(x => x.DefaultBarcodeType).HasMaxLength(20).IsRequired();
            entity.Property(x => x.DefaultScalePrefix).HasMaxLength(2).IsRequired();
            entity.Property(x => x.OrderToDispatchPurchaseAutoApprove).HasDefaultValue(false);
            entity.Property(x => x.OrderToDispatchSalesAutoApprove).HasDefaultValue(false);
            entity.Property(x => x.OrderToInvoicePurchaseAutoApprove).HasDefaultValue(false);
            entity.Property(x => x.OrderToInvoiceSalesAutoApprove).HasDefaultValue(false);
            entity.Property(x => x.DispatchToInvoicePurchaseAutoApprove).HasDefaultValue(false);
            entity.Property(x => x.DispatchToInvoiceSalesAutoApprove).HasDefaultValue(false);
        });

        builder.Entity<NumberSequence>(entity =>
        {
            // 60: varsayılan anahtarlar (ör. "SALES_INVOICE") + kullanıcının elle girdiği özel
            // seriler için "ANAHTAR:Seri" bileşik anahtarına yetecek kadar (bkz.
            // DocumentNumberGeneratorService.EnsureAtLeastForSeriesAsync).
            entity.Property(x => x.Key).HasMaxLength(60).IsRequired();
            entity.Property(x => x.Prefix).HasMaxLength(20).IsRequired();
            entity.HasIndex(x => x.Key).IsUnique();
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_NumberSequences_NextNumber", "[NextNumber] > 0");
                table.HasCheckConstraint("CK_NumberSequences_Padding", "[Padding] BETWEEN 1 AND 12");
            });
        });

        builder.Entity<UnitOfMeasure>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name);
            entity.ToTable(table =>
                table.HasCheckConstraint("CK_UnitsOfMeasure_DecimalPlaces", "[DecimalPlaces] BETWEEN 0 AND 6"));
        });

        builder.Entity<ProductUnitConversion>(entity =>
        {
            entity.Property(x => x.MultiplierToBaseUnit).HasPrecision(18, 6);
            entity.HasIndex(x => new { x.ProductId, x.UnitOfMeasureId }).IsUnique();
            entity.HasOne(x => x.Product)
                .WithMany(x => x.UnitConversions)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.UnitOfMeasure)
                .WithMany(x => x.ProductConversions)
                .HasForeignKey(x => x.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
                table.HasCheckConstraint("CK_ProductUnitConversions_Multiplier", "[MultiplierToBaseUnit] > 0"));
        });

        builder.Entity<SalesPriceList>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.CustomerId, x.ValidFromUtc, x.ValidUntilUtc });
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.SalesPriceLists)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<SalesPriceListItem>(entity =>
        {
            entity.Property(x => x.MinimumQuantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 4);
            // Kasıtlı olarak tekil (unique) değil: bir cari+ürün için zaman içinde birden fazla
            // fiyat kaydı (geçmiş) tutulur; en güncel fiyat CreatedAtUtc'ye göre seçilir.
            entity.HasIndex(x => new
            {
                x.SalesPriceListId,
                x.ProductId,
                x.ProductVariantId,
                x.MinimumQuantity
            });
            entity.HasOne(x => x.SalesPriceList)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.SalesPriceListId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.SalesPriceListItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_SalesPriceListItems_Quantity", "[MinimumQuantity] > 0");
                table.HasCheckConstraint("CK_SalesPriceListItems_Price", "[UnitPrice] >= 0");
            });
        });

        builder.Entity<ProductSerialNumber>(entity =>
        {
            entity.Property(x => x.SerialNumber).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LotNumber).HasMaxLength(100);
            entity.HasIndex(x => x.SerialNumber).IsUnique();
            entity.HasIndex(x => new { x.ProductId, x.LotNumber, x.ExpirationDateUtc });
            entity.HasIndex(x => new { x.WarehouseId, x.IsInStock });
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StockReservation>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.HasIndex(x => new { x.ProductId, x.ProductVariantId, x.WarehouseId, x.Status });
            entity.HasIndex(x => new { x.Status, x.ReservedUntilUtc });
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.QuoteLine)
                .WithMany()
                .HasForeignKey(x => x.QuoteLineId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.ToTable(table =>
                table.HasCheckConstraint("CK_StockReservations_Quantity", "[Quantity] > 0"));
        });

        builder.Entity<InventoryCount>(entity =>
        {
            entity.Property(x => x.CountNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CreatedByUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.ApprovedByUserId).HasMaxLength(450);
            entity.Property(x => x.CancelledByUserId).HasMaxLength(450);
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.HasIndex(x => x.CountNumber).IsUnique();
            // Çift tıklama/mükerrer POST koruması — bkz. InventoryCount.SubmissionKey.
            entity.HasIndex(x => new { x.CreatedByUserId, x.SubmissionKey }).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasIndex(x => new { x.WarehouseId, x.CountDateUtc });
            entity.HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<InventoryCountLine>(entity =>
        {
            entity.Property(x => x.SystemQuantity).HasPrecision(18, 3);
            entity.Property(x => x.CountedQuantity).HasPrecision(18, 3);
            entity.Ignore(x => x.DifferenceQuantity);
            entity.HasIndex(x => new { x.InventoryCountId, x.ProductId, x.ProductVariantId }).IsUnique();
            entity.HasOne(x => x.InventoryCount)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.InventoryCountId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.ToTable(table =>
                table.HasCheckConstraint("CK_InventoryCountLines_Quantities", "[SystemQuantity] >= 0 AND [CountedQuantity] >= 0"));
        });

        builder.Entity<StockSlip>(entity =>
        {
            entity.Property(x => x.SlipNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.CreatedByUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.ApprovedByUserId).HasMaxLength(450);
            entity.Property(x => x.CancelledByUserId).HasMaxLength(450);
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.HasIndex(x => x.SlipNumber).IsUnique();
            // Çift tıklama/mükerrer POST koruması — bkz. StockSlip.SubmissionKey.
            entity.HasIndex(x => new { x.CreatedByUserId, x.SubmissionKey }).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasIndex(x => new { x.WarehouseId, x.SlipDateUtc });
            entity.HasOne(x => x.Warehouse)
                .WithMany()
                .HasForeignKey(x => x.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CostCenter)
                .WithMany()
                .HasForeignKey(x => x.CostCenterId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.BusinessProject)
                .WithMany()
                .HasForeignKey(x => x.BusinessProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<StockSlipLine>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitCost).HasPrecision(18, 4);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.HasIndex(x => new { x.StockSlipId, x.LineNumber }).IsUnique();
            entity.HasOne(x => x.StockSlip)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.StockSlipId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.ToTable(table =>
                table.HasCheckConstraint("CK_StockSlipLines_Values", "[Quantity] > 0 AND [UnitCost] >= 0"));
        });

        builder.Entity<ExternalRecordMapping>(entity =>
        {
            entity.Property(x => x.SourceSystem).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ExternalId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.InternalId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ExternalCode).HasMaxLength(100);
            entity.Property(x => x.ContentHash).HasMaxLength(128);
            entity.HasIndex(x => new { x.SourceSystem, x.EntityType, x.ExternalId }).IsUnique();
            entity.HasIndex(x => new { x.EntityType, x.InternalId });
        });

        builder.Entity<IntegrationOutboxMessage>(entity =>
        {
            entity.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => new { x.ProcessedAtUtc, x.OccurredAtUtc });
        });

        builder.Entity<Currency>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(3).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Symbol).HasMaxLength(10).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<ExchangeRate>(entity =>
        {
            entity.Property(x => x.BuyingRate).HasPrecision(18, 6);
            entity.Property(x => x.SellingRate).HasPrecision(18, 6);
            entity.HasIndex(x => new { x.CurrencyId, x.RateDateUtc }).IsUnique();
            entity.HasOne(x => x.Currency)
                .WithMany(x => x.ExchangeRates)
                .HasForeignKey(x => x.CurrencyId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(table =>
                table.HasCheckConstraint("CK_ExchangeRates_Positive", "[BuyingRate] > 0 AND [SellingRate] > 0"));
        });

        builder.Entity<BusinessOrder>(entity =>
        {
            entity.Property(x => x.OrderNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.ExchangeRate).HasPrecision(18, 6);
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.DiscountTotal).HasPrecision(18, 2);
            entity.Property(x => x.TaxTotal).HasPrecision(18, 2);
            entity.Property(x => x.GrandTotal).HasPrecision(18, 2);
            entity.Property(x => x.CreatedByUserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(x => new { x.OrderType, x.OrderNumber }).IsUnique();
            // Çift tıklama/mükerrer POST koruması — bkz. BusinessOrder.SubmissionKey.
            entity.HasIndex(x => new { x.CreatedByUserId, x.SubmissionKey }).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasIndex(x => new { x.CustomerId, x.OrderDateUtc });
            entity.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Quote).WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<BusinessOrderLine>(entity =>
        {
            entity.Property(x => x.ProductCodeSnapshot).HasMaxLength(40).IsRequired();
            entity.Property(x => x.ProductNameSnapshot).HasMaxLength(200).IsRequired();
            entity.Property(x => x.UnitSnapshot).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.FulfilledQuantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 4);
            entity.Property(x => x.DiscountRate).HasPrecision(5, 2);
            entity.Property(x => x.TaxRate).HasPrecision(5, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.BusinessOrderId, x.LineNumber }).IsUnique();
            entity.HasOne(x => x.BusinessOrder).WithMany(x => x.Lines).HasForeignKey(x => x.BusinessOrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant).WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
                table.HasCheckConstraint("CK_BusinessOrderLines_Quantity", "[Quantity] > 0 AND [FulfilledQuantity] >= 0 AND [FulfilledQuantity] <= [Quantity]"));
        });

        builder.Entity<DispatchNote>(entity =>
        {
            entity.Property(x => x.DispatchNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.VehiclePlate).HasMaxLength(20);
            entity.Property(x => x.CarrierName).HasMaxLength(150);
            entity.Property(x => x.CreatedByUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.ApprovedByUserId).HasMaxLength(450);
            entity.Property(x => x.CancelledByUserId).HasMaxLength(450);
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.HasIndex(x => new { x.DispatchType, x.DispatchNumber }).IsUnique();
            // Çift tıklama/mükerrer POST koruması — bkz. DispatchNote.SubmissionKey.
            entity.HasIndex(x => new { x.CreatedByUserId, x.SubmissionKey }).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasIndex(x => new { x.CustomerId, x.DispatchDateUtc });
            entity.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BusinessOrder).WithMany().HasForeignKey(x => x.BusinessOrderId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Invoice).WithMany(x => x.DispatchNotes).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<DispatchNoteLine>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.InvoicedQuantity).HasPrecision(18, 3);
            entity.HasIndex(x => new { x.DispatchNoteId, x.LineNumber }).IsUnique();
            entity.HasOne(x => x.DispatchNote).WithMany(x => x.Lines).HasForeignKey(x => x.DispatchNoteId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant).WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BusinessOrderLine).WithMany(x => x.DispatchNoteLines).HasForeignKey(x => x.BusinessOrderLineId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_DispatchNoteLines_Quantity", "[Quantity] > 0");
                table.HasCheckConstraint("CK_DispatchNoteLines_InvoicedQuantity", "[InvoicedQuantity] >= 0 AND [InvoicedQuantity] <= [Quantity]");
            });
        });

        builder.Entity<ExpenseCategory>(entity =>
        {
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name);
        });

        builder.Entity<Expense>(entity =>
        {
            entity.Property(x => x.DocumentNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.ExchangeRate).HasPrecision(18, 6);
            entity.Property(x => x.NetAmount).HasPrecision(18, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.CreatedByUserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(x => new { x.ExpenseCategoryId, x.ExpenseDateUtc });
            // Diğer tüm evrak türlerinde (Invoice, PaymentReceipt, StockSlip, BusinessOrder,
            // DispatchNote, NegotiableInstrument, StockTransfer, InventoryCount) belge numarası
            // veritabanı seviyesinde unique — Masraf'ta eksikti, eşzamanlılık altında son
            // güvenlik ağı (uygulama seviyesindeki transaction koruması dışında) olması için eklendi.
            entity.HasIndex(x => x.DocumentNumber).IsUnique();
            // Çift tıklama/mükerrer POST koruması — bkz. Expense.SubmissionKey.
            entity.HasIndex(x => new { x.CreatedByUserId, x.SubmissionKey }).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasOne(x => x.ExpenseCategory).WithMany(x => x.Expenses).HasForeignKey(x => x.ExpenseCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.TaxRate).WithMany().HasForeignKey(x => x.TaxRateId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.FinancialAccount).WithMany().HasForeignKey(x => x.FinancialAccountId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.BusinessProject).WithMany().HasForeignKey(x => x.BusinessProjectId).OnDelete(DeleteBehavior.SetNull);
            entity.ToTable(table =>
                table.HasCheckConstraint("CK_Expenses_Amounts", "[NetAmount] >= 0 AND [TaxAmount] >= 0 AND [TotalAmount] = [NetAmount] + [TaxAmount]"));
        });

        builder.Entity<NegotiableInstrument>(entity =>
        {
            entity.Property(x => x.InstrumentNumber).HasMaxLength(80).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.BankName).HasMaxLength(150);
            entity.Property(x => x.BranchName).HasMaxLength(150);
            entity.Property(x => x.AccountNumber).HasMaxLength(80);
            entity.Property(x => x.DrawerName).HasMaxLength(200);
            entity.Property(x => x.CreatedByUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.CancelledByUserId).HasMaxLength(450);
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.HasIndex(x => new { x.InstrumentType, x.InstrumentNumber }).IsUnique();
            // Çift tıklama/mükerrer POST koruması — bkz. NegotiableInstrument.SubmissionKey.
            entity.HasIndex(x => new { x.CreatedByUserId, x.SubmissionKey }).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasIndex(x => new { x.Status, x.DueDateUtc });
            entity.HasIndex(x => new { x.CustomerId, x.DueDateUtc });
            entity.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.FinancialAccount).WithMany().HasForeignKey(x => x.FinancialAccountId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.SettlementFinancialAccount).WithMany().HasForeignKey(x => x.SettlementFinancialAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.EndorsedToCustomer).WithMany().HasForeignKey(x => x.EndorsedToCustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
                table.HasCheckConstraint("CK_NegotiableInstruments_Amount", "[Amount] > 0"));
        });

        builder.Entity<InvoicePaymentSchedule>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.InvoiceId, x.InstallmentNumber }).IsUnique();
            entity.HasIndex(x => x.DueDateUtc);
            entity.HasOne(x => x.Invoice).WithMany(x => x.PaymentSchedules).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(table =>
                table.HasCheckConstraint("CK_InvoicePaymentSchedules_Amounts", "[Amount] > 0 AND [PaidAmount] >= 0 AND [PaidAmount] <= [Amount]"));
        });

        builder.Entity<ProductImage>(entity =>
        {
            entity.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(x => x.AltText).HasMaxLength(200);
            entity.HasIndex(x => new { x.ProductId, x.DisplayOrder });
            entity.HasOne(x => x.Product)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProductVariant)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<StockTransfer>(entity =>
        {
            entity.Property(x => x.TransferNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CreatedByUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.ApprovedByUserId).HasMaxLength(450);
            entity.Property(x => x.CancelledByUserId).HasMaxLength(450);
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.HasIndex(x => x.TransferNumber).IsUnique();
            // Çift tıklama/mükerrer POST koruması — bkz. StockTransfer.SubmissionKey.
            entity.HasIndex(x => new { x.CreatedByUserId, x.SubmissionKey }).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasIndex(x => new { x.FromWarehouseId, x.TransferDateUtc });
            entity.HasIndex(x => new { x.ToWarehouseId, x.TransferDateUtc });
            entity.HasOne(x => x.FromWarehouse)
                .WithMany()
                .HasForeignKey(x => x.FromWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ToWarehouse)
                .WithMany()
                .HasForeignKey(x => x.ToWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_StockTransfers_DifferentWarehouses",
                    "[FromWarehouseId] <> [ToWarehouseId]"));
        });

        builder.Entity<StockTransferLine>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.HasIndex(x => new { x.StockTransferId, x.LineNumber }).IsUnique();
            entity.HasOne(x => x.StockTransfer)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.StockTransferId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.StockTransferLines)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.ToTable(table =>
                table.HasCheckConstraint(
                    "CK_StockTransferLines_PositiveQuantity",
                    "[Quantity] > 0"));
        });

        // ---- Restoran Modülü Faz 1 (bkz. CLEAN_ROOM_DEVELOPMENT.md) ----

        builder.Entity<Product>()
            .HasOne(x => x.DefaultKitchenStation)
            .WithMany()
            .HasForeignKey(x => x.DefaultKitchenStationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<RestaurantSection>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.BranchId, x.Name });
            entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RestaurantTable>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PosX).HasPrecision(10, 2);
            entity.Property(x => x.PosY).HasPrecision(10, 2);
            entity.HasIndex(x => new { x.RestaurantSectionId, x.Name });
            entity.HasOne(x => x.RestaurantSection)
                .WithMany(x => x.Tables)
                .HasForeignKey(x => x.RestaurantSectionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table => table.HasCheckConstraint("CK_RestaurantTables_Capacity", "[Capacity] >= 0"));
        });

        builder.Entity<RestaurantTableSession>(entity =>
        {
            entity.Property(x => x.OpenedByUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.WaiterUserId).HasMaxLength(450);
            entity.Property(x => x.ClosedByUserId).HasMaxLength(450);
            // Aynı masada iki aktif oturum olamaz — DB seviyesinde garanti.
            entity.HasIndex(x => x.RestaurantTableId)
                .IsUnique()
                .HasFilter("[Status] = 1")
                .HasDatabaseName("IX_RestaurantTableSessions_OneOpenPerTable");
            // Çift tıklama/mükerrer POST koruması — bkz. StockSlip.SubmissionKey.
            entity.HasIndex(x => x.SubmissionKey).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasOne(x => x.RestaurantTable)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.RestaurantTableId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MergedIntoSession)
                .WithMany()
                .HasForeignKey(x => x.MergedIntoSessionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RestaurantTableSessionMove>(entity =>
        {
            entity.Property(x => x.MovedByUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(300);
            entity.HasIndex(x => x.RestaurantTableSessionId);
            entity.HasOne(x => x.RestaurantTableSession)
                .WithMany(x => x.Moves)
                .HasForeignKey(x => x.RestaurantTableSessionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.FromRestaurantTable).WithMany().HasForeignKey(x => x.FromRestaurantTableId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ToRestaurantTable).WithMany().HasForeignKey(x => x.ToRestaurantTableId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RestaurantCheck>(entity =>
        {
            entity.Property(x => x.CheckNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.SubtotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            entity.Property(x => x.ServiceChargeAmount).HasPrecision(18, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.GrandTotal).HasPrecision(18, 2);
            entity.Property(x => x.CancelledByUserId).HasMaxLength(450);
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.HasIndex(x => x.CheckNumber).IsUnique();
            entity.HasIndex(x => new { x.RestaurantTableSessionId, x.OpenedAtUtc });
            // Çift tıklama/mükerrer POST koruması — bkz. StockSlip.SubmissionKey.
            entity.HasIndex(x => x.SubmissionKey).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasOne(x => x.RestaurantTableSession)
                .WithMany(x => x.Checks)
                .HasForeignKey(x => x.RestaurantTableSessionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LinkedInvoice).WithMany().HasForeignKey(x => x.LinkedInvoiceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.LinkedRetailSale).WithMany().HasForeignKey(x => x.LinkedRetailSaleId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_RestaurantChecks_Amounts",
                "[SubtotalAmount] >= 0 AND [DiscountAmount] >= 0 AND [ServiceChargeAmount] >= 0 AND [TaxAmount] >= 0 AND [GrandTotal] >= 0"));
        });

        builder.Entity<RestaurantOrder>(entity =>
        {
            entity.Property(x => x.OrderedByUserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(x => new { x.RestaurantCheckId, x.OrderedAtUtc });
            // Çift tıklama/mükerrer POST koruması — bkz. StockSlip.SubmissionKey.
            entity.HasIndex(x => x.SubmissionKey).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasOne(x => x.RestaurantCheck)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.RestaurantCheckId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RestaurantOrderLine>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.ProductNameSnapshot).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PortionNameSnapshot).HasMaxLength(50);
            entity.Property(x => x.UnitPriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.TaxRateSnapshot).HasPrecision(5, 2);
            entity.Property(x => x.DiscountAmountSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.KitchenNote).HasMaxLength(500);
            entity.Property(x => x.CancelledByUserId).HasMaxLength(450);
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.HasIndex(x => x.RestaurantOrderId);
            entity.HasIndex(x => x.Status);
            entity.HasOne(x => x.RestaurantOrder)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.RestaurantOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductPortion).WithMany().HasForeignKey(x => x.ProductPortionId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table => table.HasCheckConstraint("CK_RestaurantOrderLines_Quantity", "[Quantity] > 0"));
        });

        builder.Entity<RestaurantOrderLineModifier>(entity =>
        {
            entity.Property(x => x.NameSnapshot).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.HasIndex(x => x.RestaurantOrderLineId);
            entity.HasOne(x => x.RestaurantOrderLine)
                .WithMany(x => x.Modifiers)
                .HasForeignKey(x => x.RestaurantOrderLineId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(table => table.HasCheckConstraint("CK_RestaurantOrderLineModifiers_Quantity", "[Quantity] > 0"));
        });

        builder.Entity<ProductPortion>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PriceOverride).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.ProductId, x.Name });
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductRecipeHeader>(entity =>
        {
            entity.Property(x => x.YieldQuantity).HasPrecision(18, 3);
            // Aynı ürün/porsiyon/şube kombinasyonu için aynı anda yalnızca 1 aktif versiyon.
            entity.HasIndex(x => new { x.ProductId, x.ProductPortionId, x.BranchId })
                .IsUnique()
                .HasFilter("[ValidToUtc] IS NULL")
                .HasDatabaseName("IX_ProductRecipeHeaders_OneActiveVersion");
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductPortion).WithMany(x => x.RecipeHeaders).HasForeignKey(x => x.ProductPortionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table => table.HasCheckConstraint("CK_ProductRecipeHeaders_Yield", "[YieldQuantity] > 0"));
        });

        builder.Entity<ProductRecipeLine>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.WastagePercent).HasPrecision(5, 2);
            entity.HasIndex(x => x.ProductRecipeHeaderId);
            entity.HasOne(x => x.ProductRecipeHeader)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.ProductRecipeHeaderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.IngredientProduct).WithMany().HasForeignKey(x => x.IngredientProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.UnitOfMeasure).WithMany().HasForeignKey(x => x.UnitOfMeasureId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_ProductRecipeLines_Quantity", "[Quantity] > 0");
                table.HasCheckConstraint("CK_ProductRecipeLines_Wastage", "[WastagePercent] >= 0 AND [WastagePercent] <= 100");
            });
        });

        builder.Entity<KitchenStation>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PrinterName).HasMaxLength(150);
            entity.HasIndex(x => new { x.BranchId, x.Name });
            entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<KitchenTicket>(entity =>
        {
            entity.Property(x => x.TicketNumber).HasMaxLength(30);
            // Aynı sipariş+istasyon için mükerrer fiş oluşmaz.
            entity.HasIndex(x => new { x.RestaurantOrderId, x.KitchenStationId }).IsUnique();
            // Çift tıklama/mükerrer POST koruması — bkz. StockSlip.SubmissionKey.
            entity.HasIndex(x => x.SubmissionKey).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasOne(x => x.RestaurantOrder)
                .WithMany(x => x.KitchenTickets)
                .HasForeignKey(x => x.RestaurantOrderId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.KitchenStation).WithMany(x => x.Tickets).HasForeignKey(x => x.KitchenStationId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<KitchenTicketLine>(entity =>
        {
            // Aynı sipariş kalemi aynı fişte yalnızca 1 kez görünür — ama FARKLI fişlerde
            // (farklı istasyon veya tekrar gönderim) tekrar görünebilir, bkz. §11 Karar 4.
            entity.HasIndex(x => new { x.KitchenTicketId, x.RestaurantOrderLineId }).IsUnique();
            entity.HasIndex(x => x.RestaurantOrderLineId);
            entity.HasOne(x => x.KitchenTicket)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.KitchenTicketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.RestaurantOrderLine)
                .WithMany(x => x.KitchenTicketLines)
                .HasForeignKey(x => x.RestaurantOrderLineId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RestaurantPayment>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasIndex(x => x.RestaurantCheckId);
            // Çift tıklama/mükerrer POST koruması — bkz. StockSlip.SubmissionKey.
            entity.HasIndex(x => x.SubmissionKey).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasOne(x => x.RestaurantCheck)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.RestaurantCheckId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.FinancialAccount).WithMany().HasForeignKey(x => x.FinancialAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.FinancialTransaction).WithMany().HasForeignKey(x => x.FinancialTransactionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReversalOf).WithMany().HasForeignKey(x => x.ReversalOfId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table => table.HasCheckConstraint("CK_RestaurantPayments_Amount", "[Amount] > 0"));
        });

        builder.Entity<RestaurantCashShift>(entity =>
        {
            entity.Property(x => x.CashierUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.OpeningBalance).HasPrecision(18, 2);
            entity.Property(x => x.ClosingBalanceExpected).HasPrecision(18, 2);
            entity.Property(x => x.ClosingBalanceCounted).HasPrecision(18, 2);
            // Aynı kasada iki açık vardiya olamaz.
            entity.HasIndex(x => x.FinancialAccountId)
                .IsUnique()
                .HasFilter("[Status] = 1")
                .HasDatabaseName("IX_RestaurantCashShifts_OneOpenPerAccount");
            // Çift tıklama/mükerrer POST koruması — bkz. StockSlip.SubmissionKey.
            entity.HasIndex(x => x.SubmissionKey).IsUnique().HasFilter("[SubmissionKey] IS NOT NULL");
            entity.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.FinancialAccount).WithMany().HasForeignKey(x => x.FinancialAccountId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RetailSale>(entity =>
        {
            entity.Property(x => x.DocumentNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.SubtotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            entity.Property(x => x.ServiceChargeAmount).HasPrecision(18, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.GrandTotal).HasPrecision(18, 2);
            entity.Property(x => x.FiscalDeviceSerialNumber).HasMaxLength(50);
            entity.Property(x => x.FiscalReceiptNumber).HasMaxLength(50);
            entity.Property(x => x.ZReportNumber).HasMaxLength(50);
            entity.Property(x => x.FiscalTransactionId).HasMaxLength(100);
            entity.Property(x => x.EInvoiceUuid).HasMaxLength(100);
            entity.Property(x => x.CancelledByUserId).HasMaxLength(450);
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.HasIndex(x => x.DocumentNumber).IsUnique();
            // Bir adisyon başına en fazla 1 dahili perakende satış fişi.
            entity.HasIndex(x => x.RestaurantCheckId).IsUnique();
            entity.HasOne(x => x.RestaurantCheck).WithMany().HasForeignKey(x => x.RestaurantCheckId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_RetailSales_Amounts",
                "[SubtotalAmount] >= 0 AND [DiscountAmount] >= 0 AND [ServiceChargeAmount] >= 0 AND [TaxAmount] >= 0 AND [GrandTotal] >= 0"));
        });

        builder.Entity<RetailSaleLine>(entity =>
        {
            entity.Property(x => x.ProductNameSnapshot).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitPriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.TaxRateSnapshot).HasPrecision(5, 2);
            entity.Property(x => x.DiscountAmountSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.HasIndex(x => x.RetailSaleId);
            entity.HasOne(x => x.RetailSale)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.RetailSaleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table => table.HasCheckConstraint("CK_RetailSaleLines_Quantity", "[Quantity] > 0"));
        });

        builder.Entity<TaxRate>().HasData(CatalogSeedData.TaxRates);
        builder.Entity<ProductCategory>().HasData(CatalogSeedData.Categories);
        builder.Entity<Product>().HasData(CatalogSeedData.Products);
        builder.Entity<ProductBarcode>().HasData(CatalogSeedData.Barcodes);
        builder.Entity<InventorySettings>().HasData(new InventorySettings
        {
            Id = 1,
            RequireBarcode = true,
            AutoGenerateBarcode = true,
            DefaultBarcodeType = "EAN13",
            DefaultScalePrefix = "27",
            EnforceStockLevel = true,
            AllowNegativeStock = false,
            AllowSaleWhenOutOfStock = false,
            EnableMinimumStockWarning = true,
            RequireTransferApproval = true,
            TrackStockByVariant = false,
            RequireProductVariant = false,
            AllowSaleBelowCost = false,
            CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
        });
        builder.Entity<NumberSequence>().HasData(
            new NumberSequence
            {
                Id = 1,
                Key = "STOCK",
                Prefix = "SHN.",
                NextNumber = 1,
                Padding = 3,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 2,
                Key = "SALES_INVOICE",
                Prefix = "SF.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 3,
                Key = "PURCHASE_INVOICE",
                Prefix = "AF.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 4,
                Key = "COLLECTION_RECEIPT",
                Prefix = "TAH.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 5,
                Key = "PAYMENT_RECEIPT",
                Prefix = "TED.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 6,
                Key = "STOCK_RECEIPT",
                Prefix = "SGF.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 7,
                Key = "STOCK_ISSUE",
                Prefix = "SCF.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 8,
                Key = "STOCK_COUNT",
                Prefix = "SAY.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 9,
                Key = "SALES_DISPATCH",
                Prefix = "SIRS.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 10,
                Key = "PURCHASE_DISPATCH",
                Prefix = "AIRS.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 11,
                Key = "STOCK_TRANSFER",
                Prefix = "TRF.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 12,
                Key = "EXPENSE",
                Prefix = "MAS.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 13,
                Key = "NEGOTIABLE_CHEQUE",
                Prefix = "CEK.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 14,
                Key = "NEGOTIABLE_NOTE",
                Prefix = "SEN.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 15,
                Key = "SALES_ORDER",
                Prefix = "SSIP.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 16,
                Key = "PURCHASE_ORDER",
                Prefix = "ASIP.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 17,
                Key = "QUOTE",
                Prefix = "TEK.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            });
        builder.Entity<NumberSequence>().HasData(
            new NumberSequence
            {
                Id = 18,
                Key = "PERSONNEL",
                Prefix = "PRSNL.",
                NextNumber = 1,
                Padding = 3,
                CreatedAtUtc = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 19,
                Key = "CUSTOMER",
                Prefix = "CARI.",
                NextNumber = 1,
                Padding = 5,
                CreatedAtUtc = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 20,
                Key = "FINANCIAL_ACCOUNT_CASH",
                Prefix = "KASA.",
                NextNumber = 1,
                Padding = 3,
                CreatedAtUtc = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 21,
                Key = "FINANCIAL_ACCOUNT_BANK",
                Prefix = "BANKA.",
                NextNumber = 1,
                Padding = 3,
                CreatedAtUtc = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc)
            });
        // Otomatik barkod üretimi (bkz. BarcodeGeneratorService) artık "boş aday tara" yerine bu
        // sayaçlar üzerinden atomik rezervasyon yapıyor — Padding burada "1989" önekinden sonraki
        // gövde uzunluğu (EAN13: 12-4=8, EAN8: 7-4=3), kontrol basamağı hariç.
        // Id'ler bilinçli olarak yüksek tutuldu (1000+): DocumentNumberGeneratorService.
        // EnsureAtLeastForSeriesWithinTransactionAsync, kullanıcı elle seri girdiğinde (ör.
        // "SALES_INVOICE:ASE") IDENTITY sütunundan otomatik Id alan yeni NumberSequence satırları
        // çalışma zamanında oluşturuyor — küçük bir Id seçmek, canlıda organik olarak büyüyen bu
        // satırlarla ileride çakışma riski taşır (bu satırlardan bazıları bu migration hazırlanırken
        // tam olarak 22/23/24'ü zaten kullanıyordu).
        builder.Entity<NumberSequence>().HasData(
            new NumberSequence
            {
                Id = 1001,
                Key = "BARCODE_EAN13",
                Prefix = "1989",
                NextNumber = 1,
                Padding = 8,
                CreatedAtUtc = new DateTime(2026, 8, 2, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 1002,
                Key = "BARCODE_EAN8",
                Prefix = "1989",
                NextNumber = 1,
                Padding = 3,
                CreatedAtUtc = new DateTime(2026, 8, 2, 0, 0, 0, DateTimeKind.Utc)
            },
            new NumberSequence
            {
                Id = 1003,
                Key = "BARCODE_ASCII",
                Prefix = "AS",
                NextNumber = 1,
                Padding = 6,
                CreatedAtUtc = new DateTime(2026, 8, 2, 0, 0, 0, DateTimeKind.Utc)
            });
        builder.Entity<UnitOfMeasure>().HasData(
            new UnitOfMeasure
            {
                Id = 1,
                Code = "ADET",
                Name = "Adet",
                DecimalPlaces = 0,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new UnitOfMeasure
            {
                Id = 2,
                Code = "KG",
                Name = "Kilogram",
                DecimalPlaces = 3,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new UnitOfMeasure
            {
                Id = 3,
                Code = "PAKET",
                Name = "Paket",
                DecimalPlaces = 0,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            });
        builder.Entity<UnitOfMeasure>().HasData(
            new UnitOfMeasure
            {
                Id = 4,
                Code = "OZEL",
                Name = "Özel Fiyat",
                DecimalPlaces = 2,
                CreatedAtUtc = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc)
            },
            new UnitOfMeasure
            {
                Id = 5,
                Code = "METRE",
                Name = "Metre",
                DecimalPlaces = 2,
                CreatedAtUtc = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc)
            },
            new UnitOfMeasure
            {
                Id = 6,
                Code = "KOLI",
                Name = "Koli",
                DecimalPlaces = 0,
                CreatedAtUtc = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc)
            },
            new UnitOfMeasure
            {
                Id = 7,
                Code = "LITRE",
                Name = "Litre",
                DecimalPlaces = 2,
                CreatedAtUtc = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc)
            });
        builder.Entity<Currency>().HasData(
            new Currency
            {
                Id = 1,
                Code = "TRY",
                Name = "Türk Lirası",
                Symbol = "₺",
                IsBaseCurrency = true,
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new Currency
            {
                Id = 2,
                Code = "USD",
                Name = "Amerikan Doları",
                Symbol = "$",
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new Currency
            {
                Id = 3,
                Code = "EUR",
                Name = "Euro",
                Symbol = "€",
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            });

        foreach (var entityType in builder.Model.GetEntityTypes()
                     .Where(x => typeof(SahinSoft.Domain.Common.EntityBase).IsAssignableFrom(x.ClrType)))
        {
            builder.Entity(entityType.ClrType)
                .Property(nameof(SahinSoft.Domain.Common.EntityBase.RecordId))
                .HasDefaultValueSql("NEWSEQUENTIALID()")
                .ValueGeneratedOnAdd();
            builder.Entity(entityType.ClrType)
                .HasIndex(nameof(SahinSoft.Domain.Common.EntityBase.RecordId))
                .IsUnique();
        }
        builder.Entity<Branch>().HasData(new Branch
        {
            Id = 1,
            Code = "MERKEZ",
            Name = "Merkez Şube",
            IsHeadOffice = true,
            CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
        });
        builder.Entity<Warehouse>().HasData(new Warehouse
        {
            Id = 1,
            Code = "MERKEZ",
            Name = "Merkez Depo",
            BranchId = 1,
            IsDefault = true,
            CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
        });
        builder.Entity<PriceList>().HasData(
            new PriceList
            {
                Id = 1,
                Code = "MERKEZ",
                Name = "Merkez Fiyat",
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            },
            new PriceList
            {
                Id = 2,
                Code = "SUBE",
                Name = "Şube Fiyat",
                CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
            });
        builder.Entity<CompanySettings>().HasData(new CompanySettings
        {
            Id = 1,
            CompanyName = "ŞahinSoft",
            LogoPath = "/images/logo.png",
            CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
        });
        builder.Entity<FinancialAccount>().HasData(new FinancialAccount
        {
            Id = 1,
            Code = "KASA",
            Name = "Merkez Kasa",
            AccountType = FinancialAccountType.Cash,
            CurrencyCode = "TRY",
            CreatedAtUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var pendingAudits = CapturePendingAudits();
        var result = await base.SaveChangesAsync(cancellationToken);

        if (pendingAudits.Count > 0)
        {
            var userId = httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                ?? httpContextAccessor.HttpContext?.User?.Identity?.Name
                ?? string.Empty;
            var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

            foreach (var pending in pendingAudits)
            {
                AuditLogs.Add(new AuditLog
                {
                    UserId = userId,
                    Action = pending.Action,
                    EntityName = pending.EntityName,
                    EntityId = pending.Entry.Property(nameof(EntityBase.Id)).CurrentValue?.ToString(),
                    OldValuesJson = pending.OldValuesJson,
                    NewValuesJson = pending.NewValuesJson,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                });
            }

            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private List<PendingAudit> CapturePendingAudits()
    {
        var pending = new List<PendingAudit>();

        foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.Entity is EntityBase && e.Entity is not AuditLog
                                 && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            var oldValues = entry.State is EntityState.Modified or EntityState.Deleted
                ? SerializeValues(entry.OriginalValues)
                : null;
            var newValues = entry.State is EntityState.Added or EntityState.Modified
                ? SerializeValues(entry.CurrentValues)
                : null;

            pending.Add(new PendingAudit(entry, entry.State.ToString(), entry.Entity.GetType().Name, oldValues, newValues));
        }

        return pending;
    }

    private static string SerializeValues(PropertyValues values)
    {
        var dictionary = values.Properties.ToDictionary(p => p.Name, p => values[p]);
        return JsonSerializer.Serialize(dictionary);
    }

    private sealed record PendingAudit(EntityEntry Entry, string Action, string EntityName, string? OldValuesJson, string? NewValuesJson);
}
