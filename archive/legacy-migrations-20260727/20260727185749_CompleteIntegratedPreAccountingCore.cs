using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class CompleteIntegratedPreAccountingCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_Quantities",
                table: "Products");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "Warehouses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "TaxRates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "StockTransfers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "StockTransferLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<int>(
                name: "InventoryCountLineId",
                table: "StockMovements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "StockMovements",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<int>(
                name: "StockSlipLineId",
                table: "StockMovements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "ScaleProductSettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "Quotes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "QuoteLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "PurchasePriceLists",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "PurchasePriceListItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "ProductVariants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<bool>(
                name: "TrackLots",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrackSerialNumbers",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "ProductImages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "ProductColors",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "ProductCategories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "ProductBarcodes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "PaymentReceipts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "PaymentReceiptLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "NumberSequences",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "InvoiceLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "InventorySettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "FinancialTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "FinancialAccounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "CustomerContacts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "CustomerAddresses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "CurrentAccountTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "CostCenters",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "CompanySettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "BusinessProjects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "Branches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.CreateTable(
                name: "BusinessOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OrderDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestedDeliveryDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    QuoteId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BusinessOrders_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsBaseCurrency = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalRecordMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InternalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExternalCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastSynchronizedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalRecordMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationOutboxMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationOutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryCounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CountDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryCounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryCounts_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoicePaymentSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstallmentNumber = table.Column<int>(type: "int", nullable: false),
                    DueDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicePaymentSchedules", x => x.Id);
                    table.CheckConstraint("CK_InvoicePaymentSchedules_Amounts", "[Amount] > 0 AND [PaidAmount] >= 0 AND [PaidAmount] <= [Amount]");
                    table.ForeignKey(
                        name: "FK_InvoicePaymentSchedules_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NegotiableInstruments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentType = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InstrumentNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IssueDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    DrawerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    FinancialAccountId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NegotiableInstruments", x => x.Id);
                    table.CheckConstraint("CK_NegotiableInstruments_Amount", "[Amount] > 0");
                    table.ForeignKey(
                        name: "FK_NegotiableInstruments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NegotiableInstruments_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProductSerialNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LotNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpirationDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsInStock = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSerialNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSerialNumbers_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductSerialNumbers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductSerialNumbers_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesPriceLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ValidFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PricesIncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesPriceLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesPriceLists_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StockReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReservedUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    QuoteLineId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReservations", x => x.Id);
                    table.CheckConstraint("CK_StockReservations_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_StockReservations_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockReservations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockReservations_QuoteLines_QuoteLineId",
                        column: x => x.QuoteLineId,
                        principalTable: "QuoteLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockReservations_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockSlips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlipNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SlipDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SlipType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    BusinessProjectId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSlips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockSlips_BusinessProjects_BusinessProjectId",
                        column: x => x.BusinessProjectId,
                        principalTable: "BusinessProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockSlips_CostCenters_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockSlips_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitsOfMeasure",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DecimalPlaces = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitsOfMeasure", x => x.Id);
                    table.CheckConstraint("CK_UnitsOfMeasure_DecimalPlaces", "[DecimalPlaces] BETWEEN 0 AND 6");
                });

            migrationBuilder.CreateTable(
                name: "BusinessOrderLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    ProductCodeSnapshot = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitSnapshot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    FulfilledQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BusinessOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessOrderLines", x => x.Id);
                    table.CheckConstraint("CK_BusinessOrderLines_Quantity", "[Quantity] > 0 AND [FulfilledQuantity] >= 0 AND [FulfilledQuantity] <= [Quantity]");
                    table.ForeignKey(
                        name: "FK_BusinessOrderLines_BusinessOrders_BusinessOrderId",
                        column: x => x.BusinessOrderId,
                        principalTable: "BusinessOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessOrderLines_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BusinessOrderLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DispatchNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DispatchType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DispatchNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DispatchDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VehiclePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CarrierName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    BusinessOrderId = table.Column<int>(type: "int", nullable: true),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispatchNotes_BusinessOrders_BusinessOrderId",
                        column: x => x.BusinessOrderId,
                        principalTable: "BusinessOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DispatchNotes_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispatchNotes_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DispatchNotes_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RateDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BuyingRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    SellingRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                    table.CheckConstraint("CK_ExchangeRates_Positive", "[BuyingRate] > 0 AND [SellingRate] > 0");
                    table.ForeignKey(
                        name: "FK_ExchangeRates_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpenseDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpenseCategoryId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    TaxRateId = table.Column<int>(type: "int", nullable: true),
                    FinancialAccountId = table.Column<int>(type: "int", nullable: true),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    BusinessProjectId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.CheckConstraint("CK_Expenses_Amounts", "[NetAmount] >= 0 AND [TaxAmount] >= 0 AND [TotalAmount] = [NetAmount] + [TaxAmount]");
                    table.ForeignKey(
                        name: "FK_Expenses_BusinessProjects_BusinessProjectId",
                        column: x => x.BusinessProjectId,
                        principalTable: "BusinessProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Expenses_CostCenters_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Expenses_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Expenses_ExpenseCategories_ExpenseCategoryId",
                        column: x => x.ExpenseCategoryId,
                        principalTable: "ExpenseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Expenses_TaxRates_TaxRateId",
                        column: x => x.TaxRateId,
                        principalTable: "TaxRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InventoryCountLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CountedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    InventoryCountId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryCountLines", x => x.Id);
                    table.CheckConstraint("CK_InventoryCountLines_Quantities", "[SystemQuantity] >= 0 AND [CountedQuantity] >= 0");
                    table.ForeignKey(
                        name: "FK_InventoryCountLines_InventoryCounts_InventoryCountId",
                        column: x => x.InventoryCountId,
                        principalTable: "InventoryCounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryCountLines_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryCountLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesPriceListItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinimumQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SalesPriceListId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesPriceListItems", x => x.Id);
                    table.CheckConstraint("CK_SalesPriceListItems_Price", "[UnitPrice] >= 0");
                    table.CheckConstraint("CK_SalesPriceListItems_Quantity", "[MinimumQuantity] > 0");
                    table.ForeignKey(
                        name: "FK_SalesPriceListItems_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesPriceListItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesPriceListItems_SalesPriceLists_SalesPriceListId",
                        column: x => x.SalesPriceListId,
                        principalTable: "SalesPriceLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockSlipLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StockSlipId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockSlipLines", x => x.Id);
                    table.CheckConstraint("CK_StockSlipLines_Values", "[Quantity] > 0 AND [UnitCost] >= 0");
                    table.ForeignKey(
                        name: "FK_StockSlipLines_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockSlipLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockSlipLines_StockSlips_StockSlipId",
                        column: x => x.StockSlipId,
                        principalTable: "StockSlips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductUnitConversions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MultiplierToBaseUnit = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    IsPurchaseUnit = table.Column<bool>(type: "bit", nullable: false),
                    IsSalesUnit = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductUnitConversions", x => x.Id);
                    table.CheckConstraint("CK_ProductUnitConversions_Multiplier", "[MultiplierToBaseUnit] > 0");
                    table.ForeignKey(
                        name: "FK_ProductUnitConversions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductUnitConversions_UnitsOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DispatchNoteLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    DispatchNoteId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchNoteLines", x => x.Id);
                    table.CheckConstraint("CK_DispatchNoteLines_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_DispatchNoteLines_DispatchNotes_DispatchNoteId",
                        column: x => x.DispatchNoteId,
                        principalTable: "DispatchNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DispatchNoteLines_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DispatchNoteLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 1,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "CompanySettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "IsActive", "IsBaseCurrency", "Name", "Symbol", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, "TRY", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "Türk Lirası", "₺", null },
                    { 2, "USD", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "Amerikan Doları", "$", null },
                    { 3, "EUR", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "Euro", "€", null }
                });

            migrationBuilder.UpdateData(
                table: "InventorySettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 1,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 9,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 10,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 11,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 12,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 13,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 14,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 15,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 16,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 17,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 18,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 19,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 20,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 21,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 22,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 23,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 24,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 25,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 26,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 27,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 28,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 29,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 30,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 31,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 32,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 33,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 34,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 35,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 36,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 37,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 38,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 39,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 40,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 41,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 42,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 43,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 44,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 45,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 46,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 47,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: 4,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: 5,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: 6,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: 7,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: 8,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "TrackLots", "TrackSerialNumbers" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "TaxRates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "TaxRates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "TaxRates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                table: "TaxRates",
                keyColumn: "Id",
                keyValue: 4,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.InsertData(
                table: "UnitsOfMeasure",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "DecimalPlaces", "IsActive", "Name", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, "ADET", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), 0, true, "Adet", null },
                    { 2, "KG", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, "Kilogram", null },
                    { 3, "PAKET", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), 0, true, "Paket", null }
                });

            migrationBuilder.UpdateData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_RecordId",
                table: "Warehouses",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_RecordId",
                table: "TaxRates",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_RecordId",
                table: "StockTransfers",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferLines_RecordId",
                table: "StockTransferLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_InventoryCountLineId",
                table: "StockMovements",
                column: "InventoryCountLineId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_RecordId",
                table: "StockMovements",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_StockSlipLineId",
                table: "StockMovements",
                column: "StockSlipLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ScaleProductSettings_RecordId",
                table: "ScaleProductSettings",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_RecordId",
                table: "Quotes",
                column: "RecordId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Quotes_Totals",
                table: "Quotes",
                sql: "[Subtotal] >= 0 AND [DiscountTotal] >= 0 AND [TaxTotal] >= 0 AND [GrandTotal] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLines_RecordId",
                table: "QuoteLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePriceLists_RecordId",
                table: "PurchasePriceLists",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePriceListItems_RecordId",
                table: "PurchasePriceListItems",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_RecordId",
                table: "ProductVariants",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_RecordId",
                table: "Products",
                column: "RecordId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_Quantities",
                table: "Products",
                sql: "[MinimumStockQuantity] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_RecordId",
                table: "ProductImages",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductColors_RecordId",
                table: "ProductColors",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_RecordId",
                table: "ProductCategories",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_RecordId",
                table: "ProductBarcodes",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_RecordId",
                table: "PaymentReceipts",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceiptLines_RecordId",
                table: "PaymentReceiptLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NumberSequences_RecordId",
                table: "NumberSequences",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_RecordId",
                table: "Invoices",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_WarehouseId",
                table: "Invoices",
                column: "WarehouseId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Invoices_Totals",
                table: "Invoices",
                sql: "[Subtotal] >= 0 AND [DiscountTotal] >= 0 AND [TaxTotal] >= 0 AND [GrandTotal] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_RecordId",
                table: "InvoiceLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventorySettings_RecordId",
                table: "InventorySettings",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_RecordId",
                table: "FinancialTransactions",
                column: "RecordId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinancialTransactions_Amount",
                table: "FinancialTransactions",
                sql: "[Amount] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_RecordId",
                table: "FinancialAccounts",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_RecordId",
                table: "Customers",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContacts_RecordId",
                table: "CustomerContacts",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAddresses_RecordId",
                table: "CustomerAddresses",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrentAccountTransactions_RecordId",
                table: "CurrentAccountTransactions",
                column: "RecordId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CurrentAccountTransactions_DebitCredit",
                table: "CurrentAccountTransactions",
                sql: "([Debit] > 0 AND [Credit] = 0) OR ([Credit] > 0 AND [Debit] = 0)");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_RecordId",
                table: "CostCenters",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySettings_RecordId",
                table: "CompanySettings",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProjects_RecordId",
                table: "BusinessProjects",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_RecordId",
                table: "Branches",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_RecordId",
                table: "AuditLogs",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessOrderLines_BusinessOrderId_LineNumber",
                table: "BusinessOrderLines",
                columns: new[] { "BusinessOrderId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessOrderLines_ProductId",
                table: "BusinessOrderLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessOrderLines_ProductVariantId",
                table: "BusinessOrderLines",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessOrderLines_RecordId",
                table: "BusinessOrderLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessOrders_CustomerId_OrderDateUtc",
                table: "BusinessOrders",
                columns: new[] { "CustomerId", "OrderDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessOrders_OrderType_OrderNumber",
                table: "BusinessOrders",
                columns: new[] { "OrderType", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessOrders_QuoteId",
                table: "BusinessOrders",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessOrders_RecordId",
                table: "BusinessOrders",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_RecordId",
                table: "Currencies",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchNoteLines_DispatchNoteId_LineNumber",
                table: "DispatchNoteLines",
                columns: new[] { "DispatchNoteId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchNoteLines_ProductId",
                table: "DispatchNoteLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchNoteLines_ProductVariantId",
                table: "DispatchNoteLines",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchNoteLines_RecordId",
                table: "DispatchNoteLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchNotes_BusinessOrderId",
                table: "DispatchNotes",
                column: "BusinessOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchNotes_CustomerId_DispatchDateUtc",
                table: "DispatchNotes",
                columns: new[] { "CustomerId", "DispatchDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DispatchNotes_DispatchType_DispatchNumber",
                table: "DispatchNotes",
                columns: new[] { "DispatchType", "DispatchNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchNotes_InvoiceId",
                table: "DispatchNotes",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchNotes_RecordId",
                table: "DispatchNotes",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchNotes_WarehouseId",
                table: "DispatchNotes",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_CurrencyId_RateDateUtc",
                table: "ExchangeRates",
                columns: new[] { "CurrencyId", "RateDateUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_RecordId",
                table: "ExchangeRates",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_Code",
                table: "ExpenseCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_RecordId",
                table: "ExpenseCategories",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BusinessProjectId",
                table: "Expenses",
                column: "BusinessProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CostCenterId",
                table: "Expenses",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CustomerId",
                table: "Expenses",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_DocumentNumber",
                table: "Expenses",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ExpenseCategoryId_ExpenseDateUtc",
                table: "Expenses",
                columns: new[] { "ExpenseCategoryId", "ExpenseDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_FinancialAccountId",
                table: "Expenses",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_RecordId",
                table: "Expenses",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_TaxRateId",
                table: "Expenses",
                column: "TaxRateId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalRecordMappings_EntityType_InternalId",
                table: "ExternalRecordMappings",
                columns: new[] { "EntityType", "InternalId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalRecordMappings_RecordId",
                table: "ExternalRecordMappings",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalRecordMappings_SourceSystem_EntityType_ExternalId",
                table: "ExternalRecordMappings",
                columns: new[] { "SourceSystem", "EntityType", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationOutboxMessages_ProcessedAtUtc_OccurredAtUtc",
                table: "IntegrationOutboxMessages",
                columns: new[] { "ProcessedAtUtc", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationOutboxMessages_RecordId",
                table: "IntegrationOutboxMessages",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCountLines_InventoryCountId_ProductId_ProductVariantId",
                table: "InventoryCountLines",
                columns: new[] { "InventoryCountId", "ProductId", "ProductVariantId" },
                unique: true,
                filter: "[ProductVariantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCountLines_ProductId",
                table: "InventoryCountLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCountLines_ProductVariantId",
                table: "InventoryCountLines",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCountLines_RecordId",
                table: "InventoryCountLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_CountNumber",
                table: "InventoryCounts",
                column: "CountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_RecordId",
                table: "InventoryCounts",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCounts_WarehouseId_CountDateUtc",
                table: "InventoryCounts",
                columns: new[] { "WarehouseId", "CountDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePaymentSchedules_DueDateUtc",
                table: "InvoicePaymentSchedules",
                column: "DueDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePaymentSchedules_InvoiceId_InstallmentNumber",
                table: "InvoicePaymentSchedules",
                columns: new[] { "InvoiceId", "InstallmentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePaymentSchedules_RecordId",
                table: "InvoicePaymentSchedules",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NegotiableInstruments_CustomerId_DueDateUtc",
                table: "NegotiableInstruments",
                columns: new[] { "CustomerId", "DueDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NegotiableInstruments_FinancialAccountId",
                table: "NegotiableInstruments",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_NegotiableInstruments_InstrumentType_InstrumentNumber",
                table: "NegotiableInstruments",
                columns: new[] { "InstrumentType", "InstrumentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NegotiableInstruments_RecordId",
                table: "NegotiableInstruments",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NegotiableInstruments_Status_DueDateUtc",
                table: "NegotiableInstruments",
                columns: new[] { "Status", "DueDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductSerialNumbers_ProductId_LotNumber_ExpirationDateUtc",
                table: "ProductSerialNumbers",
                columns: new[] { "ProductId", "LotNumber", "ExpirationDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductSerialNumbers_ProductVariantId",
                table: "ProductSerialNumbers",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSerialNumbers_RecordId",
                table: "ProductSerialNumbers",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSerialNumbers_SerialNumber",
                table: "ProductSerialNumbers",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSerialNumbers_WarehouseId_IsInStock",
                table: "ProductSerialNumbers",
                columns: new[] { "WarehouseId", "IsInStock" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnitConversions_ProductId_UnitOfMeasureId",
                table: "ProductUnitConversions",
                columns: new[] { "ProductId", "UnitOfMeasureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnitConversions_RecordId",
                table: "ProductUnitConversions",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnitConversions_UnitOfMeasureId",
                table: "ProductUnitConversions",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPriceListItems_ProductId",
                table: "SalesPriceListItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPriceListItems_ProductVariantId",
                table: "SalesPriceListItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPriceListItems_RecordId",
                table: "SalesPriceListItems",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesPriceListItems_SalesPriceListId_ProductId_ProductVariantId_MinimumQuantity",
                table: "SalesPriceListItems",
                columns: new[] { "SalesPriceListId", "ProductId", "ProductVariantId", "MinimumQuantity" },
                unique: true,
                filter: "[ProductVariantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPriceLists_Code",
                table: "SalesPriceLists",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesPriceLists_CustomerId_ValidFromUtc_ValidUntilUtc",
                table: "SalesPriceLists",
                columns: new[] { "CustomerId", "ValidFromUtc", "ValidUntilUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesPriceLists_RecordId",
                table: "SalesPriceLists",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_ProductId_ProductVariantId_WarehouseId_Status",
                table: "StockReservations",
                columns: new[] { "ProductId", "ProductVariantId", "WarehouseId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_ProductVariantId",
                table: "StockReservations",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_QuoteLineId",
                table: "StockReservations",
                column: "QuoteLineId");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_RecordId",
                table: "StockReservations",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_Status_ReservedUntilUtc",
                table: "StockReservations",
                columns: new[] { "Status", "ReservedUntilUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_WarehouseId",
                table: "StockReservations",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockSlipLines_ProductId",
                table: "StockSlipLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockSlipLines_ProductVariantId",
                table: "StockSlipLines",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_StockSlipLines_RecordId",
                table: "StockSlipLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockSlipLines_StockSlipId_LineNumber",
                table: "StockSlipLines",
                columns: new[] { "StockSlipId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockSlips_BusinessProjectId",
                table: "StockSlips",
                column: "BusinessProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StockSlips_CostCenterId",
                table: "StockSlips",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_StockSlips_RecordId",
                table: "StockSlips",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockSlips_SlipNumber",
                table: "StockSlips",
                column: "SlipNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockSlips_WarehouseId_SlipDateUtc",
                table: "StockSlips",
                columns: new[] { "WarehouseId", "SlipDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UnitsOfMeasure_Code",
                table: "UnitsOfMeasure",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitsOfMeasure_RecordId",
                table: "UnitsOfMeasure",
                column: "RecordId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Warehouses_WarehouseId",
                table: "Invoices",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_InventoryCountLines_InventoryCountLineId",
                table: "StockMovements",
                column: "InventoryCountLineId",
                principalTable: "InventoryCountLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_StockSlipLines_StockSlipLineId",
                table: "StockMovements",
                column: "StockSlipLineId",
                principalTable: "StockSlipLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Warehouses_WarehouseId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_InventoryCountLines_InventoryCountLineId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_StockSlipLines_StockSlipLineId",
                table: "StockMovements");

            migrationBuilder.DropTable(
                name: "BusinessOrderLines");

            migrationBuilder.DropTable(
                name: "DispatchNoteLines");

            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "ExternalRecordMappings");

            migrationBuilder.DropTable(
                name: "IntegrationOutboxMessages");

            migrationBuilder.DropTable(
                name: "InventoryCountLines");

            migrationBuilder.DropTable(
                name: "InvoicePaymentSchedules");

            migrationBuilder.DropTable(
                name: "NegotiableInstruments");

            migrationBuilder.DropTable(
                name: "ProductSerialNumbers");

            migrationBuilder.DropTable(
                name: "ProductUnitConversions");

            migrationBuilder.DropTable(
                name: "SalesPriceListItems");

            migrationBuilder.DropTable(
                name: "StockReservations");

            migrationBuilder.DropTable(
                name: "StockSlipLines");

            migrationBuilder.DropTable(
                name: "DispatchNotes");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");

            migrationBuilder.DropTable(
                name: "InventoryCounts");

            migrationBuilder.DropTable(
                name: "UnitsOfMeasure");

            migrationBuilder.DropTable(
                name: "SalesPriceLists");

            migrationBuilder.DropTable(
                name: "StockSlips");

            migrationBuilder.DropTable(
                name: "BusinessOrders");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_RecordId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_TaxRates_RecordId",
                table: "TaxRates");

            migrationBuilder.DropIndex(
                name: "IX_StockTransfers_RecordId",
                table: "StockTransfers");

            migrationBuilder.DropIndex(
                name: "IX_StockTransferLines_RecordId",
                table: "StockTransferLines");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_InventoryCountLineId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_RecordId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_StockSlipLineId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_ScaleProductSettings_RecordId",
                table: "ScaleProductSettings");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_RecordId",
                table: "Quotes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Quotes_Totals",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_QuoteLines_RecordId",
                table: "QuoteLines");

            migrationBuilder.DropIndex(
                name: "IX_PurchasePriceLists_RecordId",
                table: "PurchasePriceLists");

            migrationBuilder.DropIndex(
                name: "IX_PurchasePriceListItems_RecordId",
                table: "PurchasePriceListItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_RecordId",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_Products_RecordId",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_Quantities",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_RecordId",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_ProductColors_RecordId",
                table: "ProductColors");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_RecordId",
                table: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_ProductBarcodes_RecordId",
                table: "ProductBarcodes");

            migrationBuilder.DropIndex(
                name: "IX_PaymentReceipts_RecordId",
                table: "PaymentReceipts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentReceiptLines_RecordId",
                table: "PaymentReceiptLines");

            migrationBuilder.DropIndex(
                name: "IX_NumberSequences_RecordId",
                table: "NumberSequences");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_RecordId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_WarehouseId",
                table: "Invoices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Invoices_Totals",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLines_RecordId",
                table: "InvoiceLines");

            migrationBuilder.DropIndex(
                name: "IX_InventorySettings_RecordId",
                table: "InventorySettings");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_RecordId",
                table: "FinancialTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinancialTransactions_Amount",
                table: "FinancialTransactions");

            migrationBuilder.DropIndex(
                name: "IX_FinancialAccounts_RecordId",
                table: "FinancialAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Customers_RecordId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CustomerContacts_RecordId",
                table: "CustomerContacts");

            migrationBuilder.DropIndex(
                name: "IX_CustomerAddresses_RecordId",
                table: "CustomerAddresses");

            migrationBuilder.DropIndex(
                name: "IX_CurrentAccountTransactions_RecordId",
                table: "CurrentAccountTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CurrentAccountTransactions_DebitCredit",
                table: "CurrentAccountTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CostCenters_RecordId",
                table: "CostCenters");

            migrationBuilder.DropIndex(
                name: "IX_CompanySettings_RecordId",
                table: "CompanySettings");

            migrationBuilder.DropIndex(
                name: "IX_BusinessProjects_RecordId",
                table: "BusinessProjects");

            migrationBuilder.DropIndex(
                name: "IX_Branches_RecordId",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_RecordId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "TaxRates");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "StockTransfers");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "StockTransferLines");

            migrationBuilder.DropColumn(
                name: "InventoryCountLineId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "StockSlipLineId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "ScaleProductSettings");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "QuoteLines");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "PurchasePriceLists");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "PurchasePriceListItems");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TrackLots",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TrackSerialNumbers",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "ProductColors");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "ProductBarcodes");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "PaymentReceiptLines");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "NumberSequences");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "CustomerContacts");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "CustomerAddresses");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "CurrentAccountTransactions");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "BusinessProjects");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "AuditLogs");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_Quantities",
                table: "Products",
                sql: "[StockQuantity] >= 0 AND [MinimumStockQuantity] >= 0");
        }
    }
}
