using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer2022 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsHeadOffice = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxOffice = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Iban = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: true),
                    LogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CostCenters",
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
                    table.PrimaryKey("PK_CostCenters", x => x.Id);
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
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxOffice = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    IdentityNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCustomer = table.Column<bool>(type: "bit", nullable: false),
                    IsSupplier = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
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
                name: "FinancialAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Iban = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialAccounts", x => x.Id);
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
                name: "InventorySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequireBarcode = table.Column<bool>(type: "bit", nullable: false),
                    AutoGenerateBarcode = table.Column<bool>(type: "bit", nullable: false),
                    DefaultBarcodeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DefaultScalePrefix = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    EnforceStockLevel = table.Column<bool>(type: "bit", nullable: false),
                    AllowNegativeStock = table.Column<bool>(type: "bit", nullable: false),
                    AllowSaleWhenOutOfStock = table.Column<bool>(type: "bit", nullable: false),
                    EnableMinimumStockWarning = table.Column<bool>(type: "bit", nullable: false),
                    RequireTransferApproval = table.Column<bool>(type: "bit", nullable: false),
                    TrackStockByVariant = table.Column<bool>(type: "bit", nullable: false),
                    RequireProductVariant = table.Column<bool>(type: "bit", nullable: false),
                    AllowSaleBelowCost = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventorySettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NumberSequences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NextNumber = table.Column<long>(type: "bigint", nullable: false),
                    Padding = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumberSequences", x => x.Id);
                    table.CheckConstraint("CK_NumberSequences_NextNumber", "[NextNumber] > 0");
                    table.CheckConstraint("CK_NumberSequences_Padding", "[Padding] BETWEEN 1 AND 12");
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WebsitePath = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HexCode = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductColors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsExempt = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRates", x => x.Id);
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
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warehouses_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
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
                name: "CustomerAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AddressType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CountryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerAddresses_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerContacts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptType = table.Column<int>(type: "int", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReceiptDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    BusinessProjectId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentReceipts_BusinessProjects_BusinessProjectId",
                        column: x => x.BusinessProjectId,
                        principalTable: "BusinessProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PaymentReceipts_CostCenters_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PaymentReceipts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchasePriceLists",
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
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchasePriceLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchasePriceLists_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuoteNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    QuoteDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                    table.CheckConstraint("CK_Quotes_Totals", "[Subtotal] >= 0 AND [DiscountTotal] >= 0 AND [TaxTotal] >= 0 AND [GrandTotal] >= 0");
                    table.ForeignKey(
                        name: "FK_Quotes_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
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
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StockQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MinimumStockQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TrackStock = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WebsitePath = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    TaxRateId = table.Column<int>(type: "int", nullable: false),
                    TrackSerialNumbers = table.Column<bool>(type: "bit", nullable: false),
                    TrackLots = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.CheckConstraint("CK_Products_Prices", "[PurchasePrice] >= 0 AND [SalePrice] >= 0");
                    table.CheckConstraint("CK_Products_Quantities", "[MinimumStockQuantity] >= 0");
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_TaxRates_TaxRateId",
                        column: x => x.TaxRateId,
                        principalTable: "TaxRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "StockTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TransferDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FromWarehouseId = table.Column<int>(type: "int", nullable: false),
                    ToWarehouseId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransfers", x => x.Id);
                    table.CheckConstraint("CK_StockTransfers_DifferentWarehouses", "[FromWarehouseId] <> [ToWarehouseId]");
                    table.ForeignKey(
                        name: "FK_StockTransfers_Warehouses_FromWarehouseId",
                        column: x => x.FromWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransfers_Warehouses_ToWarehouseId",
                        column: x => x.ToWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    InvoiceDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    QuoteId = table.Column<int>(type: "int", nullable: true),
                    PurchasePriceListId = table.Column<int>(type: "int", nullable: true),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    BusinessProjectId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.CheckConstraint("CK_Invoices_Totals", "[Subtotal] >= 0 AND [DiscountTotal] >= 0 AND [TaxTotal] >= 0 AND [GrandTotal] >= 0");
                    table.ForeignKey(
                        name: "FK_Invoices_BusinessProjects_BusinessProjectId",
                        column: x => x.BusinessProjectId,
                        principalTable: "BusinessProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_CostCenters_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_PurchasePriceLists_PurchasePriceListId",
                        column: x => x.PurchasePriceListId,
                        principalTable: "PurchasePriceLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariantCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VariantName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AdditionalPurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdditionalSalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ColorId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_ProductColors_ColorId",
                        column: x => x.ColorId,
                        principalTable: "ProductColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchasePriceListItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinimumQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    SupplierProductCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchasePriceListId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchasePriceListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchasePriceListItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchasePriceListItems_PurchasePriceLists_PurchasePriceListId",
                        column: x => x.PurchasePriceListId,
                        principalTable: "PurchasePriceLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuoteLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    ProductCodeSnapshot = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitSnapshot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuoteId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuoteLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QuoteLines_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScaleProductSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeasurementType = table.Column<int>(type: "int", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    PluCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    BarcodeContainsPrice = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScaleProductSettings", x => x.Id);
                    table.CheckConstraint("CK_ScaleProductSettings_PluLength", "LEN([PluCode]) = 5 AND [PluCode] NOT LIKE '%[^0-9]%'");
                    table.CheckConstraint("CK_ScaleProductSettings_Prefix", "[Prefix] IN (N'27', N'28', N'29')");
                    table.ForeignKey(
                        name: "FK_ScaleProductSettings_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurrentAccountTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    QuoteId = table.Column<int>(type: "int", nullable: true),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentAccountTransactions", x => x.Id);
                    table.CheckConstraint("CK_CurrentAccountTransactions_DebitCredit", "([Debit] > 0 AND [Credit] = 0) OR ([Credit] > 0 AND [Debit] = 0)");
                    table.ForeignKey(
                        name: "FK_CurrentAccountTransactions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurrentAccountTransactions_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CurrentAccountTransactions_Quotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quotes",
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
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BusinessOrderLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "InvoiceLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    ProductCodeSnapshot = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitSnapshot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductBarcodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BarcodeType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UnitMultiplier = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBarcodes", x => x.Id);
                    table.CheckConstraint("CK_ProductBarcodes_Length", "([BarcodeType] = N'EAN13' AND LEN([Barcode]) = 13) OR ([BarcodeType] = N'EAN8' AND LEN([Barcode]) = 8) OR ([BarcodeType] = N'SCALE' AND LEN([Barcode]) = 7) OR [BarcodeType] = N'OTHER'");
                    table.CheckConstraint("CK_ProductBarcodes_Numeric", "[BarcodeType] = N'OTHER' OR [Barcode] NOT LIKE '%[^0-9]%'");
                    table.ForeignKey(
                        name: "FK_ProductBarcodes_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductBarcodes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AltText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "StockTransferLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StockTransferId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferLines", x => x.Id);
                    table.CheckConstraint("CK_StockTransferLines_PositiveQuantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_StockTransferLines_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockTransferLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransferLines_StockTransfers_StockTransferId",
                        column: x => x.StockTransferId,
                        principalTable: "StockTransfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "FinancialTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinancialAccountId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    CurrentAccountTransactionId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.Id);
                    table.CheckConstraint("CK_FinancialTransactions_Amount", "[Amount] > 0");
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_CurrentAccountTransactions_CurrentAccountTransactionId",
                        column: x => x.CurrentAccountTransactionId,
                        principalTable: "CurrentAccountTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
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
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispatchNoteLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovementDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MovementType = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    InvoiceLineId = table.Column<int>(type: "int", nullable: true),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    BusinessProjectId = table.Column<int>(type: "int", nullable: true),
                    ProductVariantId = table.Column<int>(type: "int", nullable: true),
                    StockTransferLineId = table.Column<int>(type: "int", nullable: true),
                    StockSlipLineId = table.Column<int>(type: "int", nullable: true),
                    InventoryCountLineId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockMovements_BusinessProjects_BusinessProjectId",
                        column: x => x.BusinessProjectId,
                        principalTable: "BusinessProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockMovements_CostCenters_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockMovements_InventoryCountLines_InventoryCountLineId",
                        column: x => x.InventoryCountLineId,
                        principalTable: "InventoryCountLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockMovements_InvoiceLines_InvoiceLineId",
                        column: x => x.InvoiceLineId,
                        principalTable: "InvoiceLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockMovements_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockMovements_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_StockSlipLines_StockSlipLineId",
                        column: x => x.StockSlipLineId,
                        principalTable: "StockSlipLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockMovements_StockTransferLines_StockTransferLineId",
                        column: x => x.StockTransferLineId,
                        principalTable: "StockTransferLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StockMovements_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentReceiptLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    DueDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentReceiptId = table.Column<int>(type: "int", nullable: false),
                    FinancialAccountId = table.Column<int>(type: "int", nullable: false),
                    CurrentAccountTransactionId = table.Column<int>(type: "int", nullable: true),
                    FinancialTransactionId = table.Column<int>(type: "int", nullable: true),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentReceiptLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentReceiptLines_CurrentAccountTransactions_CurrentAccountTransactionId",
                        column: x => x.CurrentAccountTransactionId,
                        principalTable: "CurrentAccountTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PaymentReceiptLines_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentReceiptLines_FinancialTransactions_FinancialTransactionId",
                        column: x => x.FinancialTransactionId,
                        principalTable: "FinancialTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PaymentReceiptLines_PaymentReceipts_PaymentReceiptId",
                        column: x => x.PaymentReceiptId,
                        principalTable: "PaymentReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "Id", "Address", "Code", "CreatedAtUtc", "IsActive", "IsHeadOffice", "Name", "Phone", "UpdatedAtUtc" },
                values: new object[] { 1, null, "MERKEZ", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "Merkez Şube", null, null });

            migrationBuilder.InsertData(
                table: "CompanySettings",
                columns: new[] { "Id", "Address", "BankName", "CompanyName", "CreatedAtUtc", "Email", "Iban", "LogoPath", "Phone", "TaxNumber", "TaxOffice", "UpdatedAtUtc", "Website" },
                values: new object[] { 1, null, null, "ŞahinSoft", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, null, null, null });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "IsActive", "IsBaseCurrency", "Name", "Symbol", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, "TRY", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "Türk Lirası", "₺", null },
                    { 2, "USD", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "Amerikan Doları", "$", null },
                    { 3, "EUR", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "Euro", "€", null }
                });

            migrationBuilder.InsertData(
                table: "InventorySettings",
                columns: new[] { "Id", "AllowNegativeStock", "AllowSaleBelowCost", "AllowSaleWhenOutOfStock", "AutoGenerateBarcode", "CreatedAtUtc", "DefaultBarcodeType", "DefaultScalePrefix", "EnableMinimumStockWarning", "EnforceStockLevel", "RequireBarcode", "RequireProductVariant", "RequireTransferApproval", "TrackStockByVariant", "UpdatedAtUtc" },
                values: new object[] { 1, false, false, false, true, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "EAN13", "27", true, true, true, false, true, false, null });

            migrationBuilder.InsertData(
                table: "NumberSequences",
                columns: new[] { "Id", "CreatedAtUtc", "Key", "NextNumber", "Padding", "Prefix", "UpdatedAtUtc" },
                values: new object[] { 1, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "STOCK", 1L, 3, "SHN.", null });

            migrationBuilder.InsertData(
                table: "ProductCategories",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "IsActive", "Name", "UpdatedAtUtc", "WebsitePath" },
                values: new object[,]
                {
                    { 1, "YAZARKASA", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "Yazar Kasa POS", null, "yazarkasa-pos.html" },
                    { 2, "TERAZI", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "Teraziler", null, "teraziler.html" },
                    { 3, "BARKOD", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "Barkod Okuyucular", null, "barkod-okuyucular.html" },
                    { 4, "YAZICI", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "Yazıcılar", null, "yazicilar.html" },
                    { 5, "ELTERM", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "El Terminalleri", null, "el-terminali.html" },
                    { 6, "POSPC", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "Dokunmatik POS", null, "dokunmatik-pos.html" },
                    { 7, "YAZILIM", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "Yazılım ve Entegrasyon", null, "kurumsal-yazilim.html" },
                    { 8, "POSEKIP", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "POS Çevre Birimleri", null, "index.html" }
                });

            migrationBuilder.InsertData(
                table: "TaxRates",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "IsActive", "IsExempt", "Name", "Rate", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, "KDV10", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "KDV %10", 10m, null },
                    { 2, "KDV20", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "KDV %20", 20m, null },
                    { 3, "KDV0", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "KDV %0", 0m, null },
                    { 4, "KDV1", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "KDV %1", 1m, null }
                });

            migrationBuilder.InsertData(
                table: "UnitsOfMeasure",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "DecimalPlaces", "IsActive", "Name", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, "ADET", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), 0, true, "Adet", null },
                    { 2, "KG", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, "Kilogram", null },
                    { 3, "PAKET", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), 0, true, "Paket", null }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Barcode", "Brand", "CategoryId", "CreatedAtUtc", "Description", "ImagePath", "IsActive", "MinimumStockQuantity", "Model", "Name", "ProductType", "PurchasePrice", "SalePrice", "StockCode", "StockQuantity", "TaxRateId", "TrackLots", "TrackSerialNumbers", "TrackStock", "Unit", "UpdatedAtUtc", "WebsitePath" },
                values: new object[,]
                {
                    { 1, "2000000000015", "Ingenico", 1, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "IDE280", "Ingenico IDE280", "Donanım", 0m, 0m, "YK-0001", 0m, 1, false, false, true, "Adet", null, "yazarkasa-pos.html" },
                    { 2, "2000000000022", "Ingenico", 1, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Move 5000F", "Ingenico Move 5000F", "Donanım", 0m, 0m, "YK-0002", 0m, 1, false, false, true, "Adet", null, "yazarkasa-pos.html" },
                    { 3, "2000000000039", "PayGo", 1, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "SP630PRO ECR", "PAYGO SP630PRO ECR", "Donanım", 0m, 0m, "YK-0003", 0m, 1, false, false, true, "Adet", null, "yazarkasa-pos.html" },
                    { 4, "2000000000046", "Profilo", 1, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "S900", "Profilo S900", "Donanım", 0m, 0m, "YK-0004", 0m, 1, false, false, true, "Adet", null, "yazarkasa-pos.html" },
                    { 5, "2000000000053", "inPOS", 1, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "m530", "inPOS m530 Mobil POS", "Donanım", 0m, 0m, "YK-0005", 0m, 1, false, false, true, "Adet", null, "yazarkasa-pos.html" },
                    { 6, "2000000000060", "CAS", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "CL3000", "CAS CL3000 Market Terazisi", "Donanım", 0m, 0m, "TR-0001", 0m, 2, false, false, true, "Adet", null, "teraziler.html" },
                    { 7, "2000000000077", "CAS", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "CL8000", "CAS CL8000 Dokunmatik Terazi", "Donanım", 0m, 0m, "TR-0002", 0m, 2, false, false, true, "Adet", null, "teraziler.html" },
                    { 8, "2000000000084", "CAS", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "CN-1", "CAS CN-1 Sistem Terazisi", "Donanım", 0m, 0m, "TR-0003", 0m, 2, false, false, true, "Adet", null, "teraziler.html" },
                    { 9, "2000000000091", "Digi", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "SM100P", "Digi SM100P Boyunlu Terazi", "Donanım", 0m, 0m, "TR-0004", 0m, 2, false, false, true, "Adet", null, "teraziler.html" },
                    { 10, "2000000000107", "Digi", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "SM-120T", "Digi SM-120T Dokunmatik Terazi", "Donanım", 0m, 0m, "TR-0005", 0m, 2, false, false, true, "Adet", null, "teraziler.html" },
                    { 11, "2000000000114", "CAS", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "ER-JR", "CAS ER-JR Masaüstü Terazi", "Donanım", 0m, 0m, "TR-0006", 0m, 2, false, false, true, "Adet", null, "teraziler.html" },
                    { 12, "2000000000121", "CAS", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "FW-500", "CAS FW-500 Su Geçirmez Terazi", "Donanım", 0m, 0m, "TR-0007", 0m, 2, false, false, true, "Adet", null, "teraziler.html" },
                    { 13, "2000000000138", "CAS", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "PDI", "CAS PDI Ankastre Kasa Terazisi", "Donanım", 0m, 0m, "TR-0008", 0m, 2, false, false, true, "Adet", null, "teraziler.html" },
                    { 14, "2000000000145", "Hillpos", 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HRS-28", "Hillpos HRS-28", "Donanım", 0m, 0m, "BO-0001", 0m, 2, false, false, true, "Adet", null, "barkod-okuyucular.html" },
                    { 15, "2000000000152", "Hillpos", 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HSC-82", "Hillpos HSC-82", "Donanım", 0m, 0m, "BO-0002", 0m, 2, false, false, true, "Adet", null, "barkod-okuyucular.html" },
                    { 16, "2000000000169", "Hillpos", 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HSD-92", "Hillpos HSD-92", "Donanım", 0m, 0m, "BO-0003", 0m, 2, false, false, true, "Adet", null, "barkod-okuyucular.html" },
                    { 17, "2000000000176", "Newland", 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "TP-13", "Newland TP-13", "Donanım", 0m, 0m, "BO-0004", 0m, 2, false, false, true, "Adet", null, "barkod-okuyucular.html" },
                    { 18, "2000000000183", "Newland", 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "TP-14", "Newland TP-14", "Donanım", 0m, 0m, "BO-0005", 0m, 2, false, false, true, "Adet", null, "barkod-okuyucular.html" },
                    { 19, "2000000000190", "Hillpos", 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HS-6700", "Hillpos HS-6700", "Donanım", 0m, 0m, "BO-0006", 0m, 2, false, false, true, "Adet", null, "barkod-okuyucular.html" },
                    { 20, "2000000000206", "Hillpos", 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "VS-6800", "Hillpos VS-6800", "Donanım", 0m, 0m, "BO-0007", 0m, 2, false, false, true, "Adet", null, "barkod-okuyucular.html" },
                    { 21, "2000000000213", "Argox", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "OS-214 Plus", "Argox OS-214 Plus Barkod Yazıcı", "Donanım", 0m, 0m, "YZ-0001", 0m, 2, false, false, true, "Adet", null, "yazicilar.html" },
                    { 22, "2000000000220", "Hillpos", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HDT-400", "Hillpos HDT-400 Barkod Yazıcı", "Donanım", 0m, 0m, "YZ-0002", 0m, 2, false, false, true, "Adet", null, "yazicilar.html" },
                    { 23, "2000000000237", "Hillpos", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HTT-440", "Hillpos HTT-440 Barkod Yazıcı", "Donanım", 0m, 0m, "YZ-0003", 0m, 2, false, false, true, "Adet", null, "yazicilar.html" },
                    { 24, "2000000000244", "TSC", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "TTP-244CE", "TSC TTP-244CE Barkod Yazıcı", "Donanım", 0m, 0m, "YZ-0004", 0m, 2, false, false, true, "Adet", null, "yazicilar.html" },
                    { 25, "2000000000251", "Xprinter", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "XP-470B", "Xprinter XP-470B Barkod Yazıcı", "Donanım", 0m, 0m, "YZ-0005", 0m, 2, false, false, true, "Adet", null, "yazicilar.html" },
                    { 26, "2000000000268", "Hillpos", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "H380", "Hillpos H380 Fiş Yazıcı", "Donanım", 0m, 0m, "YZ-0006", 0m, 2, false, false, true, "Adet", null, "yazicilar.html" },
                    { 27, "2000000000275", "Hillpos", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Q800", "Hillpos Q800 Fiş Yazıcı", "Donanım", 0m, 0m, "YZ-0007", 0m, 2, false, false, true, "Adet", null, "yazicilar.html" },
                    { 28, "2000000000282", "Bixolon", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "SPP-R310", "Bixolon SPP-R310 Mobil Fiş Yazıcı", "Donanım", 0m, 0m, "YZ-0008", 0m, 2, false, false, true, "Adet", null, "yazicilar.html" },
                    { 29, "2000000000299", "Chainway", 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "C61", "Chainway C61", "Donanım", 0m, 0m, "ET-0001", 0m, 2, false, false, true, "Adet", null, "el-terminali.html" },
                    { 30, "2000000000305", "Chainway", 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "C66", "Chainway C66", "Donanım", 0m, 0m, "ET-0002", 0m, 2, false, false, true, "Adet", null, "el-terminali.html" },
                    { 31, "2000000000312", "Hillpos", 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "C7X", "Hillpos C7X Tablet", "Donanım", 0m, 0m, "ET-0003", 0m, 2, false, false, true, "Adet", null, "el-terminali.html" },
                    { 32, "2000000000329", "Hillpos", 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "CM550X", "Hillpos CM550X", "Donanım", 0m, 0m, "ET-0004", 0m, 2, false, false, true, "Adet", null, "el-terminali.html" },
                    { 33, "2000000000336", "Hillpos", 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HT42", "Hillpos HT42", "Donanım", 0m, 0m, "ET-0005", 0m, 2, false, false, true, "Adet", null, "el-terminali.html" },
                    { 34, "2000000000343", "Hillpos", 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HT42K", "Hillpos HT42K", "Donanım", 0m, 0m, "ET-0006", 0m, 2, false, false, true, "Adet", null, "el-terminali.html" },
                    { 35, "2000000000350", "Hillpos", 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HT44", "Hillpos HT44", "Donanım", 0m, 0m, "ET-0007", 0m, 2, false, false, true, "Adet", null, "el-terminali.html" },
                    { 36, "2000000000367", "Hillpos", 6, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Touch Pro 15", "Hillpos Touch Pro 15", "Donanım", 0m, 0m, "PC-0001", 0m, 2, false, false, true, "Adet", null, "dokunmatik-pos.html" },
                    { 37, "2000000000374", "Hillpos", 6, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "All-in-One Dual POS", "Hillpos All-in-One Dual POS", "Donanım", 0m, 0m, "PC-0002", 0m, 2, false, false, true, "Adet", null, "dokunmatik-pos.html" },
                    { 38, "2000000000381", "Hillpos", 6, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Slim Touch 15", "Hillpos Slim Touch 15", "Donanım", 0m, 0m, "PC-0003", 0m, 2, false, false, true, "Adet", null, "dokunmatik-pos.html" },
                    { 39, "2000000000398", "Hillpos", 6, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Kiosk POS 21.5", "Hillpos Kiosk POS 21.5", "Donanım", 0m, 0m, "PC-0004", 0m, 2, false, false, true, "Adet", null, "dokunmatik-pos.html" },
                    { 40, "2000000000404", null, 7, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, null, "Özel ERP & CRM Yazılımları", "Yazılım", 0m, 0m, "YW-0001", 0m, 3, false, false, false, "Adet", null, "kurumsal-yazilim.html" },
                    { 41, "2000000000411", null, 7, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, null, "Stok ve Depo Yönetimi Yazılımı", "Yazılım", 0m, 0m, "YW-0002", 0m, 3, false, false, false, "Adet", null, "kurumsal-yazilim.html" },
                    { 42, "2000000000428", null, 7, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, null, "Fabrika ve Üretim Takibi Yazılımı", "Yazılım", 0m, 0m, "YW-0003", 0m, 3, false, false, false, "Adet", null, "kurumsal-yazilim.html" },
                    { 43, "2000000000435", null, 7, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, null, "API ve Donanım Entegrasyonları", "Yazılım", 0m, 0m, "YW-0004", 0m, 3, false, false, false, "Adet", null, "kurumsal-yazilim.html" },
                    { 44, "2000000000442", null, 7, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, null, "GİB & E-Fatura Çözümleri", "Yazılım", 0m, 0m, "YW-0005", 0m, 3, false, false, false, "Adet", null, "kurumsal-yazilim.html" },
                    { 45, "2000000000459", "Genel", 8, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Metal Kasa", "Para Çekmecesi", "Donanım", 0m, 0m, "PE-0001", 0m, 2, false, false, true, "Adet", null, "index.html" },
                    { 46, "2000000000466", "Genel", 8, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Fiyat Sorgulama Terminali", "Fiyat Gör Cihazı", "Donanım", 0m, 0m, "PE-0002", 0m, 2, false, false, true, "Adet", null, "index.html" },
                    { 47, "2000000000473", "Genel", 8, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Mobil Fiş Yazıcı", "Mobil Yazıcı", "Donanım", 0m, 0m, "PE-0003", 0m, 2, false, false, true, "Adet", null, "index.html" }
                });

            migrationBuilder.InsertData(
                table: "Warehouses",
                columns: new[] { "Id", "BranchId", "Code", "CreatedAtUtc", "IsActive", "IsDefault", "Name", "UpdatedAtUtc" },
                values: new object[] { 1, 1, "MERKEZ", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "Merkez Depo", null });

            migrationBuilder.InsertData(
                table: "ProductBarcodes",
                columns: new[] { "Id", "Barcode", "BarcodeType", "CreatedAtUtc", "IsActive", "IsPrimary", "ProductId", "ProductVariantId", "UnitMultiplier", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, "2000000000015", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 1, null, 1m, null },
                    { 2, "2000000000022", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 2, null, 1m, null },
                    { 3, "2000000000039", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 3, null, 1m, null },
                    { 4, "2000000000046", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 4, null, 1m, null },
                    { 5, "2000000000053", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 5, null, 1m, null },
                    { 6, "2000000000060", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 6, null, 1m, null },
                    { 7, "2000000000077", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 7, null, 1m, null },
                    { 8, "2000000000084", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 8, null, 1m, null },
                    { 9, "2000000000091", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 9, null, 1m, null },
                    { 10, "2000000000107", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 10, null, 1m, null },
                    { 11, "2000000000114", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 11, null, 1m, null },
                    { 12, "2000000000121", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 12, null, 1m, null },
                    { 13, "2000000000138", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 13, null, 1m, null },
                    { 14, "2000000000145", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 14, null, 1m, null },
                    { 15, "2000000000152", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 15, null, 1m, null },
                    { 16, "2000000000169", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 16, null, 1m, null },
                    { 17, "2000000000176", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 17, null, 1m, null },
                    { 18, "2000000000183", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 18, null, 1m, null },
                    { 19, "2000000000190", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 19, null, 1m, null },
                    { 20, "2000000000206", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 20, null, 1m, null },
                    { 21, "2000000000213", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 21, null, 1m, null },
                    { 22, "2000000000220", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 22, null, 1m, null },
                    { 23, "2000000000237", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 23, null, 1m, null },
                    { 24, "2000000000244", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 24, null, 1m, null },
                    { 25, "2000000000251", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 25, null, 1m, null },
                    { 26, "2000000000268", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 26, null, 1m, null },
                    { 27, "2000000000275", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 27, null, 1m, null },
                    { 28, "2000000000282", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 28, null, 1m, null },
                    { 29, "2000000000299", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 29, null, 1m, null },
                    { 30, "2000000000305", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 30, null, 1m, null },
                    { 31, "2000000000312", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 31, null, 1m, null },
                    { 32, "2000000000329", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 32, null, 1m, null },
                    { 33, "2000000000336", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 33, null, 1m, null },
                    { 34, "2000000000343", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 34, null, 1m, null },
                    { 35, "2000000000350", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 35, null, 1m, null },
                    { 36, "2000000000367", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 36, null, 1m, null },
                    { 37, "2000000000374", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 37, null, 1m, null },
                    { 38, "2000000000381", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 38, null, 1m, null },
                    { 39, "2000000000398", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 39, null, 1m, null },
                    { 40, "2000000000404", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 40, null, 1m, null },
                    { 41, "2000000000411", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 41, null, 1m, null },
                    { 42, "2000000000428", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 42, null, 1m, null },
                    { 43, "2000000000435", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 43, null, 1m, null },
                    { 44, "2000000000442", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 44, null, 1m, null },
                    { 45, "2000000000459", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 45, null, 1m, null },
                    { 46, "2000000000466", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 46, null, 1m, null },
                    { 47, "2000000000473", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 47, null, 1m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_RecordId",
                table: "AuditLogs",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_CreatedAtUtc",
                table: "AuditLogs",
                columns: new[] { "UserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Code",
                table: "Branches",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_RecordId",
                table: "Branches",
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
                name: "IX_BusinessProjects_Code",
                table: "BusinessProjects",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProjects_RecordId",
                table: "BusinessProjects",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySettings_RecordId",
                table: "CompanySettings",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_Code",
                table: "CostCenters",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_RecordId",
                table: "CostCenters",
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
                name: "IX_CurrentAccountTransactions_CustomerId_TransactionDateUtc",
                table: "CurrentAccountTransactions",
                columns: new[] { "CustomerId", "TransactionDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CurrentAccountTransactions_DocumentNumber",
                table: "CurrentAccountTransactions",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentAccountTransactions_InvoiceId",
                table: "CurrentAccountTransactions",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentAccountTransactions_QuoteId",
                table: "CurrentAccountTransactions",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentAccountTransactions_RecordId",
                table: "CurrentAccountTransactions",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAddresses_CustomerId_AddressType",
                table: "CustomerAddresses",
                columns: new[] { "CustomerId", "AddressType" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAddresses_RecordId",
                table: "CustomerAddresses",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContacts_CustomerId",
                table: "CustomerContacts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerContacts_RecordId",
                table: "CustomerContacts",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Code",
                table: "Customers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Name",
                table: "Customers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_RecordId",
                table: "Customers",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TaxNumber",
                table: "Customers",
                column: "TaxNumber");

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
                name: "IX_FinancialAccounts_Code",
                table: "FinancialAccounts",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_Iban",
                table: "FinancialAccounts",
                column: "Iban");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_RecordId",
                table: "FinancialAccounts",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_CurrentAccountTransactionId",
                table: "FinancialTransactions",
                column: "CurrentAccountTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_CustomerId",
                table: "FinancialTransactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_DocumentNumber",
                table: "FinancialTransactions",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_FinancialAccountId_TransactionDateUtc",
                table: "FinancialTransactions",
                columns: new[] { "FinancialAccountId", "TransactionDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_RecordId",
                table: "FinancialTransactions",
                column: "RecordId",
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
                name: "IX_InventorySettings_RecordId",
                table: "InventorySettings",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_InvoiceId_LineNumber",
                table: "InvoiceLines",
                columns: new[] { "InvoiceId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_ProductId",
                table: "InvoiceLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_ProductVariantId",
                table: "InvoiceLines",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_RecordId",
                table: "InvoiceLines",
                column: "RecordId",
                unique: true);

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
                name: "IX_Invoices_BusinessProjectId",
                table: "Invoices",
                column: "BusinessProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CostCenterId",
                table: "Invoices",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId_InvoiceDateUtc",
                table: "Invoices",
                columns: new[] { "CustomerId", "InvoiceDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceType_InvoiceNumber",
                table: "Invoices",
                columns: new[] { "InvoiceType", "InvoiceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceType_Status_InvoiceDateUtc",
                table: "Invoices",
                columns: new[] { "InvoiceType", "Status", "InvoiceDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PurchasePriceListId",
                table: "Invoices",
                column: "PurchasePriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_QuoteId",
                table: "Invoices",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_RecordId",
                table: "Invoices",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_WarehouseId",
                table: "Invoices",
                column: "WarehouseId");

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
                name: "IX_NumberSequences_Key",
                table: "NumberSequences",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NumberSequences_RecordId",
                table: "NumberSequences",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceiptLines_CurrentAccountTransactionId",
                table: "PaymentReceiptLines",
                column: "CurrentAccountTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceiptLines_FinancialAccountId",
                table: "PaymentReceiptLines",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceiptLines_FinancialTransactionId",
                table: "PaymentReceiptLines",
                column: "FinancialTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceiptLines_PaymentReceiptId_LineNumber",
                table: "PaymentReceiptLines",
                columns: new[] { "PaymentReceiptId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceiptLines_RecordId",
                table: "PaymentReceiptLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_BusinessProjectId",
                table: "PaymentReceipts",
                column: "BusinessProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_CostCenterId",
                table: "PaymentReceipts",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_CustomerId_ReceiptDateUtc",
                table: "PaymentReceipts",
                columns: new[] { "CustomerId", "ReceiptDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_ReceiptType_ReceiptNumber",
                table: "PaymentReceipts",
                columns: new[] { "ReceiptType", "ReceiptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_RecordId",
                table: "PaymentReceipts",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_Barcode",
                table: "ProductBarcodes",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_ProductId_ProductVariantId",
                table: "ProductBarcodes",
                columns: new[] { "ProductId", "ProductVariantId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_ProductVariantId",
                table: "ProductBarcodes",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_RecordId",
                table: "ProductBarcodes",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Code",
                table: "ProductCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_RecordId",
                table: "ProductCategories",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductColors_Code",
                table: "ProductColors",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductColors_RecordId",
                table: "ProductColors",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_DisplayOrder",
                table: "ProductImages",
                columns: new[] { "ProductId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductVariantId",
                table: "ProductImages",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_RecordId",
                table: "ProductImages",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_RecordId",
                table: "Products",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_StockCode",
                table: "Products",
                column: "StockCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_TaxRateId",
                table: "Products",
                column: "TaxRateId");

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
                name: "IX_ProductVariants_ColorId",
                table: "ProductVariants",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId_ColorId",
                table: "ProductVariants",
                columns: new[] { "ProductId", "ColorId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_RecordId",
                table: "ProductVariants",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_VariantCode",
                table: "ProductVariants",
                column: "VariantCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePriceListItems_ProductId",
                table: "PurchasePriceListItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePriceListItems_PurchasePriceListId_ProductId_MinimumQuantity",
                table: "PurchasePriceListItems",
                columns: new[] { "PurchasePriceListId", "ProductId", "MinimumQuantity" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePriceListItems_RecordId",
                table: "PurchasePriceListItems",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePriceLists_Code",
                table: "PurchasePriceLists",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePriceLists_CustomerId_ValidFromUtc_ValidUntilUtc",
                table: "PurchasePriceLists",
                columns: new[] { "CustomerId", "ValidFromUtc", "ValidUntilUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePriceLists_RecordId",
                table: "PurchasePriceLists",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLines_ProductId",
                table: "QuoteLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLines_QuoteId_LineNumber",
                table: "QuoteLines",
                columns: new[] { "QuoteId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteLines_RecordId",
                table: "QuoteLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CustomerId_QuoteDateUtc",
                table: "Quotes",
                columns: new[] { "CustomerId", "QuoteDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_QuoteNumber",
                table: "Quotes",
                column: "QuoteNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_RecordId",
                table: "Quotes",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_Status",
                table: "Quotes",
                column: "Status");

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
                name: "IX_ScaleProductSettings_Prefix_PluCode",
                table: "ScaleProductSettings",
                columns: new[] { "Prefix", "PluCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScaleProductSettings_ProductId",
                table: "ScaleProductSettings",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScaleProductSettings_RecordId",
                table: "ScaleProductSettings",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_BusinessProjectId",
                table: "StockMovements",
                column: "BusinessProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_CostCenterId",
                table: "StockMovements",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_DocumentNumber",
                table: "StockMovements",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_InventoryCountLineId",
                table: "StockMovements",
                column: "InventoryCountLineId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_InvoiceLineId",
                table: "StockMovements",
                column: "InvoiceLineId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductId_WarehouseId_MovementDateUtc",
                table: "StockMovements",
                columns: new[] { "ProductId", "WarehouseId", "MovementDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductVariantId",
                table: "StockMovements",
                column: "ProductVariantId");

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
                name: "IX_StockMovements_StockTransferLineId",
                table: "StockMovements",
                column: "StockTransferLineId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_WarehouseId",
                table: "StockMovements",
                column: "WarehouseId");

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
                name: "IX_StockTransferLines_ProductId",
                table: "StockTransferLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferLines_ProductVariantId",
                table: "StockTransferLines",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferLines_RecordId",
                table: "StockTransferLines",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferLines_StockTransferId_LineNumber",
                table: "StockTransferLines",
                columns: new[] { "StockTransferId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_FromWarehouseId_TransferDateUtc",
                table: "StockTransfers",
                columns: new[] { "FromWarehouseId", "TransferDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_RecordId",
                table: "StockTransfers",
                column: "RecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_ToWarehouseId_TransferDateUtc",
                table: "StockTransfers",
                columns: new[] { "ToWarehouseId", "TransferDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_TransferNumber",
                table: "StockTransfers",
                column: "TransferNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_Code",
                table: "TaxRates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_RecordId",
                table: "TaxRates",
                column: "RecordId",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_BranchId_Name",
                table: "Warehouses",
                columns: new[] { "BranchId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Code",
                table: "Warehouses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_RecordId",
                table: "Warehouses",
                column: "RecordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BusinessOrderLines");

            migrationBuilder.DropTable(
                name: "CompanySettings");

            migrationBuilder.DropTable(
                name: "CustomerAddresses");

            migrationBuilder.DropTable(
                name: "CustomerContacts");

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
                name: "InventorySettings");

            migrationBuilder.DropTable(
                name: "InvoicePaymentSchedules");

            migrationBuilder.DropTable(
                name: "NegotiableInstruments");

            migrationBuilder.DropTable(
                name: "NumberSequences");

            migrationBuilder.DropTable(
                name: "PaymentReceiptLines");

            migrationBuilder.DropTable(
                name: "ProductBarcodes");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "ProductSerialNumbers");

            migrationBuilder.DropTable(
                name: "ProductUnitConversions");

            migrationBuilder.DropTable(
                name: "PurchasePriceListItems");

            migrationBuilder.DropTable(
                name: "SalesPriceListItems");

            migrationBuilder.DropTable(
                name: "ScaleProductSettings");

            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "StockReservations");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "DispatchNotes");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");

            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "PaymentReceipts");

            migrationBuilder.DropTable(
                name: "UnitsOfMeasure");

            migrationBuilder.DropTable(
                name: "SalesPriceLists");

            migrationBuilder.DropTable(
                name: "InventoryCountLines");

            migrationBuilder.DropTable(
                name: "InvoiceLines");

            migrationBuilder.DropTable(
                name: "StockSlipLines");

            migrationBuilder.DropTable(
                name: "StockTransferLines");

            migrationBuilder.DropTable(
                name: "QuoteLines");

            migrationBuilder.DropTable(
                name: "BusinessOrders");

            migrationBuilder.DropTable(
                name: "CurrentAccountTransactions");

            migrationBuilder.DropTable(
                name: "FinancialAccounts");

            migrationBuilder.DropTable(
                name: "InventoryCounts");

            migrationBuilder.DropTable(
                name: "StockSlips");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "StockTransfers");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "ProductColors");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "BusinessProjects");

            migrationBuilder.DropTable(
                name: "CostCenters");

            migrationBuilder.DropTable(
                name: "PurchasePriceLists");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "TaxRates");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Branches");
        }
    }
}
