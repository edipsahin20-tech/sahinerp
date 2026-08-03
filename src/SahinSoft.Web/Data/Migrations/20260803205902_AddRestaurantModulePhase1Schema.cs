using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantModulePhase1Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultKitchenStationId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KitchenStations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrinterName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitchenStations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KitchenStations_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductPortions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PriceOverride = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPortions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPortions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantCashShifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CashierUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OpenedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosingBalanceExpected = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ClosingBalanceCounted = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SubmissionKey = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    FinancialAccountId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantCashShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantCashShifts_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantCashShifts_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantSections_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductRecipeHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ValidFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    YieldQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductPortionId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    ProductPortionId1 = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRecipeHeaders", x => x.Id);
                    table.CheckConstraint("CK_ProductRecipeHeaders_Yield", "[YieldQuantity] > 0");
                    table.ForeignKey(
                        name: "FK_ProductRecipeHeaders_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductRecipeHeaders_ProductPortions_ProductPortionId",
                        column: x => x.ProductPortionId,
                        principalTable: "ProductPortions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductRecipeHeaders_ProductPortions_ProductPortionId1",
                        column: x => x.ProductPortionId1,
                        principalTable: "ProductPortions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductRecipeHeaders_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductRecipeHeaders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    PosX = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    PosY = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RestaurantSectionId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantTables", x => x.Id);
                    table.CheckConstraint("CK_RestaurantTables_Capacity", "[Capacity] >= 0");
                    table.ForeignKey(
                        name: "FK_RestaurantTables_RestaurantSections_RestaurantSectionId",
                        column: x => x.RestaurantSectionId,
                        principalTable: "RestaurantSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductRecipeLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    WastagePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ProductRecipeHeaderId = table.Column<int>(type: "int", nullable: false),
                    IngredientProductId = table.Column<int>(type: "int", nullable: false),
                    UnitOfMeasureId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRecipeLines", x => x.Id);
                    table.CheckConstraint("CK_ProductRecipeLines_Quantity", "[Quantity] > 0");
                    table.CheckConstraint("CK_ProductRecipeLines_Wastage", "[WastagePercent] >= 0 AND [WastagePercent] <= 100");
                    table.ForeignKey(
                        name: "FK_ProductRecipeLines_ProductRecipeHeaders_ProductRecipeHeaderId",
                        column: x => x.ProductRecipeHeaderId,
                        principalTable: "ProductRecipeHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductRecipeLines_Products_IngredientProductId",
                        column: x => x.IngredientProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductRecipeLines_UnitsOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantTableSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OpenedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OpenedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    GuestCount = table.Column<int>(type: "int", nullable: false),
                    WaiterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SubmissionKey = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RestaurantTableId = table.Column<int>(type: "int", nullable: false),
                    MergedIntoSessionId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantTableSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantTableSessions_RestaurantTableSessions_MergedIntoSessionId",
                        column: x => x.MergedIntoSessionId,
                        principalTable: "RestaurantTableSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantTableSessions_RestaurantTables_RestaurantTableId",
                        column: x => x.RestaurantTableId,
                        principalTable: "RestaurantTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantTableSessionMoves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RestaurantTableSessionId = table.Column<int>(type: "int", nullable: false),
                    FromRestaurantTableId = table.Column<int>(type: "int", nullable: false),
                    ToRestaurantTableId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantTableSessionMoves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantTableSessionMoves_RestaurantTableSessions_RestaurantTableSessionId",
                        column: x => x.RestaurantTableSessionId,
                        principalTable: "RestaurantTableSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantTableSessionMoves_RestaurantTables_FromRestaurantTableId",
                        column: x => x.FromRestaurantTableId,
                        principalTable: "RestaurantTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantTableSessionMoves_RestaurantTables_ToRestaurantTableId",
                        column: x => x.ToRestaurantTableId,
                        principalTable: "RestaurantTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KitchenTicketLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    KitchenTicketId = table.Column<int>(type: "int", nullable: false),
                    RestaurantOrderLineId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitchenTicketLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KitchenTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmissionKey = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RestaurantOrderId = table.Column<int>(type: "int", nullable: false),
                    KitchenStationId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitchenTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KitchenTickets_KitchenStations_KitchenStationId",
                        column: x => x.KitchenStationId,
                        principalTable: "KitchenStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CheckNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OpenedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubtotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ServiceChargeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CancelledByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubmissionKey = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RestaurantTableSessionId = table.Column<int>(type: "int", nullable: false),
                    LinkedInvoiceId = table.Column<int>(type: "int", nullable: true),
                    LinkedRetailSaleId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantChecks", x => x.Id);
                    table.CheckConstraint("CK_RestaurantChecks_Amounts", "[SubtotalAmount] >= 0 AND [DiscountAmount] >= 0 AND [ServiceChargeAmount] >= 0 AND [TaxAmount] >= 0 AND [GrandTotal] >= 0");
                    table.ForeignKey(
                        name: "FK_RestaurantChecks_Invoices_LinkedInvoiceId",
                        column: x => x.LinkedInvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantChecks_RestaurantTableSessions_RestaurantTableSessionId",
                        column: x => x.RestaurantTableSessionId,
                        principalTable: "RestaurantTableSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SubmissionKey = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RestaurantCheckId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantOrders_RestaurantChecks_RestaurantCheckId",
                        column: x => x.RestaurantCheckId,
                        principalTable: "RestaurantChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsReversal = table.Column<bool>(type: "bit", nullable: false),
                    ReversalOfId = table.Column<int>(type: "int", nullable: true),
                    SubmissionKey = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RestaurantCheckId = table.Column<int>(type: "int", nullable: false),
                    FinancialAccountId = table.Column<int>(type: "int", nullable: false),
                    FinancialTransactionId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantPayments", x => x.Id);
                    table.CheckConstraint("CK_RestaurantPayments_Amount", "[Amount] > 0");
                    table.ForeignKey(
                        name: "FK_RestaurantPayments_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantPayments_FinancialTransactions_FinancialTransactionId",
                        column: x => x.FinancialTransactionId,
                        principalTable: "FinancialTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantPayments_RestaurantChecks_RestaurantCheckId",
                        column: x => x.RestaurantCheckId,
                        principalTable: "RestaurantChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantPayments_RestaurantPayments_ReversalOfId",
                        column: x => x.ReversalOfId,
                        principalTable: "RestaurantPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RetailSales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubtotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ServiceChargeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FiscalDeviceSerialNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FiscalReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ZReportNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FiscalizationStatus = table.Column<int>(type: "int", nullable: false),
                    FiscalTransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EInvoiceUuid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CancelledByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RestaurantCheckId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetailSales", x => x.Id);
                    table.CheckConstraint("CK_RetailSales_Amounts", "[SubtotalAmount] >= 0 AND [DiscountAmount] >= 0 AND [ServiceChargeAmount] >= 0 AND [TaxAmount] >= 0 AND [GrandTotal] >= 0");
                    table.ForeignKey(
                        name: "FK_RetailSales_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetailSales_RestaurantChecks_RestaurantCheckId",
                        column: x => x.RestaurantCheckId,
                        principalTable: "RestaurantChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantOrderLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PortionNameSnapshot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UnitPriceSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxRateSnapshot = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DiscountAmountSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RecipeVersionUsed = table.Column<int>(type: "int", nullable: true),
                    KitchenNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CancelledByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RestaurantOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductPortionId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantOrderLines", x => x.Id);
                    table.CheckConstraint("CK_RestaurantOrderLines_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_RestaurantOrderLines_ProductPortions_ProductPortionId",
                        column: x => x.ProductPortionId,
                        principalTable: "ProductPortions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantOrderLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantOrderLines_RestaurantOrders_RestaurantOrderId",
                        column: x => x.RestaurantOrderId,
                        principalTable: "RestaurantOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RetailSaleLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPriceSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxRateSnapshot = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DiscountAmountSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RetailSaleId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetailSaleLines", x => x.Id);
                    table.CheckConstraint("CK_RetailSaleLines_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_RetailSaleLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RetailSaleLines_RetailSales_RetailSaleId",
                        column: x => x.RetailSaleId,
                        principalTable: "RetailSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantOrderLineModifiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameSnapshot = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PriceSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RestaurantOrderLineId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantOrderLineModifiers", x => x.Id);
                    table.CheckConstraint("CK_RestaurantOrderLineModifiers_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_RestaurantOrderLineModifiers_RestaurantOrderLines_RestaurantOrderLineId",
                        column: x => x.RestaurantOrderLineId,
                        principalTable: "RestaurantOrderLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 17,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 19,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 20,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 21,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 22,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 23,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 24,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 25,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 26,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 27,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 28,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 29,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 30,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 31,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 32,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 33,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 34,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 35,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 36,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 37,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 38,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 39,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 40,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 41,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 42,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 43,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 44,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 45,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 46,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 47,
                column: "DefaultKitchenStationId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Products_DefaultKitchenStationId",
                table: "Products",
                column: "DefaultKitchenStationId");

            migrationBuilder.CreateIndex(
                name: "IX_KitchenStations_BranchId_Name",
                table: "KitchenStations",
                columns: new[] { "BranchId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_KitchenStations_RecordId",
                table: "KitchenStations",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KitchenTicketLines_KitchenTicketId_RestaurantOrderLineId",
                table: "KitchenTicketLines",
                columns: new[] { "KitchenTicketId", "RestaurantOrderLineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KitchenTicketLines_RecordId",
                table: "KitchenTicketLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KitchenTicketLines_RestaurantOrderLineId",
                table: "KitchenTicketLines",
                column: "RestaurantOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_KitchenTickets_KitchenStationId",
                table: "KitchenTickets",
                column: "KitchenStationId");

            migrationBuilder.CreateIndex(
                name: "IX_KitchenTickets_RecordId",
                table: "KitchenTickets",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KitchenTickets_RestaurantOrderId_KitchenStationId",
                table: "KitchenTickets",
                columns: new[] { "RestaurantOrderId", "KitchenStationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KitchenTickets_SubmissionKey",
                table: "KitchenTickets",
                column: "SubmissionKey",
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPortions_ProductId_Name",
                table: "ProductPortions",
                columns: new[] { "ProductId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPortions_RecordId",
                table: "ProductPortions",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipeHeaders_BranchId",
                table: "ProductRecipeHeaders",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipeHeaders_OneActiveVersion",
                table: "ProductRecipeHeaders",
                columns: new[] { "ProductId", "ProductPortionId", "BranchId" },
                unique: true,
                filter: "[ValidToUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipeHeaders_ProductPortionId",
                table: "ProductRecipeHeaders",
                column: "ProductPortionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipeHeaders_ProductPortionId1",
                table: "ProductRecipeHeaders",
                column: "ProductPortionId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipeHeaders_RecordId",
                table: "ProductRecipeHeaders",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipeHeaders_WarehouseId",
                table: "ProductRecipeHeaders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipeLines_IngredientProductId",
                table: "ProductRecipeLines",
                column: "IngredientProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipeLines_ProductRecipeHeaderId",
                table: "ProductRecipeLines",
                column: "ProductRecipeHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipeLines_RecordId",
                table: "ProductRecipeLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipeLines_UnitOfMeasureId",
                table: "ProductRecipeLines",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantCashShifts_BranchId",
                table: "RestaurantCashShifts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantCashShifts_OneOpenPerAccount",
                table: "RestaurantCashShifts",
                column: "FinancialAccountId",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantCashShifts_RecordId",
                table: "RestaurantCashShifts",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantCashShifts_SubmissionKey",
                table: "RestaurantCashShifts",
                column: "SubmissionKey",
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantChecks_CheckNumber",
                table: "RestaurantChecks",
                column: "CheckNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantChecks_LinkedInvoiceId",
                table: "RestaurantChecks",
                column: "LinkedInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantChecks_LinkedRetailSaleId",
                table: "RestaurantChecks",
                column: "LinkedRetailSaleId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantChecks_RecordId",
                table: "RestaurantChecks",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantChecks_RestaurantTableSessionId_OpenedAtUtc",
                table: "RestaurantChecks",
                columns: new[] { "RestaurantTableSessionId", "OpenedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantChecks_SubmissionKey",
                table: "RestaurantChecks",
                column: "SubmissionKey",
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrderLineModifiers_RecordId",
                table: "RestaurantOrderLineModifiers",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrderLineModifiers_RestaurantOrderLineId",
                table: "RestaurantOrderLineModifiers",
                column: "RestaurantOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrderLines_ProductId",
                table: "RestaurantOrderLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrderLines_ProductPortionId",
                table: "RestaurantOrderLines",
                column: "ProductPortionId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrderLines_RecordId",
                table: "RestaurantOrderLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrderLines_RestaurantOrderId",
                table: "RestaurantOrderLines",
                column: "RestaurantOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrderLines_Status",
                table: "RestaurantOrderLines",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrders_RecordId",
                table: "RestaurantOrders",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrders_RestaurantCheckId_OrderedAtUtc",
                table: "RestaurantOrders",
                columns: new[] { "RestaurantCheckId", "OrderedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrders_SubmissionKey",
                table: "RestaurantOrders",
                column: "SubmissionKey",
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPayments_FinancialAccountId",
                table: "RestaurantPayments",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPayments_FinancialTransactionId",
                table: "RestaurantPayments",
                column: "FinancialTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPayments_RecordId",
                table: "RestaurantPayments",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPayments_RestaurantCheckId",
                table: "RestaurantPayments",
                column: "RestaurantCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPayments_ReversalOfId",
                table: "RestaurantPayments",
                column: "ReversalOfId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantPayments_SubmissionKey",
                table: "RestaurantPayments",
                column: "SubmissionKey",
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantSections_BranchId_Name",
                table: "RestaurantSections",
                columns: new[] { "BranchId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantSections_RecordId",
                table: "RestaurantSections",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTables_RecordId",
                table: "RestaurantTables",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTables_RestaurantSectionId_Name",
                table: "RestaurantTables",
                columns: new[] { "RestaurantSectionId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTableSessionMoves_FromRestaurantTableId",
                table: "RestaurantTableSessionMoves",
                column: "FromRestaurantTableId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTableSessionMoves_RecordId",
                table: "RestaurantTableSessionMoves",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTableSessionMoves_RestaurantTableSessionId",
                table: "RestaurantTableSessionMoves",
                column: "RestaurantTableSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTableSessionMoves_ToRestaurantTableId",
                table: "RestaurantTableSessionMoves",
                column: "ToRestaurantTableId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTableSessions_MergedIntoSessionId",
                table: "RestaurantTableSessions",
                column: "MergedIntoSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTableSessions_OneOpenPerTable",
                table: "RestaurantTableSessions",
                column: "RestaurantTableId",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTableSessions_RecordId",
                table: "RestaurantTableSessions",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTableSessions_SubmissionKey",
                table: "RestaurantTableSessions",
                column: "SubmissionKey",
                unique: true,
                filter: "[SubmissionKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RetailSaleLines_ProductId",
                table: "RetailSaleLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RetailSaleLines_RecordId",
                table: "RetailSaleLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetailSaleLines_RetailSaleId",
                table: "RetailSaleLines",
                column: "RetailSaleId");

            migrationBuilder.CreateIndex(
                name: "IX_RetailSales_CustomerId",
                table: "RetailSales",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RetailSales_DocumentNumber",
                table: "RetailSales",
                column: "DocumentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetailSales_RecordId",
                table: "RetailSales",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetailSales_RestaurantCheckId",
                table: "RetailSales",
                column: "RestaurantCheckId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_KitchenStations_DefaultKitchenStationId",
                table: "Products",
                column: "DefaultKitchenStationId",
                principalTable: "KitchenStations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_KitchenTicketLines_KitchenTickets_KitchenTicketId",
                table: "KitchenTicketLines",
                column: "KitchenTicketId",
                principalTable: "KitchenTickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KitchenTicketLines_RestaurantOrderLines_RestaurantOrderLineId",
                table: "KitchenTicketLines",
                column: "RestaurantOrderLineId",
                principalTable: "RestaurantOrderLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KitchenTickets_RestaurantOrders_RestaurantOrderId",
                table: "KitchenTickets",
                column: "RestaurantOrderId",
                principalTable: "RestaurantOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantChecks_RetailSales_LinkedRetailSaleId",
                table: "RestaurantChecks",
                column: "LinkedRetailSaleId",
                principalTable: "RetailSales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_KitchenStations_DefaultKitchenStationId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantChecks_RestaurantTableSessions_RestaurantTableSessionId",
                table: "RestaurantChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantChecks_RetailSales_LinkedRetailSaleId",
                table: "RestaurantChecks");

            migrationBuilder.DropTable(
                name: "KitchenTicketLines");

            migrationBuilder.DropTable(
                name: "ProductRecipeLines");

            migrationBuilder.DropTable(
                name: "RestaurantCashShifts");

            migrationBuilder.DropTable(
                name: "RestaurantOrderLineModifiers");

            migrationBuilder.DropTable(
                name: "RestaurantPayments");

            migrationBuilder.DropTable(
                name: "RestaurantTableSessionMoves");

            migrationBuilder.DropTable(
                name: "RetailSaleLines");

            migrationBuilder.DropTable(
                name: "KitchenTickets");

            migrationBuilder.DropTable(
                name: "ProductRecipeHeaders");

            migrationBuilder.DropTable(
                name: "RestaurantOrderLines");

            migrationBuilder.DropTable(
                name: "KitchenStations");

            migrationBuilder.DropTable(
                name: "ProductPortions");

            migrationBuilder.DropTable(
                name: "RestaurantOrders");

            migrationBuilder.DropTable(
                name: "RestaurantTableSessions");

            migrationBuilder.DropTable(
                name: "RestaurantTables");

            migrationBuilder.DropTable(
                name: "RestaurantSections");

            migrationBuilder.DropTable(
                name: "RetailSales");

            migrationBuilder.DropTable(
                name: "RestaurantChecks");

            migrationBuilder.DropIndex(
                name: "IX_Products_DefaultKitchenStationId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DefaultKitchenStationId",
                table: "Products");
        }
    }
}
