IF DB_ID(N'SahinSoftDb') IS NULL
BEGIN
    CREATE DATABASE [SahinSoftDb];
END;
GO

USE [SahinSoftDb];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [AuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Action] nvarchar(50) NOT NULL,
    [EntityName] nvarchar(100) NOT NULL,
    [EntityId] nvarchar(100) NULL,
    [OldValuesJson] nvarchar(max) NULL,
    [NewValuesJson] nvarchar(max) NULL,
    [IpAddress] nvarchar(64) NULL,
    [UserAgent] nvarchar(500) NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);

CREATE TABLE [Branches] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(30) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Address] nvarchar(max) NULL,
    [Phone] nvarchar(30) NULL,
    [IsHeadOffice] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Branches] PRIMARY KEY ([Id])
);

CREATE TABLE [BusinessProjects] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(30) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [StartDateUtc] datetime2 NULL,
    [EndDateUtc] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_BusinessProjects] PRIMARY KEY ([Id])
);

CREATE TABLE [CompanySettings] (
    [Id] int NOT NULL IDENTITY,
    [CompanyName] nvarchar(200) NOT NULL,
    [TaxOffice] nvarchar(100) NULL,
    [TaxNumber] nvarchar(11) NULL,
    [Address] nvarchar(max) NULL,
    [Phone] nvarchar(30) NULL,
    [Email] nvarchar(200) NULL,
    [Website] nvarchar(250) NULL,
    [BankName] nvarchar(150) NULL,
    [Iban] nvarchar(34) NULL,
    [LogoPath] nvarchar(500) NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_CompanySettings] PRIMARY KEY ([Id])
);

CREATE TABLE [CostCenters] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(30) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [IsActive] bit NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_CostCenters] PRIMARY KEY ([Id])
);

CREATE TABLE [Currencies] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(3) NOT NULL,
    [Name] nvarchar(80) NOT NULL,
    [Symbol] nvarchar(10) NOT NULL,
    [IsBaseCurrency] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Currencies] PRIMARY KEY ([Id])
);

CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(30) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [TaxOffice] nvarchar(100) NULL,
    [TaxNumber] nvarchar(11) NULL,
    [IdentityNumber] nvarchar(11) NULL,
    [Phone] nvarchar(30) NULL,
    [Email] nvarchar(200) NULL,
    [Address] nvarchar(max) NULL,
    [City] nvarchar(100) NULL,
    [District] nvarchar(100) NULL,
    [Notes] nvarchar(max) NULL,
    [IsCustomer] bit NOT NULL,
    [IsSupplier] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);

CREATE TABLE [ExpenseCategories] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(30) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [IsActive] bit NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ExpenseCategories] PRIMARY KEY ([Id])
);

CREATE TABLE [ExternalRecordMappings] (
    [Id] int NOT NULL IDENTITY,
    [SourceSystem] nvarchar(50) NOT NULL,
    [EntityType] nvarchar(100) NOT NULL,
    [ExternalId] nvarchar(200) NOT NULL,
    [InternalId] nvarchar(100) NOT NULL,
    [ExternalCode] nvarchar(100) NULL,
    [LastSynchronizedAtUtc] datetime2 NOT NULL,
    [ContentHash] nvarchar(128) NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ExternalRecordMappings] PRIMARY KEY ([Id])
);

CREATE TABLE [FinancialAccounts] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(30) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [AccountType] int NOT NULL,
    [CurrencyCode] nvarchar(3) NOT NULL,
    [BankName] nvarchar(150) NULL,
    [BranchName] nvarchar(150) NULL,
    [Iban] nvarchar(34) NULL,
    [IsActive] bit NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_FinancialAccounts] PRIMARY KEY ([Id])
);

CREATE TABLE [IntegrationOutboxMessages] (
    [Id] int NOT NULL IDENTITY,
    [EventType] nvarchar(200) NOT NULL,
    [PayloadJson] nvarchar(max) NOT NULL,
    [OccurredAtUtc] datetime2 NOT NULL,
    [ProcessedAtUtc] datetime2 NULL,
    [RetryCount] int NOT NULL,
    [LastError] nvarchar(max) NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_IntegrationOutboxMessages] PRIMARY KEY ([Id])
);

CREATE TABLE [InventorySettings] (
    [Id] int NOT NULL IDENTITY,
    [RequireBarcode] bit NOT NULL,
    [AutoGenerateBarcode] bit NOT NULL,
    [DefaultBarcodeType] nvarchar(20) NOT NULL,
    [DefaultScalePrefix] nvarchar(2) NOT NULL,
    [EnforceStockLevel] bit NOT NULL,
    [AllowNegativeStock] bit NOT NULL,
    [AllowSaleWhenOutOfStock] bit NOT NULL,
    [EnableMinimumStockWarning] bit NOT NULL,
    [RequireTransferApproval] bit NOT NULL,
    [TrackStockByVariant] bit NOT NULL,
    [RequireProductVariant] bit NOT NULL,
    [AllowSaleBelowCost] bit NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_InventorySettings] PRIMARY KEY ([Id])
);

CREATE TABLE [NumberSequences] (
    [Id] int NOT NULL IDENTITY,
    [Key] nvarchar(30) NOT NULL,
    [Prefix] nvarchar(20) NOT NULL,
    [NextNumber] bigint NOT NULL,
    [Padding] int NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_NumberSequences] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_NumberSequences_NextNumber] CHECK ([NextNumber] > 0),
    CONSTRAINT [CK_NumberSequences_Padding] CHECK ([Padding] BETWEEN 1 AND 12)
);

CREATE TABLE [ProductCategories] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(30) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [WebsitePath] nvarchar(250) NULL,
    [IsActive] bit NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ProductCategories] PRIMARY KEY ([Id])
);

CREATE TABLE [ProductColors] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(30) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [HexCode] nvarchar(7) NULL,
    [IsActive] bit NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ProductColors] PRIMARY KEY ([Id])
);

CREATE TABLE [TaxRates] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(20) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Rate] decimal(5,2) NOT NULL,
    [IsExempt] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_TaxRates] PRIMARY KEY ([Id])
);

CREATE TABLE [UnitsOfMeasure] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(20) NOT NULL,
    [Name] nvarchar(80) NOT NULL,
    [DecimalPlaces] int NOT NULL,
    [IsActive] bit NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_UnitsOfMeasure] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_UnitsOfMeasure_DecimalPlaces] CHECK ([DecimalPlaces] BETWEEN 0 AND 6)
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Warehouses] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(30) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [IsDefault] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [BranchId] int NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Warehouses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Warehouses_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ExchangeRates] (
    [Id] int NOT NULL IDENTITY,
    [RateDateUtc] datetime2 NOT NULL,
    [BuyingRate] decimal(18,6) NOT NULL,
    [SellingRate] decimal(18,6) NOT NULL,
    [CurrencyId] int NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ExchangeRates] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ExchangeRates_Positive] CHECK ([BuyingRate] > 0 AND [SellingRate] > 0),
    CONSTRAINT [FK_ExchangeRates_Currencies_CurrencyId] FOREIGN KEY ([CurrencyId]) REFERENCES [Currencies] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CustomerAddresses] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(100) NOT NULL,
    [AddressType] nvarchar(30) NOT NULL,
    [AddressLine] nvarchar(500) NOT NULL,
    [District] nvarchar(100) NULL,
    [City] nvarchar(100) NULL,
    [PostalCode] nvarchar(20) NULL,
    [CountryCode] nvarchar(2) NOT NULL,
    [IsDefault] bit NOT NULL,
    [CustomerId] int NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_CustomerAddresses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerAddresses_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CustomerContacts] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(150) NOT NULL,
    [Title] nvarchar(100) NULL,
    [Phone] nvarchar(30) NULL,
    [Email] nvarchar(200) NULL,
    [IsPrimary] bit NOT NULL,
    [CustomerId] int NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_CustomerContacts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerContacts_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PaymentReceipts] (
    [Id] int NOT NULL IDENTITY,
    [ReceiptType] int NOT NULL,
    [ReceiptNumber] nvarchar(30) NOT NULL,
    [DocumentNumber] nvarchar(50) NULL,
    [ReceiptDateUtc] datetime2 NOT NULL,
    [CurrencyCode] nvarchar(3) NOT NULL,
    [ExchangeRate] decimal(18,6) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Description] nvarchar(max) NULL,
    [CreatedByUserId] nvarchar(450) NOT NULL,
    [IsApproved] bit NOT NULL,
    [CustomerId] int NOT NULL,
    [CostCenterId] int NULL,
    [BusinessProjectId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_PaymentReceipts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PaymentReceipts_BusinessProjects_BusinessProjectId] FOREIGN KEY ([BusinessProjectId]) REFERENCES [BusinessProjects] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_PaymentReceipts_CostCenters_CostCenterId] FOREIGN KEY ([CostCenterId]) REFERENCES [CostCenters] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_PaymentReceipts_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PurchasePriceLists] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(30) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [CurrencyCode] nvarchar(3) NOT NULL,
    [ValidFromUtc] datetime2 NOT NULL,
    [ValidUntilUtc] datetime2 NULL,
    [PricesIncludeTax] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CustomerId] int NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_PurchasePriceLists] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PurchasePriceLists_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Quotes] (
    [Id] int NOT NULL IDENTITY,
    [QuoteNumber] nvarchar(30) NOT NULL,
    [QuoteDateUtc] datetime2 NOT NULL,
    [ValidUntilUtc] datetime2 NULL,
    [Status] int NOT NULL,
    [CurrencyCode] nvarchar(3) NOT NULL,
    [ExchangeRate] decimal(18,6) NOT NULL,
    [Subtotal] decimal(18,2) NOT NULL,
    [DiscountTotal] decimal(18,2) NOT NULL,
    [TaxTotal] decimal(18,2) NOT NULL,
    [GrandTotal] decimal(18,2) NOT NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedByUserId] nvarchar(450) NOT NULL,
    [CustomerId] int NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Quotes] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Quotes_Totals] CHECK ([Subtotal] >= 0 AND [DiscountTotal] >= 0 AND [TaxTotal] >= 0 AND [GrandTotal] >= 0),
    CONSTRAINT [FK_Quotes_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SalesPriceLists] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(30) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [CurrencyCode] nvarchar(3) NOT NULL,
    [ValidFromUtc] datetime2 NOT NULL,
    [ValidUntilUtc] datetime2 NULL,
    [PricesIncludeTax] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CustomerId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_SalesPriceLists] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SalesPriceLists_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [NegotiableInstruments] (
    [Id] int NOT NULL IDENTITY,
    [InstrumentType] int NOT NULL,
    [Direction] int NOT NULL,
    [Status] int NOT NULL,
    [InstrumentNumber] nvarchar(80) NOT NULL,
    [IssueDateUtc] datetime2 NOT NULL,
    [DueDateUtc] datetime2 NOT NULL,
    [CurrencyCode] nvarchar(3) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [BankName] nvarchar(150) NULL,
    [BranchName] nvarchar(150) NULL,
    [AccountNumber] nvarchar(80) NULL,
    [DrawerName] nvarchar(200) NULL,
    [Description] nvarchar(max) NULL,
    [CustomerId] int NOT NULL,
    [FinancialAccountId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_NegotiableInstruments] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_NegotiableInstruments_Amount] CHECK ([Amount] > 0),
    CONSTRAINT [FK_NegotiableInstruments_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_NegotiableInstruments_FinancialAccounts_FinancialAccountId] FOREIGN KEY ([FinancialAccountId]) REFERENCES [FinancialAccounts] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [Expenses] (
    [Id] int NOT NULL IDENTITY,
    [DocumentNumber] nvarchar(50) NOT NULL,
    [ExpenseDateUtc] datetime2 NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [CurrencyCode] nvarchar(3) NOT NULL,
    [ExchangeRate] decimal(18,6) NOT NULL,
    [NetAmount] decimal(18,2) NOT NULL,
    [TaxAmount] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [ExpenseCategoryId] int NOT NULL,
    [CustomerId] int NULL,
    [TaxRateId] int NULL,
    [FinancialAccountId] int NULL,
    [CostCenterId] int NULL,
    [BusinessProjectId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Expenses] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Expenses_Amounts] CHECK ([NetAmount] >= 0 AND [TaxAmount] >= 0 AND [TotalAmount] = [NetAmount] + [TaxAmount]),
    CONSTRAINT [FK_Expenses_BusinessProjects_BusinessProjectId] FOREIGN KEY ([BusinessProjectId]) REFERENCES [BusinessProjects] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Expenses_CostCenters_CostCenterId] FOREIGN KEY ([CostCenterId]) REFERENCES [CostCenters] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Expenses_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Expenses_ExpenseCategories_ExpenseCategoryId] FOREIGN KEY ([ExpenseCategoryId]) REFERENCES [ExpenseCategories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Expenses_FinancialAccounts_FinancialAccountId] FOREIGN KEY ([FinancialAccountId]) REFERENCES [FinancialAccounts] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Expenses_TaxRates_TaxRateId] FOREIGN KEY ([TaxRateId]) REFERENCES [TaxRates] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [StockCode] nvarchar(40) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Brand] nvarchar(100) NULL,
    [Model] nvarchar(100) NULL,
    [Barcode] nvarchar(50) NULL,
    [Unit] nvarchar(20) NOT NULL,
    [ProductType] nvarchar(30) NOT NULL,
    [Description] nvarchar(max) NULL,
    [PurchasePrice] decimal(18,2) NOT NULL,
    [SalePrice] decimal(18,2) NOT NULL,
    [StockQuantity] decimal(18,3) NOT NULL,
    [MinimumStockQuantity] decimal(18,3) NOT NULL,
    [TrackStock] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [ImagePath] nvarchar(500) NULL,
    [WebsitePath] nvarchar(250) NULL,
    [CategoryId] int NOT NULL,
    [TaxRateId] int NOT NULL,
    [TrackSerialNumbers] bit NOT NULL,
    [TrackLots] bit NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Products_Prices] CHECK ([PurchasePrice] >= 0 AND [SalePrice] >= 0),
    CONSTRAINT [CK_Products_Quantities] CHECK ([MinimumStockQuantity] >= 0),
    CONSTRAINT [FK_Products_ProductCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ProductCategories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Products_TaxRates_TaxRateId] FOREIGN KEY ([TaxRateId]) REFERENCES [TaxRates] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [InventoryCounts] (
    [Id] int NOT NULL IDENTITY,
    [CountNumber] nvarchar(30) NOT NULL,
    [CountDateUtc] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [CreatedByUserId] nvarchar(450) NOT NULL,
    [ApprovedByUserId] nvarchar(450) NULL,
    [ApprovedAtUtc] datetime2 NULL,
    [WarehouseId] int NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_InventoryCounts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InventoryCounts_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [StockSlips] (
    [Id] int NOT NULL IDENTITY,
    [SlipNumber] nvarchar(30) NOT NULL,
    [SlipDateUtc] datetime2 NOT NULL,
    [SlipType] int NOT NULL,
    [Status] int NOT NULL,
    [Description] nvarchar(500) NULL,
    [CreatedByUserId] nvarchar(450) NOT NULL,
    [ApprovedByUserId] nvarchar(450) NULL,
    [ApprovedAtUtc] datetime2 NULL,
    [WarehouseId] int NOT NULL,
    [CostCenterId] int NULL,
    [BusinessProjectId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_StockSlips] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StockSlips_BusinessProjects_BusinessProjectId] FOREIGN KEY ([BusinessProjectId]) REFERENCES [BusinessProjects] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_StockSlips_CostCenters_CostCenterId] FOREIGN KEY ([CostCenterId]) REFERENCES [CostCenters] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_StockSlips_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [StockTransfers] (
    [Id] int NOT NULL IDENTITY,
    [TransferNumber] nvarchar(30) NOT NULL,
    [TransferDateUtc] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [Description] nvarchar(max) NULL,
    [CreatedByUserId] nvarchar(450) NOT NULL,
    [ApprovedByUserId] nvarchar(450) NULL,
    [ApprovedAtUtc] datetime2 NULL,
    [FromWarehouseId] int NOT NULL,
    [ToWarehouseId] int NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_StockTransfers] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_StockTransfers_DifferentWarehouses] CHECK ([FromWarehouseId] <> [ToWarehouseId]),
    CONSTRAINT [FK_StockTransfers_Warehouses_FromWarehouseId] FOREIGN KEY ([FromWarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StockTransfers_Warehouses_ToWarehouseId] FOREIGN KEY ([ToWarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [BusinessOrders] (
    [Id] int NOT NULL IDENTITY,
    [OrderType] int NOT NULL,
    [Status] int NOT NULL,
    [OrderNumber] nvarchar(30) NOT NULL,
    [OrderDateUtc] datetime2 NOT NULL,
    [RequestedDeliveryDateUtc] datetime2 NULL,
    [CurrencyCode] nvarchar(3) NOT NULL,
    [ExchangeRate] decimal(18,6) NOT NULL,
    [Subtotal] decimal(18,2) NOT NULL,
    [DiscountTotal] decimal(18,2) NOT NULL,
    [TaxTotal] decimal(18,2) NOT NULL,
    [GrandTotal] decimal(18,2) NOT NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedByUserId] nvarchar(450) NOT NULL,
    [CustomerId] int NOT NULL,
    [QuoteId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_BusinessOrders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BusinessOrders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_BusinessOrders_Quotes_QuoteId] FOREIGN KEY ([QuoteId]) REFERENCES [Quotes] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [Invoices] (
    [Id] int NOT NULL IDENTITY,
    [InvoiceType] int NOT NULL,
    [Status] int NOT NULL,
    [InvoiceNumber] nvarchar(30) NOT NULL,
    [InvoiceDateUtc] datetime2 NOT NULL,
    [DueDateUtc] datetime2 NULL,
    [CurrencyCode] nvarchar(3) NOT NULL,
    [ExchangeRate] decimal(18,6) NOT NULL,
    [Subtotal] decimal(18,2) NOT NULL,
    [DiscountTotal] decimal(18,2) NOT NULL,
    [TaxTotal] decimal(18,2) NOT NULL,
    [GrandTotal] decimal(18,2) NOT NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedByUserId] nvarchar(450) NOT NULL,
    [CustomerId] int NOT NULL,
    [WarehouseId] int NOT NULL,
    [QuoteId] int NULL,
    [PurchasePriceListId] int NULL,
    [CostCenterId] int NULL,
    [BusinessProjectId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Invoices_Totals] CHECK ([Subtotal] >= 0 AND [DiscountTotal] >= 0 AND [TaxTotal] >= 0 AND [GrandTotal] >= 0),
    CONSTRAINT [FK_Invoices_BusinessProjects_BusinessProjectId] FOREIGN KEY ([BusinessProjectId]) REFERENCES [BusinessProjects] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Invoices_CostCenters_CostCenterId] FOREIGN KEY ([CostCenterId]) REFERENCES [CostCenters] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Invoices_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Invoices_PurchasePriceLists_PurchasePriceListId] FOREIGN KEY ([PurchasePriceListId]) REFERENCES [PurchasePriceLists] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Invoices_Quotes_QuoteId] FOREIGN KEY ([QuoteId]) REFERENCES [Quotes] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Invoices_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ProductUnitConversions] (
    [Id] int NOT NULL IDENTITY,
    [MultiplierToBaseUnit] decimal(18,6) NOT NULL,
    [IsPurchaseUnit] bit NOT NULL,
    [IsSalesUnit] bit NOT NULL,
    [ProductId] int NOT NULL,
    [UnitOfMeasureId] int NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ProductUnitConversions] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ProductUnitConversions_Multiplier] CHECK ([MultiplierToBaseUnit] > 0),
    CONSTRAINT [FK_ProductUnitConversions_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductUnitConversions_UnitsOfMeasure_UnitOfMeasureId] FOREIGN KEY ([UnitOfMeasureId]) REFERENCES [UnitsOfMeasure] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ProductVariants] (
    [Id] int NOT NULL IDENTITY,
    [VariantCode] nvarchar(50) NOT NULL,
    [VariantName] nvarchar(150) NOT NULL,
    [AdditionalPurchasePrice] decimal(18,2) NOT NULL,
    [AdditionalSalePrice] decimal(18,2) NOT NULL,
    [IsActive] bit NOT NULL,
    [ProductId] int NOT NULL,
    [ColorId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ProductVariants] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductVariants_ProductColors_ColorId] FOREIGN KEY ([ColorId]) REFERENCES [ProductColors] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_ProductVariants_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PurchasePriceListItems] (
    [Id] int NOT NULL IDENTITY,
    [MinimumQuantity] decimal(18,3) NOT NULL,
    [UnitPrice] decimal(18,4) NOT NULL,
    [SupplierProductCode] nvarchar(80) NULL,
    [Notes] nvarchar(max) NULL,
    [PurchasePriceListId] int NOT NULL,
    [ProductId] int NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_PurchasePriceListItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PurchasePriceListItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PurchasePriceListItems_PurchasePriceLists_PurchasePriceListId] FOREIGN KEY ([PurchasePriceListId]) REFERENCES [PurchasePriceLists] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [QuoteLines] (
    [Id] int NOT NULL IDENTITY,
    [LineNumber] int NOT NULL,
    [ProductCodeSnapshot] nvarchar(40) NOT NULL,
    [ProductNameSnapshot] nvarchar(200) NOT NULL,
    [UnitSnapshot] nvarchar(20) NOT NULL,
    [Quantity] decimal(18,3) NOT NULL,
    [UnitPrice] decimal(18,4) NOT NULL,
    [DiscountRate] decimal(5,2) NOT NULL,
    [DiscountAmount] decimal(18,2) NOT NULL,
    [TaxRate] decimal(5,2) NOT NULL,
    [TaxAmount] decimal(18,2) NOT NULL,
    [LineTotal] decimal(18,2) NOT NULL,
    [Description] nvarchar(max) NULL,
    [QuoteId] int NOT NULL,
    [ProductId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_QuoteLines] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_QuoteLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_QuoteLines_Quotes_QuoteId] FOREIGN KEY ([QuoteId]) REFERENCES [Quotes] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ScaleProductSettings] (
    [Id] int NOT NULL IDENTITY,
    [MeasurementType] int NOT NULL,
    [Prefix] nvarchar(2) NOT NULL,
    [PluCode] nvarchar(5) NOT NULL,
    [BarcodeContainsPrice] bit NOT NULL,
    [ProductId] int NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ScaleProductSettings] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ScaleProductSettings_PluLength] CHECK (LEN([PluCode]) = 5 AND [PluCode] NOT LIKE '%[^0-9]%'),
    CONSTRAINT [CK_ScaleProductSettings_Prefix] CHECK ([Prefix] IN (N'27', N'28', N'29')),
    CONSTRAINT [FK_ScaleProductSettings_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CurrentAccountTransactions] (
    [Id] int NOT NULL IDENTITY,
    [TransactionDateUtc] datetime2 NOT NULL,
    [TransactionType] int NOT NULL,
    [DocumentNumber] nvarchar(50) NOT NULL,
    [CurrencyCode] nvarchar(3) NOT NULL,
    [ExchangeRate] decimal(18,6) NOT NULL,
    [Debit] decimal(18,2) NOT NULL,
    [Credit] decimal(18,2) NOT NULL,
    [DueDateUtc] datetime2 NULL,
    [Description] nvarchar(max) NULL,
    [CustomerId] int NOT NULL,
    [QuoteId] int NULL,
    [InvoiceId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_CurrentAccountTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_CurrentAccountTransactions_DebitCredit] CHECK (([Debit] > 0 AND [Credit] = 0) OR ([Credit] > 0 AND [Debit] = 0)),
    CONSTRAINT [FK_CurrentAccountTransactions_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CurrentAccountTransactions_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_CurrentAccountTransactions_Quotes_QuoteId] FOREIGN KEY ([QuoteId]) REFERENCES [Quotes] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [DispatchNotes] (
    [Id] int NOT NULL IDENTITY,
    [DispatchType] int NOT NULL,
    [Status] int NOT NULL,
    [DispatchNumber] nvarchar(30) NOT NULL,
    [DispatchDateUtc] datetime2 NOT NULL,
    [VehiclePlate] nvarchar(20) NULL,
    [CarrierName] nvarchar(150) NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedByUserId] nvarchar(450) NOT NULL,
    [CustomerId] int NOT NULL,
    [WarehouseId] int NOT NULL,
    [BusinessOrderId] int NULL,
    [InvoiceId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_DispatchNotes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DispatchNotes_BusinessOrders_BusinessOrderId] FOREIGN KEY ([BusinessOrderId]) REFERENCES [BusinessOrders] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_DispatchNotes_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DispatchNotes_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_DispatchNotes_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [InvoicePaymentSchedules] (
    [Id] int NOT NULL IDENTITY,
    [InstallmentNumber] int NOT NULL,
    [DueDateUtc] datetime2 NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaidAmount] decimal(18,2) NOT NULL,
    [InvoiceId] int NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_InvoicePaymentSchedules] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_InvoicePaymentSchedules_Amounts] CHECK ([Amount] > 0 AND [PaidAmount] >= 0 AND [PaidAmount] <= [Amount]),
    CONSTRAINT [FK_InvoicePaymentSchedules_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [BusinessOrderLines] (
    [Id] int NOT NULL IDENTITY,
    [LineNumber] int NOT NULL,
    [ProductCodeSnapshot] nvarchar(40) NOT NULL,
    [ProductNameSnapshot] nvarchar(200) NOT NULL,
    [UnitSnapshot] nvarchar(20) NOT NULL,
    [Quantity] decimal(18,3) NOT NULL,
    [FulfilledQuantity] decimal(18,3) NOT NULL,
    [UnitPrice] decimal(18,4) NOT NULL,
    [DiscountRate] decimal(5,2) NOT NULL,
    [TaxRate] decimal(5,2) NOT NULL,
    [LineTotal] decimal(18,2) NOT NULL,
    [BusinessOrderId] int NOT NULL,
    [ProductId] int NULL,
    [ProductVariantId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_BusinessOrderLines] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_BusinessOrderLines_Quantity] CHECK ([Quantity] > 0 AND [FulfilledQuantity] >= 0 AND [FulfilledQuantity] <= [Quantity]),
    CONSTRAINT [FK_BusinessOrderLines_BusinessOrders_BusinessOrderId] FOREIGN KEY ([BusinessOrderId]) REFERENCES [BusinessOrders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BusinessOrderLines_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_BusinessOrderLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [InventoryCountLines] (
    [Id] int NOT NULL IDENTITY,
    [SystemQuantity] decimal(18,3) NOT NULL,
    [CountedQuantity] decimal(18,3) NOT NULL,
    [InventoryCountId] int NOT NULL,
    [ProductId] int NOT NULL,
    [ProductVariantId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_InventoryCountLines] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_InventoryCountLines_Quantities] CHECK ([SystemQuantity] >= 0 AND [CountedQuantity] >= 0),
    CONSTRAINT [FK_InventoryCountLines_InventoryCounts_InventoryCountId] FOREIGN KEY ([InventoryCountId]) REFERENCES [InventoryCounts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_InventoryCountLines_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_InventoryCountLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [InvoiceLines] (
    [Id] int NOT NULL IDENTITY,
    [LineNumber] int NOT NULL,
    [ProductCodeSnapshot] nvarchar(40) NOT NULL,
    [ProductNameSnapshot] nvarchar(200) NOT NULL,
    [UnitSnapshot] nvarchar(20) NOT NULL,
    [Quantity] decimal(18,3) NOT NULL,
    [UnitPrice] decimal(18,4) NOT NULL,
    [DiscountRate] decimal(5,2) NOT NULL,
    [DiscountAmount] decimal(18,2) NOT NULL,
    [TaxRate] decimal(5,2) NOT NULL,
    [TaxAmount] decimal(18,2) NOT NULL,
    [LineTotal] decimal(18,2) NOT NULL,
    [Description] nvarchar(max) NULL,
    [InvoiceId] int NOT NULL,
    [ProductId] int NULL,
    [ProductVariantId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_InvoiceLines] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InvoiceLines_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_InvoiceLines_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_InvoiceLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [ProductBarcodes] (
    [Id] int NOT NULL IDENTITY,
    [Barcode] nvarchar(50) NOT NULL,
    [BarcodeType] nvarchar(30) NOT NULL,
    [UnitMultiplier] decimal(18,3) NOT NULL,
    [IsPrimary] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [ProductId] int NOT NULL,
    [ProductVariantId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ProductBarcodes] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ProductBarcodes_Length] CHECK (([BarcodeType] = N'EAN13' AND LEN([Barcode]) = 13) OR ([BarcodeType] = N'EAN8' AND LEN([Barcode]) = 8) OR ([BarcodeType] = N'SCALE' AND LEN([Barcode]) = 7) OR [BarcodeType] = N'OTHER'),
    CONSTRAINT [CK_ProductBarcodes_Numeric] CHECK ([BarcodeType] = N'OTHER' OR [Barcode] NOT LIKE '%[^0-9]%'),
    CONSTRAINT [FK_ProductBarcodes_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]),
    CONSTRAINT [FK_ProductBarcodes_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ProductImages] (
    [Id] int NOT NULL IDENTITY,
    [FilePath] nvarchar(500) NOT NULL,
    [AltText] nvarchar(200) NULL,
    [DisplayOrder] int NOT NULL,
    [IsPrimary] bit NOT NULL,
    [ProductId] int NOT NULL,
    [ProductVariantId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ProductImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductImages_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]),
    CONSTRAINT [FK_ProductImages_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ProductSerialNumbers] (
    [Id] int NOT NULL IDENTITY,
    [SerialNumber] nvarchar(100) NOT NULL,
    [LotNumber] nvarchar(100) NULL,
    [ExpirationDateUtc] datetime2 NULL,
    [IsInStock] bit NOT NULL,
    [ProductId] int NOT NULL,
    [ProductVariantId] int NULL,
    [WarehouseId] int NOT NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_ProductSerialNumbers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductSerialNumbers_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_ProductSerialNumbers_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProductSerialNumbers_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SalesPriceListItems] (
    [Id] int NOT NULL IDENTITY,
    [MinimumQuantity] decimal(18,3) NOT NULL,
    [UnitPrice] decimal(18,4) NOT NULL,
    [SalesPriceListId] int NOT NULL,
    [ProductId] int NOT NULL,
    [ProductVariantId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_SalesPriceListItems] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_SalesPriceListItems_Price] CHECK ([UnitPrice] >= 0),
    CONSTRAINT [CK_SalesPriceListItems_Quantity] CHECK ([MinimumQuantity] > 0),
    CONSTRAINT [FK_SalesPriceListItems_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_SalesPriceListItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SalesPriceListItems_SalesPriceLists_SalesPriceListId] FOREIGN KEY ([SalesPriceListId]) REFERENCES [SalesPriceLists] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [StockSlipLines] (
    [Id] int NOT NULL IDENTITY,
    [LineNumber] int NOT NULL,
    [Quantity] decimal(18,3) NOT NULL,
    [UnitCost] decimal(18,4) NOT NULL,
    [Description] nvarchar(500) NULL,
    [StockSlipId] int NOT NULL,
    [ProductId] int NOT NULL,
    [ProductVariantId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_StockSlipLines] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_StockSlipLines_Values] CHECK ([Quantity] > 0 AND [UnitCost] >= 0),
    CONSTRAINT [FK_StockSlipLines_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_StockSlipLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StockSlipLines_StockSlips_StockSlipId] FOREIGN KEY ([StockSlipId]) REFERENCES [StockSlips] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [StockTransferLines] (
    [Id] int NOT NULL IDENTITY,
    [LineNumber] int NOT NULL,
    [Quantity] decimal(18,3) NOT NULL,
    [Description] nvarchar(max) NULL,
    [StockTransferId] int NOT NULL,
    [ProductId] int NOT NULL,
    [ProductVariantId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_StockTransferLines] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_StockTransferLines_PositiveQuantity] CHECK ([Quantity] > 0),
    CONSTRAINT [FK_StockTransferLines_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_StockTransferLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StockTransferLines_StockTransfers_StockTransferId] FOREIGN KEY ([StockTransferId]) REFERENCES [StockTransfers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [StockReservations] (
    [Id] int NOT NULL IDENTITY,
    [Quantity] decimal(18,3) NOT NULL,
    [ReservedUntilUtc] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [ProductId] int NOT NULL,
    [ProductVariantId] int NULL,
    [WarehouseId] int NOT NULL,
    [QuoteLineId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_StockReservations] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_StockReservations_Quantity] CHECK ([Quantity] > 0),
    CONSTRAINT [FK_StockReservations_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_StockReservations_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StockReservations_QuoteLines_QuoteLineId] FOREIGN KEY ([QuoteLineId]) REFERENCES [QuoteLines] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_StockReservations_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [FinancialTransactions] (
    [Id] int NOT NULL IDENTITY,
    [TransactionDateUtc] datetime2 NOT NULL,
    [TransactionType] int NOT NULL,
    [DocumentNumber] nvarchar(50) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [ExchangeRate] decimal(18,6) NOT NULL,
    [Description] nvarchar(max) NULL,
    [FinancialAccountId] int NOT NULL,
    [CustomerId] int NULL,
    [CurrentAccountTransactionId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_FinancialTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_FinancialTransactions_Amount] CHECK ([Amount] > 0),
    CONSTRAINT [FK_FinancialTransactions_CurrentAccountTransactions_CurrentAccountTransactionId] FOREIGN KEY ([CurrentAccountTransactionId]) REFERENCES [CurrentAccountTransactions] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_FinancialTransactions_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_FinancialTransactions_FinancialAccounts_FinancialAccountId] FOREIGN KEY ([FinancialAccountId]) REFERENCES [FinancialAccounts] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [DispatchNoteLines] (
    [Id] int NOT NULL IDENTITY,
    [LineNumber] int NOT NULL,
    [Quantity] decimal(18,3) NOT NULL,
    [DispatchNoteId] int NOT NULL,
    [ProductId] int NOT NULL,
    [ProductVariantId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_DispatchNoteLines] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_DispatchNoteLines_Quantity] CHECK ([Quantity] > 0),
    CONSTRAINT [FK_DispatchNoteLines_DispatchNotes_DispatchNoteId] FOREIGN KEY ([DispatchNoteId]) REFERENCES [DispatchNotes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DispatchNoteLines_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_DispatchNoteLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [StockMovements] (
    [Id] int NOT NULL IDENTITY,
    [MovementDateUtc] datetime2 NOT NULL,
    [MovementType] int NOT NULL,
    [Quantity] decimal(18,3) NOT NULL,
    [UnitCost] decimal(18,4) NOT NULL,
    [DocumentNumber] nvarchar(50) NULL,
    [Description] nvarchar(max) NULL,
    [ProductId] int NOT NULL,
    [WarehouseId] int NOT NULL,
    [InvoiceLineId] int NULL,
    [CostCenterId] int NULL,
    [BusinessProjectId] int NULL,
    [ProductVariantId] int NULL,
    [StockTransferLineId] int NULL,
    [StockSlipLineId] int NULL,
    [InventoryCountLineId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_StockMovements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StockMovements_BusinessProjects_BusinessProjectId] FOREIGN KEY ([BusinessProjectId]) REFERENCES [BusinessProjects] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_StockMovements_CostCenters_CostCenterId] FOREIGN KEY ([CostCenterId]) REFERENCES [CostCenters] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_StockMovements_InventoryCountLines_InventoryCountLineId] FOREIGN KEY ([InventoryCountLineId]) REFERENCES [InventoryCountLines] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_StockMovements_InvoiceLines_InvoiceLineId] FOREIGN KEY ([InvoiceLineId]) REFERENCES [InvoiceLines] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_StockMovements_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_StockMovements_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StockMovements_StockSlipLines_StockSlipLineId] FOREIGN KEY ([StockSlipLineId]) REFERENCES [StockSlipLines] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_StockMovements_StockTransferLines_StockTransferLineId] FOREIGN KEY ([StockTransferLineId]) REFERENCES [StockTransferLines] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_StockMovements_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PaymentReceiptLines] (
    [Id] int NOT NULL IDENTITY,
    [LineNumber] int NOT NULL,
    [PaymentMethod] int NOT NULL,
    [ReferenceNumber] nvarchar(80) NULL,
    [DueDateUtc] datetime2 NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Description] nvarchar(max) NULL,
    [PaymentReceiptId] int NOT NULL,
    [FinancialAccountId] int NOT NULL,
    [CurrentAccountTransactionId] int NULL,
    [FinancialTransactionId] int NULL,
    [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_PaymentReceiptLines] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PaymentReceiptLines_CurrentAccountTransactions_CurrentAccountTransactionId] FOREIGN KEY ([CurrentAccountTransactionId]) REFERENCES [CurrentAccountTransactions] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_PaymentReceiptLines_FinancialAccounts_FinancialAccountId] FOREIGN KEY ([FinancialAccountId]) REFERENCES [FinancialAccounts] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PaymentReceiptLines_FinancialTransactions_FinancialTransactionId] FOREIGN KEY ([FinancialTransactionId]) REFERENCES [FinancialTransactions] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_PaymentReceiptLines_PaymentReceipts_PaymentReceiptId] FOREIGN KEY ([PaymentReceiptId]) REFERENCES [PaymentReceipts] ([Id]) ON DELETE CASCADE
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'Code', N'CreatedAtUtc', N'IsActive', N'IsHeadOffice', N'Name', N'Phone', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Branches]'))
    SET IDENTITY_INSERT [Branches] ON;
INSERT INTO [Branches] ([Id], [Address], [Code], [CreatedAtUtc], [IsActive], [IsHeadOffice], [Name], [Phone], [UpdatedAtUtc])
VALUES (1, NULL, N'MERKEZ', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), N'Merkez Şube', NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'Code', N'CreatedAtUtc', N'IsActive', N'IsHeadOffice', N'Name', N'Phone', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Branches]'))
    SET IDENTITY_INSERT [Branches] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'BankName', N'CompanyName', N'CreatedAtUtc', N'Email', N'Iban', N'LogoPath', N'Phone', N'TaxNumber', N'TaxOffice', N'UpdatedAtUtc', N'Website') AND [object_id] = OBJECT_ID(N'[CompanySettings]'))
    SET IDENTITY_INSERT [CompanySettings] ON;
INSERT INTO [CompanySettings] ([Id], [Address], [BankName], [CompanyName], [CreatedAtUtc], [Email], [Iban], [LogoPath], [Phone], [TaxNumber], [TaxOffice], [UpdatedAtUtc], [Website])
VALUES (1, NULL, NULL, N'ŞahinSoft', '2026-07-27T00:00:00.0000000Z', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'BankName', N'CompanyName', N'CreatedAtUtc', N'Email', N'Iban', N'LogoPath', N'Phone', N'TaxNumber', N'TaxOffice', N'UpdatedAtUtc', N'Website') AND [object_id] = OBJECT_ID(N'[CompanySettings]'))
    SET IDENTITY_INSERT [CompanySettings] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'IsActive', N'IsBaseCurrency', N'Name', N'Symbol', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Currencies]'))
    SET IDENTITY_INSERT [Currencies] ON;
INSERT INTO [Currencies] ([Id], [Code], [CreatedAtUtc], [IsActive], [IsBaseCurrency], [Name], [Symbol], [UpdatedAtUtc])
VALUES (1, N'TRY', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), N'Türk Lirası', N'₺', NULL),
(2, N'USD', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(0 AS bit), N'Amerikan Doları', N'$', NULL),
(3, N'EUR', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(0 AS bit), N'Euro', N'€', NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'IsActive', N'IsBaseCurrency', N'Name', N'Symbol', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Currencies]'))
    SET IDENTITY_INSERT [Currencies] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AllowNegativeStock', N'AllowSaleBelowCost', N'AllowSaleWhenOutOfStock', N'AutoGenerateBarcode', N'CreatedAtUtc', N'DefaultBarcodeType', N'DefaultScalePrefix', N'EnableMinimumStockWarning', N'EnforceStockLevel', N'RequireBarcode', N'RequireProductVariant', N'RequireTransferApproval', N'TrackStockByVariant', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[InventorySettings]'))
    SET IDENTITY_INSERT [InventorySettings] ON;
INSERT INTO [InventorySettings] ([Id], [AllowNegativeStock], [AllowSaleBelowCost], [AllowSaleWhenOutOfStock], [AutoGenerateBarcode], [CreatedAtUtc], [DefaultBarcodeType], [DefaultScalePrefix], [EnableMinimumStockWarning], [EnforceStockLevel], [RequireBarcode], [RequireProductVariant], [RequireTransferApproval], [TrackStockByVariant], [UpdatedAtUtc])
VALUES (1, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), '2026-07-27T00:00:00.0000000Z', N'EAN13', N'27', CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AllowNegativeStock', N'AllowSaleBelowCost', N'AllowSaleWhenOutOfStock', N'AutoGenerateBarcode', N'CreatedAtUtc', N'DefaultBarcodeType', N'DefaultScalePrefix', N'EnableMinimumStockWarning', N'EnforceStockLevel', N'RequireBarcode', N'RequireProductVariant', N'RequireTransferApproval', N'TrackStockByVariant', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[InventorySettings]'))
    SET IDENTITY_INSERT [InventorySettings] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
    SET IDENTITY_INSERT [NumberSequences] ON;
INSERT INTO [NumberSequences] ([Id], [CreatedAtUtc], [Key], [NextNumber], [Padding], [Prefix], [UpdatedAtUtc])
VALUES (1, '2026-07-27T00:00:00.0000000Z', N'STOCK', CAST(1 AS bigint), 3, N'SHN.', NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
    SET IDENTITY_INSERT [NumberSequences] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'IsActive', N'Name', N'UpdatedAtUtc', N'WebsitePath') AND [object_id] = OBJECT_ID(N'[ProductCategories]'))
    SET IDENTITY_INSERT [ProductCategories] ON;
INSERT INTO [ProductCategories] ([Id], [Code], [CreatedAtUtc], [IsActive], [Name], [UpdatedAtUtc], [WebsitePath])
VALUES (1, N'YAZARKASA', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), N'Yazar Kasa POS', NULL, N'yazarkasa-pos.html'),
(2, N'TERAZI', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), N'Teraziler', NULL, N'teraziler.html'),
(3, N'BARKOD', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), N'Barkod Okuyucular', NULL, N'barkod-okuyucular.html'),
(4, N'YAZICI', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), N'Yazıcılar', NULL, N'yazicilar.html'),
(5, N'ELTERM', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), N'El Terminalleri', NULL, N'el-terminali.html'),
(6, N'POSPC', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), N'Dokunmatik POS', NULL, N'dokunmatik-pos.html'),
(7, N'YAZILIM', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), N'Yazılım ve Entegrasyon', NULL, N'kurumsal-yazilim.html'),
(8, N'POSEKIP', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), N'POS Çevre Birimleri', NULL, N'index.html');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'IsActive', N'Name', N'UpdatedAtUtc', N'WebsitePath') AND [object_id] = OBJECT_ID(N'[ProductCategories]'))
    SET IDENTITY_INSERT [ProductCategories] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'IsActive', N'IsExempt', N'Name', N'Rate', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[TaxRates]'))
    SET IDENTITY_INSERT [TaxRates] ON;
INSERT INTO [TaxRates] ([Id], [Code], [CreatedAtUtc], [IsActive], [IsExempt], [Name], [Rate], [UpdatedAtUtc])
VALUES (1, N'KDV10', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(0 AS bit), N'KDV %10', 10.0, NULL),
(2, N'KDV20', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(0 AS bit), N'KDV %20', 20.0, NULL),
(3, N'KDV0', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), N'KDV %0', 0.0, NULL),
(4, N'KDV1', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(0 AS bit), N'KDV %1', 1.0, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'IsActive', N'IsExempt', N'Name', N'Rate', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[TaxRates]'))
    SET IDENTITY_INSERT [TaxRates] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'DecimalPlaces', N'IsActive', N'Name', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[UnitsOfMeasure]'))
    SET IDENTITY_INSERT [UnitsOfMeasure] ON;
INSERT INTO [UnitsOfMeasure] ([Id], [Code], [CreatedAtUtc], [DecimalPlaces], [IsActive], [Name], [UpdatedAtUtc])
VALUES (1, N'ADET', '2026-07-27T00:00:00.0000000Z', 0, CAST(1 AS bit), N'Adet', NULL),
(2, N'KG', '2026-07-27T00:00:00.0000000Z', 3, CAST(1 AS bit), N'Kilogram', NULL),
(3, N'PAKET', '2026-07-27T00:00:00.0000000Z', 0, CAST(1 AS bit), N'Paket', NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'DecimalPlaces', N'IsActive', N'Name', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[UnitsOfMeasure]'))
    SET IDENTITY_INSERT [UnitsOfMeasure] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Barcode', N'Brand', N'CategoryId', N'CreatedAtUtc', N'Description', N'ImagePath', N'IsActive', N'MinimumStockQuantity', N'Model', N'Name', N'ProductType', N'PurchasePrice', N'SalePrice', N'StockCode', N'StockQuantity', N'TaxRateId', N'TrackLots', N'TrackSerialNumbers', N'TrackStock', N'Unit', N'UpdatedAtUtc', N'WebsitePath') AND [object_id] = OBJECT_ID(N'[Products]'))
    SET IDENTITY_INSERT [Products] ON;
INSERT INTO [Products] ([Id], [Barcode], [Brand], [CategoryId], [CreatedAtUtc], [Description], [ImagePath], [IsActive], [MinimumStockQuantity], [Model], [Name], [ProductType], [PurchasePrice], [SalePrice], [StockCode], [StockQuantity], [TaxRateId], [TrackLots], [TrackSerialNumbers], [TrackStock], [Unit], [UpdatedAtUtc], [WebsitePath])
VALUES (1, N'2000000000015', N'Ingenico', 1, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'IDE280', N'Ingenico IDE280', N'Donanım', 0.0, 0.0, N'YK-0001', 0.0, 1, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'yazarkasa-pos.html'),
(2, N'2000000000022', N'Ingenico', 1, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'Move 5000F', N'Ingenico Move 5000F', N'Donanım', 0.0, 0.0, N'YK-0002', 0.0, 1, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'yazarkasa-pos.html'),
(3, N'2000000000039', N'PayGo', 1, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'SP630PRO ECR', N'PAYGO SP630PRO ECR', N'Donanım', 0.0, 0.0, N'YK-0003', 0.0, 1, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'yazarkasa-pos.html'),
(4, N'2000000000046', N'Profilo', 1, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'S900', N'Profilo S900', N'Donanım', 0.0, 0.0, N'YK-0004', 0.0, 1, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'yazarkasa-pos.html'),
(5, N'2000000000053', N'inPOS', 1, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'm530', N'inPOS m530 Mobil POS', N'Donanım', 0.0, 0.0, N'YK-0005', 0.0, 1, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'yazarkasa-pos.html'),
(6, N'2000000000060', N'CAS', 2, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'CL3000', N'CAS CL3000 Market Terazisi', N'Donanım', 0.0, 0.0, N'TR-0001', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'teraziler.html'),
(7, N'2000000000077', N'CAS', 2, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'CL8000', N'CAS CL8000 Dokunmatik Terazi', N'Donanım', 0.0, 0.0, N'TR-0002', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'teraziler.html'),
(8, N'2000000000084', N'CAS', 2, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'CN-1', N'CAS CN-1 Sistem Terazisi', N'Donanım', 0.0, 0.0, N'TR-0003', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'teraziler.html'),
(9, N'2000000000091', N'Digi', 2, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'SM100P', N'Digi SM100P Boyunlu Terazi', N'Donanım', 0.0, 0.0, N'TR-0004', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'teraziler.html'),
(10, N'2000000000107', N'Digi', 2, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'SM-120T', N'Digi SM-120T Dokunmatik Terazi', N'Donanım', 0.0, 0.0, N'TR-0005', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'teraziler.html'),
(11, N'2000000000114', N'CAS', 2, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'ER-JR', N'CAS ER-JR Masaüstü Terazi', N'Donanım', 0.0, 0.0, N'TR-0006', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'teraziler.html'),
(12, N'2000000000121', N'CAS', 2, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'FW-500', N'CAS FW-500 Su Geçirmez Terazi', N'Donanım', 0.0, 0.0, N'TR-0007', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'teraziler.html'),
(13, N'2000000000138', N'CAS', 2, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'PDI', N'CAS PDI Ankastre Kasa Terazisi', N'Donanım', 0.0, 0.0, N'TR-0008', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'teraziler.html'),
(14, N'2000000000145', N'Hillpos', 3, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'HRS-28', N'Hillpos HRS-28', N'Donanım', 0.0, 0.0, N'BO-0001', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'barkod-okuyucular.html'),
(15, N'2000000000152', N'Hillpos', 3, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'HSC-82', N'Hillpos HSC-82', N'Donanım', 0.0, 0.0, N'BO-0002', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'barkod-okuyucular.html'),
(16, N'2000000000169', N'Hillpos', 3, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'HSD-92', N'Hillpos HSD-92', N'Donanım', 0.0, 0.0, N'BO-0003', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'barkod-okuyucular.html'),
(17, N'2000000000176', N'Newland', 3, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'TP-13', N'Newland TP-13', N'Donanım', 0.0, 0.0, N'BO-0004', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'barkod-okuyucular.html'),
(18, N'2000000000183', N'Newland', 3, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'TP-14', N'Newland TP-14', N'Donanım', 0.0, 0.0, N'BO-0005', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'barkod-okuyucular.html'),
(19, N'2000000000190', N'Hillpos', 3, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'HS-6700', N'Hillpos HS-6700', N'Donanım', 0.0, 0.0, N'BO-0006', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'barkod-okuyucular.html'),
(20, N'2000000000206', N'Hillpos', 3, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'VS-6800', N'Hillpos VS-6800', N'Donanım', 0.0, 0.0, N'BO-0007', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'barkod-okuyucular.html'),
(21, N'2000000000213', N'Argox', 4, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'OS-214 Plus', N'Argox OS-214 Plus Barkod Yazıcı', N'Donanım', 0.0, 0.0, N'YZ-0001', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'yazicilar.html'),
(22, N'2000000000220', N'Hillpos', 4, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'HDT-400', N'Hillpos HDT-400 Barkod Yazıcı', N'Donanım', 0.0, 0.0, N'YZ-0002', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'yazicilar.html'),
(23, N'2000000000237', N'Hillpos', 4, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'HTT-440', N'Hillpos HTT-440 Barkod Yazıcı', N'Donanım', 0.0, 0.0, N'YZ-0003', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'yazicilar.html'),
(24, N'2000000000244', N'TSC', 4, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'TTP-244CE', N'TSC TTP-244CE Barkod Yazıcı', N'Donanım', 0.0, 0.0, N'YZ-0004', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'yazicilar.html'),
(25, N'2000000000251', N'Xprinter', 4, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'XP-470B', N'Xprinter XP-470B Barkod Yazıcı', N'Donanım', 0.0, 0.0, N'YZ-0005', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'yazicilar.html'),
(26, N'2000000000268', N'Hillpos', 4, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'H380', N'Hillpos H380 Fiş Yazıcı', N'Donanım', 0.0, 0.0, N'YZ-0006', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'yazicilar.html'),
(27, N'2000000000275', N'Hillpos', 4, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'Q800', N'Hillpos Q800 Fiş Yazıcı', N'Donanım', 0.0, 0.0, N'YZ-0007', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'yazicilar.html'),
(28, N'2000000000282', N'Bixolon', 4, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'SPP-R310', N'Bixolon SPP-R310 Mobil Fiş Yazıcı', N'Donanım', 0.0, 0.0, N'YZ-0008', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'yazicilar.html'),
(29, N'2000000000299', N'Chainway', 5, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'C61', N'Chainway C61', N'Donanım', 0.0, 0.0, N'ET-0001', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'el-terminali.html'),
(30, N'2000000000305', N'Chainway', 5, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'C66', N'Chainway C66', N'Donanım', 0.0, 0.0, N'ET-0002', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'el-terminali.html'),
(31, N'2000000000312', N'Hillpos', 5, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'C7X', N'Hillpos C7X Tablet', N'Donanım', 0.0, 0.0, N'ET-0003', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'el-terminali.html'),
(32, N'2000000000329', N'Hillpos', 5, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'CM550X', N'Hillpos CM550X', N'Donanım', 0.0, 0.0, N'ET-0004', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'el-terminali.html'),
(33, N'2000000000336', N'Hillpos', 5, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'HT42', N'Hillpos HT42', N'Donanım', 0.0, 0.0, N'ET-0005', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'el-terminali.html'),
(34, N'2000000000343', N'Hillpos', 5, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'HT42K', N'Hillpos HT42K', N'Donanım', 0.0, 0.0, N'ET-0006', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'el-terminali.html'),
(35, N'2000000000350', N'Hillpos', 5, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'HT44', N'Hillpos HT44', N'Donanım', 0.0, 0.0, N'ET-0007', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'el-terminali.html'),
(36, N'2000000000367', N'Hillpos', 6, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'Touch Pro 15', N'Hillpos Touch Pro 15', N'Donanım', 0.0, 0.0, N'PC-0001', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'dokunmatik-pos.html'),
(37, N'2000000000374', N'Hillpos', 6, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'All-in-One Dual POS', N'Hillpos All-in-One Dual POS', N'Donanım', 0.0, 0.0, N'PC-0002', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'dokunmatik-pos.html'),
(38, N'2000000000381', N'Hillpos', 6, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'Slim Touch 15', N'Hillpos Slim Touch 15', N'Donanım', 0.0, 0.0, N'PC-0003', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'dokunmatik-pos.html'),
(39, N'2000000000398', N'Hillpos', 6, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'Kiosk POS 21.5', N'Hillpos Kiosk POS 21.5', N'Donanım', 0.0, 0.0, N'PC-0004', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'dokunmatik-pos.html'),
(40, N'2000000000404', NULL, 7, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, NULL, N'Özel ERP & CRM Yazılımları', N'Yazılım', 0.0, 0.0, N'YW-0001', 0.0, 3, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N'Adet', NULL, N'kurumsal-yazilim.html'),
(41, N'2000000000411', NULL, 7, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, NULL, N'Stok ve Depo Yönetimi Yazılımı', N'Yazılım', 0.0, 0.0, N'YW-0002', 0.0, 3, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N'Adet', NULL, N'kurumsal-yazilim.html'),
(42, N'2000000000428', NULL, 7, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, NULL, N'Fabrika ve Üretim Takibi Yazılımı', N'Yazılım', 0.0, 0.0, N'YW-0003', 0.0, 3, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N'Adet', NULL, N'kurumsal-yazilim.html');
INSERT INTO [Products] ([Id], [Barcode], [Brand], [CategoryId], [CreatedAtUtc], [Description], [ImagePath], [IsActive], [MinimumStockQuantity], [Model], [Name], [ProductType], [PurchasePrice], [SalePrice], [StockCode], [StockQuantity], [TaxRateId], [TrackLots], [TrackSerialNumbers], [TrackStock], [Unit], [UpdatedAtUtc], [WebsitePath])
VALUES (43, N'2000000000435', NULL, 7, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, NULL, N'API ve Donanım Entegrasyonları', N'Yazılım', 0.0, 0.0, N'YW-0004', 0.0, 3, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N'Adet', NULL, N'kurumsal-yazilim.html'),
(44, N'2000000000442', NULL, 7, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, NULL, N'GİB & E-Fatura Çözümleri', N'Yazılım', 0.0, 0.0, N'YW-0005', 0.0, 3, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N'Adet', NULL, N'kurumsal-yazilim.html'),
(45, N'2000000000459', N'Genel', 8, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'Metal Kasa', N'Para Çekmecesi', N'Donanım', 0.0, 0.0, N'PE-0001', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'index.html'),
(46, N'2000000000466', N'Genel', 8, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'Fiyat Sorgulama Terminali', N'Fiyat Gör Cihazı', N'Donanım', 0.0, 0.0, N'PE-0002', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'index.html'),
(47, N'2000000000473', N'Genel', 8, '2026-07-27T00:00:00.0000000Z', NULL, NULL, CAST(1 AS bit), 0.0, N'Mobil Fiş Yazıcı', N'Mobil Yazıcı', N'Donanım', 0.0, 0.0, N'PE-0003', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N'Adet', NULL, N'index.html');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Barcode', N'Brand', N'CategoryId', N'CreatedAtUtc', N'Description', N'ImagePath', N'IsActive', N'MinimumStockQuantity', N'Model', N'Name', N'ProductType', N'PurchasePrice', N'SalePrice', N'StockCode', N'StockQuantity', N'TaxRateId', N'TrackLots', N'TrackSerialNumbers', N'TrackStock', N'Unit', N'UpdatedAtUtc', N'WebsitePath') AND [object_id] = OBJECT_ID(N'[Products]'))
    SET IDENTITY_INSERT [Products] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BranchId', N'Code', N'CreatedAtUtc', N'IsActive', N'IsDefault', N'Name', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Warehouses]'))
    SET IDENTITY_INSERT [Warehouses] ON;
INSERT INTO [Warehouses] ([Id], [BranchId], [Code], [CreatedAtUtc], [IsActive], [IsDefault], [Name], [UpdatedAtUtc])
VALUES (1, 1, N'MERKEZ', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), N'Merkez Depo', NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BranchId', N'Code', N'CreatedAtUtc', N'IsActive', N'IsDefault', N'Name', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Warehouses]'))
    SET IDENTITY_INSERT [Warehouses] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Barcode', N'BarcodeType', N'CreatedAtUtc', N'IsActive', N'IsPrimary', N'ProductId', N'ProductVariantId', N'UnitMultiplier', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[ProductBarcodes]'))
    SET IDENTITY_INSERT [ProductBarcodes] ON;
INSERT INTO [ProductBarcodes] ([Id], [Barcode], [BarcodeType], [CreatedAtUtc], [IsActive], [IsPrimary], [ProductId], [ProductVariantId], [UnitMultiplier], [UpdatedAtUtc])
VALUES (1, N'2000000000015', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 1, NULL, 1.0, NULL),
(2, N'2000000000022', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 2, NULL, 1.0, NULL),
(3, N'2000000000039', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 3, NULL, 1.0, NULL),
(4, N'2000000000046', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 4, NULL, 1.0, NULL),
(5, N'2000000000053', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 5, NULL, 1.0, NULL),
(6, N'2000000000060', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 6, NULL, 1.0, NULL),
(7, N'2000000000077', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 7, NULL, 1.0, NULL),
(8, N'2000000000084', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 8, NULL, 1.0, NULL),
(9, N'2000000000091', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 9, NULL, 1.0, NULL),
(10, N'2000000000107', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 10, NULL, 1.0, NULL),
(11, N'2000000000114', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 11, NULL, 1.0, NULL),
(12, N'2000000000121', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 12, NULL, 1.0, NULL),
(13, N'2000000000138', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 13, NULL, 1.0, NULL),
(14, N'2000000000145', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 14, NULL, 1.0, NULL),
(15, N'2000000000152', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 15, NULL, 1.0, NULL),
(16, N'2000000000169', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 16, NULL, 1.0, NULL),
(17, N'2000000000176', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 17, NULL, 1.0, NULL),
(18, N'2000000000183', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 18, NULL, 1.0, NULL),
(19, N'2000000000190', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 19, NULL, 1.0, NULL),
(20, N'2000000000206', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 20, NULL, 1.0, NULL),
(21, N'2000000000213', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 21, NULL, 1.0, NULL),
(22, N'2000000000220', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 22, NULL, 1.0, NULL),
(23, N'2000000000237', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 23, NULL, 1.0, NULL),
(24, N'2000000000244', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 24, NULL, 1.0, NULL),
(25, N'2000000000251', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 25, NULL, 1.0, NULL),
(26, N'2000000000268', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 26, NULL, 1.0, NULL),
(27, N'2000000000275', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 27, NULL, 1.0, NULL),
(28, N'2000000000282', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 28, NULL, 1.0, NULL),
(29, N'2000000000299', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 29, NULL, 1.0, NULL),
(30, N'2000000000305', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 30, NULL, 1.0, NULL),
(31, N'2000000000312', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 31, NULL, 1.0, NULL),
(32, N'2000000000329', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 32, NULL, 1.0, NULL),
(33, N'2000000000336', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 33, NULL, 1.0, NULL),
(34, N'2000000000343', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 34, NULL, 1.0, NULL),
(35, N'2000000000350', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 35, NULL, 1.0, NULL),
(36, N'2000000000367', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 36, NULL, 1.0, NULL),
(37, N'2000000000374', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 37, NULL, 1.0, NULL),
(38, N'2000000000381', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 38, NULL, 1.0, NULL),
(39, N'2000000000398', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 39, NULL, 1.0, NULL),
(40, N'2000000000404', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 40, NULL, 1.0, NULL),
(41, N'2000000000411', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 41, NULL, 1.0, NULL),
(42, N'2000000000428', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 42, NULL, 1.0, NULL);
INSERT INTO [ProductBarcodes] ([Id], [Barcode], [BarcodeType], [CreatedAtUtc], [IsActive], [IsPrimary], [ProductId], [ProductVariantId], [UnitMultiplier], [UpdatedAtUtc])
VALUES (43, N'2000000000435', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 43, NULL, 1.0, NULL),
(44, N'2000000000442', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 44, NULL, 1.0, NULL),
(45, N'2000000000459', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 45, NULL, 1.0, NULL),
(46, N'2000000000466', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 46, NULL, 1.0, NULL),
(47, N'2000000000473', N'EAN13', '2026-07-27T00:00:00.0000000Z', CAST(1 AS bit), CAST(1 AS bit), 47, NULL, 1.0, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Barcode', N'BarcodeType', N'CreatedAtUtc', N'IsActive', N'IsPrimary', N'ProductId', N'ProductVariantId', N'UnitMultiplier', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[ProductBarcodes]'))
    SET IDENTITY_INSERT [ProductBarcodes] OFF;

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_AuditLogs_EntityName_EntityId] ON [AuditLogs] ([EntityName], [EntityId]);

CREATE UNIQUE INDEX [IX_AuditLogs_RecordId] ON [AuditLogs] ([RecordId]);

CREATE INDEX [IX_AuditLogs_UserId_CreatedAtUtc] ON [AuditLogs] ([UserId], [CreatedAtUtc]);

CREATE UNIQUE INDEX [IX_Branches_Code] ON [Branches] ([Code]);

CREATE UNIQUE INDEX [IX_Branches_RecordId] ON [Branches] ([RecordId]);

CREATE UNIQUE INDEX [IX_BusinessOrderLines_BusinessOrderId_LineNumber] ON [BusinessOrderLines] ([BusinessOrderId], [LineNumber]);

CREATE INDEX [IX_BusinessOrderLines_ProductId] ON [BusinessOrderLines] ([ProductId]);

CREATE INDEX [IX_BusinessOrderLines_ProductVariantId] ON [BusinessOrderLines] ([ProductVariantId]);

CREATE UNIQUE INDEX [IX_BusinessOrderLines_RecordId] ON [BusinessOrderLines] ([RecordId]);

CREATE INDEX [IX_BusinessOrders_CustomerId_OrderDateUtc] ON [BusinessOrders] ([CustomerId], [OrderDateUtc]);

CREATE UNIQUE INDEX [IX_BusinessOrders_OrderType_OrderNumber] ON [BusinessOrders] ([OrderType], [OrderNumber]);

CREATE INDEX [IX_BusinessOrders_QuoteId] ON [BusinessOrders] ([QuoteId]);

CREATE UNIQUE INDEX [IX_BusinessOrders_RecordId] ON [BusinessOrders] ([RecordId]);

CREATE UNIQUE INDEX [IX_BusinessProjects_Code] ON [BusinessProjects] ([Code]);

CREATE UNIQUE INDEX [IX_BusinessProjects_RecordId] ON [BusinessProjects] ([RecordId]);

CREATE UNIQUE INDEX [IX_CompanySettings_RecordId] ON [CompanySettings] ([RecordId]);

CREATE UNIQUE INDEX [IX_CostCenters_Code] ON [CostCenters] ([Code]);

CREATE UNIQUE INDEX [IX_CostCenters_RecordId] ON [CostCenters] ([RecordId]);

CREATE UNIQUE INDEX [IX_Currencies_Code] ON [Currencies] ([Code]);

CREATE UNIQUE INDEX [IX_Currencies_RecordId] ON [Currencies] ([RecordId]);

CREATE INDEX [IX_CurrentAccountTransactions_CustomerId_TransactionDateUtc] ON [CurrentAccountTransactions] ([CustomerId], [TransactionDateUtc]);

CREATE INDEX [IX_CurrentAccountTransactions_DocumentNumber] ON [CurrentAccountTransactions] ([DocumentNumber]);

CREATE INDEX [IX_CurrentAccountTransactions_InvoiceId] ON [CurrentAccountTransactions] ([InvoiceId]);

CREATE INDEX [IX_CurrentAccountTransactions_QuoteId] ON [CurrentAccountTransactions] ([QuoteId]);

CREATE UNIQUE INDEX [IX_CurrentAccountTransactions_RecordId] ON [CurrentAccountTransactions] ([RecordId]);

CREATE INDEX [IX_CustomerAddresses_CustomerId_AddressType] ON [CustomerAddresses] ([CustomerId], [AddressType]);

CREATE UNIQUE INDEX [IX_CustomerAddresses_RecordId] ON [CustomerAddresses] ([RecordId]);

CREATE INDEX [IX_CustomerContacts_CustomerId] ON [CustomerContacts] ([CustomerId]);

CREATE UNIQUE INDEX [IX_CustomerContacts_RecordId] ON [CustomerContacts] ([RecordId]);

CREATE UNIQUE INDEX [IX_Customers_Code] ON [Customers] ([Code]);

CREATE INDEX [IX_Customers_Name] ON [Customers] ([Name]);

CREATE UNIQUE INDEX [IX_Customers_RecordId] ON [Customers] ([RecordId]);

CREATE INDEX [IX_Customers_TaxNumber] ON [Customers] ([TaxNumber]);

CREATE UNIQUE INDEX [IX_DispatchNoteLines_DispatchNoteId_LineNumber] ON [DispatchNoteLines] ([DispatchNoteId], [LineNumber]);

CREATE INDEX [IX_DispatchNoteLines_ProductId] ON [DispatchNoteLines] ([ProductId]);

CREATE INDEX [IX_DispatchNoteLines_ProductVariantId] ON [DispatchNoteLines] ([ProductVariantId]);

CREATE UNIQUE INDEX [IX_DispatchNoteLines_RecordId] ON [DispatchNoteLines] ([RecordId]);

CREATE INDEX [IX_DispatchNotes_BusinessOrderId] ON [DispatchNotes] ([BusinessOrderId]);

CREATE INDEX [IX_DispatchNotes_CustomerId_DispatchDateUtc] ON [DispatchNotes] ([CustomerId], [DispatchDateUtc]);

CREATE UNIQUE INDEX [IX_DispatchNotes_DispatchType_DispatchNumber] ON [DispatchNotes] ([DispatchType], [DispatchNumber]);

CREATE INDEX [IX_DispatchNotes_InvoiceId] ON [DispatchNotes] ([InvoiceId]);

CREATE UNIQUE INDEX [IX_DispatchNotes_RecordId] ON [DispatchNotes] ([RecordId]);

CREATE INDEX [IX_DispatchNotes_WarehouseId] ON [DispatchNotes] ([WarehouseId]);

CREATE UNIQUE INDEX [IX_ExchangeRates_CurrencyId_RateDateUtc] ON [ExchangeRates] ([CurrencyId], [RateDateUtc]);

CREATE UNIQUE INDEX [IX_ExchangeRates_RecordId] ON [ExchangeRates] ([RecordId]);

CREATE UNIQUE INDEX [IX_ExpenseCategories_Code] ON [ExpenseCategories] ([Code]);

CREATE UNIQUE INDEX [IX_ExpenseCategories_RecordId] ON [ExpenseCategories] ([RecordId]);

CREATE INDEX [IX_Expenses_BusinessProjectId] ON [Expenses] ([BusinessProjectId]);

CREATE INDEX [IX_Expenses_CostCenterId] ON [Expenses] ([CostCenterId]);

CREATE INDEX [IX_Expenses_CustomerId] ON [Expenses] ([CustomerId]);

CREATE INDEX [IX_Expenses_DocumentNumber] ON [Expenses] ([DocumentNumber]);

CREATE INDEX [IX_Expenses_ExpenseCategoryId_ExpenseDateUtc] ON [Expenses] ([ExpenseCategoryId], [ExpenseDateUtc]);

CREATE INDEX [IX_Expenses_FinancialAccountId] ON [Expenses] ([FinancialAccountId]);

CREATE UNIQUE INDEX [IX_Expenses_RecordId] ON [Expenses] ([RecordId]);

CREATE INDEX [IX_Expenses_TaxRateId] ON [Expenses] ([TaxRateId]);

CREATE INDEX [IX_ExternalRecordMappings_EntityType_InternalId] ON [ExternalRecordMappings] ([EntityType], [InternalId]);

CREATE UNIQUE INDEX [IX_ExternalRecordMappings_RecordId] ON [ExternalRecordMappings] ([RecordId]);

CREATE UNIQUE INDEX [IX_ExternalRecordMappings_SourceSystem_EntityType_ExternalId] ON [ExternalRecordMappings] ([SourceSystem], [EntityType], [ExternalId]);

CREATE UNIQUE INDEX [IX_FinancialAccounts_Code] ON [FinancialAccounts] ([Code]);

CREATE INDEX [IX_FinancialAccounts_Iban] ON [FinancialAccounts] ([Iban]);

CREATE UNIQUE INDEX [IX_FinancialAccounts_RecordId] ON [FinancialAccounts] ([RecordId]);

CREATE INDEX [IX_FinancialTransactions_CurrentAccountTransactionId] ON [FinancialTransactions] ([CurrentAccountTransactionId]);

CREATE INDEX [IX_FinancialTransactions_CustomerId] ON [FinancialTransactions] ([CustomerId]);

CREATE INDEX [IX_FinancialTransactions_DocumentNumber] ON [FinancialTransactions] ([DocumentNumber]);

CREATE INDEX [IX_FinancialTransactions_FinancialAccountId_TransactionDateUtc] ON [FinancialTransactions] ([FinancialAccountId], [TransactionDateUtc]);

CREATE UNIQUE INDEX [IX_FinancialTransactions_RecordId] ON [FinancialTransactions] ([RecordId]);

CREATE INDEX [IX_IntegrationOutboxMessages_ProcessedAtUtc_OccurredAtUtc] ON [IntegrationOutboxMessages] ([ProcessedAtUtc], [OccurredAtUtc]);

CREATE UNIQUE INDEX [IX_IntegrationOutboxMessages_RecordId] ON [IntegrationOutboxMessages] ([RecordId]);

CREATE UNIQUE INDEX [IX_InventoryCountLines_InventoryCountId_ProductId_ProductVariantId] ON [InventoryCountLines] ([InventoryCountId], [ProductId], [ProductVariantId]) WHERE [ProductVariantId] IS NOT NULL;

CREATE INDEX [IX_InventoryCountLines_ProductId] ON [InventoryCountLines] ([ProductId]);

CREATE INDEX [IX_InventoryCountLines_ProductVariantId] ON [InventoryCountLines] ([ProductVariantId]);

CREATE UNIQUE INDEX [IX_InventoryCountLines_RecordId] ON [InventoryCountLines] ([RecordId]);

CREATE UNIQUE INDEX [IX_InventoryCounts_CountNumber] ON [InventoryCounts] ([CountNumber]);

CREATE UNIQUE INDEX [IX_InventoryCounts_RecordId] ON [InventoryCounts] ([RecordId]);

CREATE INDEX [IX_InventoryCounts_WarehouseId_CountDateUtc] ON [InventoryCounts] ([WarehouseId], [CountDateUtc]);

CREATE UNIQUE INDEX [IX_InventorySettings_RecordId] ON [InventorySettings] ([RecordId]);

CREATE UNIQUE INDEX [IX_InvoiceLines_InvoiceId_LineNumber] ON [InvoiceLines] ([InvoiceId], [LineNumber]);

CREATE INDEX [IX_InvoiceLines_ProductId] ON [InvoiceLines] ([ProductId]);

CREATE INDEX [IX_InvoiceLines_ProductVariantId] ON [InvoiceLines] ([ProductVariantId]);

CREATE UNIQUE INDEX [IX_InvoiceLines_RecordId] ON [InvoiceLines] ([RecordId]);

CREATE INDEX [IX_InvoicePaymentSchedules_DueDateUtc] ON [InvoicePaymentSchedules] ([DueDateUtc]);

CREATE UNIQUE INDEX [IX_InvoicePaymentSchedules_InvoiceId_InstallmentNumber] ON [InvoicePaymentSchedules] ([InvoiceId], [InstallmentNumber]);

CREATE UNIQUE INDEX [IX_InvoicePaymentSchedules_RecordId] ON [InvoicePaymentSchedules] ([RecordId]);

CREATE INDEX [IX_Invoices_BusinessProjectId] ON [Invoices] ([BusinessProjectId]);

CREATE INDEX [IX_Invoices_CostCenterId] ON [Invoices] ([CostCenterId]);

CREATE INDEX [IX_Invoices_CustomerId_InvoiceDateUtc] ON [Invoices] ([CustomerId], [InvoiceDateUtc]);

CREATE UNIQUE INDEX [IX_Invoices_InvoiceType_InvoiceNumber] ON [Invoices] ([InvoiceType], [InvoiceNumber]);

CREATE INDEX [IX_Invoices_InvoiceType_Status_InvoiceDateUtc] ON [Invoices] ([InvoiceType], [Status], [InvoiceDateUtc]);

CREATE INDEX [IX_Invoices_PurchasePriceListId] ON [Invoices] ([PurchasePriceListId]);

CREATE INDEX [IX_Invoices_QuoteId] ON [Invoices] ([QuoteId]);

CREATE UNIQUE INDEX [IX_Invoices_RecordId] ON [Invoices] ([RecordId]);

CREATE INDEX [IX_Invoices_WarehouseId] ON [Invoices] ([WarehouseId]);

CREATE INDEX [IX_NegotiableInstruments_CustomerId_DueDateUtc] ON [NegotiableInstruments] ([CustomerId], [DueDateUtc]);

CREATE INDEX [IX_NegotiableInstruments_FinancialAccountId] ON [NegotiableInstruments] ([FinancialAccountId]);

CREATE UNIQUE INDEX [IX_NegotiableInstruments_InstrumentType_InstrumentNumber] ON [NegotiableInstruments] ([InstrumentType], [InstrumentNumber]);

CREATE UNIQUE INDEX [IX_NegotiableInstruments_RecordId] ON [NegotiableInstruments] ([RecordId]);

CREATE INDEX [IX_NegotiableInstruments_Status_DueDateUtc] ON [NegotiableInstruments] ([Status], [DueDateUtc]);

CREATE UNIQUE INDEX [IX_NumberSequences_Key] ON [NumberSequences] ([Key]);

CREATE UNIQUE INDEX [IX_NumberSequences_RecordId] ON [NumberSequences] ([RecordId]);

CREATE INDEX [IX_PaymentReceiptLines_CurrentAccountTransactionId] ON [PaymentReceiptLines] ([CurrentAccountTransactionId]);

CREATE INDEX [IX_PaymentReceiptLines_FinancialAccountId] ON [PaymentReceiptLines] ([FinancialAccountId]);

CREATE INDEX [IX_PaymentReceiptLines_FinancialTransactionId] ON [PaymentReceiptLines] ([FinancialTransactionId]);

CREATE UNIQUE INDEX [IX_PaymentReceiptLines_PaymentReceiptId_LineNumber] ON [PaymentReceiptLines] ([PaymentReceiptId], [LineNumber]);

CREATE UNIQUE INDEX [IX_PaymentReceiptLines_RecordId] ON [PaymentReceiptLines] ([RecordId]);

CREATE INDEX [IX_PaymentReceipts_BusinessProjectId] ON [PaymentReceipts] ([BusinessProjectId]);

CREATE INDEX [IX_PaymentReceipts_CostCenterId] ON [PaymentReceipts] ([CostCenterId]);

CREATE INDEX [IX_PaymentReceipts_CustomerId_ReceiptDateUtc] ON [PaymentReceipts] ([CustomerId], [ReceiptDateUtc]);

CREATE UNIQUE INDEX [IX_PaymentReceipts_ReceiptType_ReceiptNumber] ON [PaymentReceipts] ([ReceiptType], [ReceiptNumber]);

CREATE UNIQUE INDEX [IX_PaymentReceipts_RecordId] ON [PaymentReceipts] ([RecordId]);

CREATE UNIQUE INDEX [IX_ProductBarcodes_Barcode] ON [ProductBarcodes] ([Barcode]);

CREATE INDEX [IX_ProductBarcodes_ProductId_ProductVariantId] ON [ProductBarcodes] ([ProductId], [ProductVariantId]);

CREATE INDEX [IX_ProductBarcodes_ProductVariantId] ON [ProductBarcodes] ([ProductVariantId]);

CREATE UNIQUE INDEX [IX_ProductBarcodes_RecordId] ON [ProductBarcodes] ([RecordId]);

CREATE UNIQUE INDEX [IX_ProductCategories_Code] ON [ProductCategories] ([Code]);

CREATE UNIQUE INDEX [IX_ProductCategories_RecordId] ON [ProductCategories] ([RecordId]);

CREATE UNIQUE INDEX [IX_ProductColors_Code] ON [ProductColors] ([Code]);

CREATE UNIQUE INDEX [IX_ProductColors_RecordId] ON [ProductColors] ([RecordId]);

CREATE INDEX [IX_ProductImages_ProductId_DisplayOrder] ON [ProductImages] ([ProductId], [DisplayOrder]);

CREATE INDEX [IX_ProductImages_ProductVariantId] ON [ProductImages] ([ProductVariantId]);

CREATE UNIQUE INDEX [IX_ProductImages_RecordId] ON [ProductImages] ([RecordId]);

CREATE UNIQUE INDEX [IX_Products_Barcode] ON [Products] ([Barcode]) WHERE [Barcode] IS NOT NULL;

CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);

CREATE UNIQUE INDEX [IX_Products_RecordId] ON [Products] ([RecordId]);

CREATE UNIQUE INDEX [IX_Products_StockCode] ON [Products] ([StockCode]);

CREATE INDEX [IX_Products_TaxRateId] ON [Products] ([TaxRateId]);

CREATE INDEX [IX_ProductSerialNumbers_ProductId_LotNumber_ExpirationDateUtc] ON [ProductSerialNumbers] ([ProductId], [LotNumber], [ExpirationDateUtc]);

CREATE INDEX [IX_ProductSerialNumbers_ProductVariantId] ON [ProductSerialNumbers] ([ProductVariantId]);

CREATE UNIQUE INDEX [IX_ProductSerialNumbers_RecordId] ON [ProductSerialNumbers] ([RecordId]);

CREATE UNIQUE INDEX [IX_ProductSerialNumbers_SerialNumber] ON [ProductSerialNumbers] ([SerialNumber]);

CREATE INDEX [IX_ProductSerialNumbers_WarehouseId_IsInStock] ON [ProductSerialNumbers] ([WarehouseId], [IsInStock]);

CREATE UNIQUE INDEX [IX_ProductUnitConversions_ProductId_UnitOfMeasureId] ON [ProductUnitConversions] ([ProductId], [UnitOfMeasureId]);

CREATE UNIQUE INDEX [IX_ProductUnitConversions_RecordId] ON [ProductUnitConversions] ([RecordId]);

CREATE INDEX [IX_ProductUnitConversions_UnitOfMeasureId] ON [ProductUnitConversions] ([UnitOfMeasureId]);

CREATE INDEX [IX_ProductVariants_ColorId] ON [ProductVariants] ([ColorId]);

CREATE INDEX [IX_ProductVariants_ProductId_ColorId] ON [ProductVariants] ([ProductId], [ColorId]);

CREATE UNIQUE INDEX [IX_ProductVariants_RecordId] ON [ProductVariants] ([RecordId]);

CREATE UNIQUE INDEX [IX_ProductVariants_VariantCode] ON [ProductVariants] ([VariantCode]);

CREATE INDEX [IX_PurchasePriceListItems_ProductId] ON [PurchasePriceListItems] ([ProductId]);

CREATE UNIQUE INDEX [IX_PurchasePriceListItems_PurchasePriceListId_ProductId_MinimumQuantity] ON [PurchasePriceListItems] ([PurchasePriceListId], [ProductId], [MinimumQuantity]);

CREATE UNIQUE INDEX [IX_PurchasePriceListItems_RecordId] ON [PurchasePriceListItems] ([RecordId]);

CREATE UNIQUE INDEX [IX_PurchasePriceLists_Code] ON [PurchasePriceLists] ([Code]);

CREATE INDEX [IX_PurchasePriceLists_CustomerId_ValidFromUtc_ValidUntilUtc] ON [PurchasePriceLists] ([CustomerId], [ValidFromUtc], [ValidUntilUtc]);

CREATE UNIQUE INDEX [IX_PurchasePriceLists_RecordId] ON [PurchasePriceLists] ([RecordId]);

CREATE INDEX [IX_QuoteLines_ProductId] ON [QuoteLines] ([ProductId]);

CREATE UNIQUE INDEX [IX_QuoteLines_QuoteId_LineNumber] ON [QuoteLines] ([QuoteId], [LineNumber]);

CREATE UNIQUE INDEX [IX_QuoteLines_RecordId] ON [QuoteLines] ([RecordId]);

CREATE INDEX [IX_Quotes_CustomerId_QuoteDateUtc] ON [Quotes] ([CustomerId], [QuoteDateUtc]);

CREATE UNIQUE INDEX [IX_Quotes_QuoteNumber] ON [Quotes] ([QuoteNumber]);

CREATE UNIQUE INDEX [IX_Quotes_RecordId] ON [Quotes] ([RecordId]);

CREATE INDEX [IX_Quotes_Status] ON [Quotes] ([Status]);

CREATE INDEX [IX_SalesPriceListItems_ProductId] ON [SalesPriceListItems] ([ProductId]);

CREATE INDEX [IX_SalesPriceListItems_ProductVariantId] ON [SalesPriceListItems] ([ProductVariantId]);

CREATE UNIQUE INDEX [IX_SalesPriceListItems_RecordId] ON [SalesPriceListItems] ([RecordId]);

CREATE UNIQUE INDEX [IX_SalesPriceListItems_SalesPriceListId_ProductId_ProductVariantId_MinimumQuantity] ON [SalesPriceListItems] ([SalesPriceListId], [ProductId], [ProductVariantId], [MinimumQuantity]) WHERE [ProductVariantId] IS NOT NULL;

CREATE UNIQUE INDEX [IX_SalesPriceLists_Code] ON [SalesPriceLists] ([Code]);

CREATE INDEX [IX_SalesPriceLists_CustomerId_ValidFromUtc_ValidUntilUtc] ON [SalesPriceLists] ([CustomerId], [ValidFromUtc], [ValidUntilUtc]);

CREATE UNIQUE INDEX [IX_SalesPriceLists_RecordId] ON [SalesPriceLists] ([RecordId]);

CREATE UNIQUE INDEX [IX_ScaleProductSettings_Prefix_PluCode] ON [ScaleProductSettings] ([Prefix], [PluCode]);

CREATE UNIQUE INDEX [IX_ScaleProductSettings_ProductId] ON [ScaleProductSettings] ([ProductId]);

CREATE UNIQUE INDEX [IX_ScaleProductSettings_RecordId] ON [ScaleProductSettings] ([RecordId]);

CREATE INDEX [IX_StockMovements_BusinessProjectId] ON [StockMovements] ([BusinessProjectId]);

CREATE INDEX [IX_StockMovements_CostCenterId] ON [StockMovements] ([CostCenterId]);

CREATE INDEX [IX_StockMovements_DocumentNumber] ON [StockMovements] ([DocumentNumber]);

CREATE INDEX [IX_StockMovements_InventoryCountLineId] ON [StockMovements] ([InventoryCountLineId]);

CREATE INDEX [IX_StockMovements_InvoiceLineId] ON [StockMovements] ([InvoiceLineId]);

CREATE INDEX [IX_StockMovements_ProductId_WarehouseId_MovementDateUtc] ON [StockMovements] ([ProductId], [WarehouseId], [MovementDateUtc]);

CREATE INDEX [IX_StockMovements_ProductVariantId] ON [StockMovements] ([ProductVariantId]);

CREATE UNIQUE INDEX [IX_StockMovements_RecordId] ON [StockMovements] ([RecordId]);

CREATE INDEX [IX_StockMovements_StockSlipLineId] ON [StockMovements] ([StockSlipLineId]);

CREATE INDEX [IX_StockMovements_StockTransferLineId] ON [StockMovements] ([StockTransferLineId]);

CREATE INDEX [IX_StockMovements_WarehouseId] ON [StockMovements] ([WarehouseId]);

CREATE INDEX [IX_StockReservations_ProductId_ProductVariantId_WarehouseId_Status] ON [StockReservations] ([ProductId], [ProductVariantId], [WarehouseId], [Status]);

CREATE INDEX [IX_StockReservations_ProductVariantId] ON [StockReservations] ([ProductVariantId]);

CREATE INDEX [IX_StockReservations_QuoteLineId] ON [StockReservations] ([QuoteLineId]);

CREATE UNIQUE INDEX [IX_StockReservations_RecordId] ON [StockReservations] ([RecordId]);

CREATE INDEX [IX_StockReservations_Status_ReservedUntilUtc] ON [StockReservations] ([Status], [ReservedUntilUtc]);

CREATE INDEX [IX_StockReservations_WarehouseId] ON [StockReservations] ([WarehouseId]);

CREATE INDEX [IX_StockSlipLines_ProductId] ON [StockSlipLines] ([ProductId]);

CREATE INDEX [IX_StockSlipLines_ProductVariantId] ON [StockSlipLines] ([ProductVariantId]);

CREATE UNIQUE INDEX [IX_StockSlipLines_RecordId] ON [StockSlipLines] ([RecordId]);

CREATE UNIQUE INDEX [IX_StockSlipLines_StockSlipId_LineNumber] ON [StockSlipLines] ([StockSlipId], [LineNumber]);

CREATE INDEX [IX_StockSlips_BusinessProjectId] ON [StockSlips] ([BusinessProjectId]);

CREATE INDEX [IX_StockSlips_CostCenterId] ON [StockSlips] ([CostCenterId]);

CREATE UNIQUE INDEX [IX_StockSlips_RecordId] ON [StockSlips] ([RecordId]);

CREATE UNIQUE INDEX [IX_StockSlips_SlipNumber] ON [StockSlips] ([SlipNumber]);

CREATE INDEX [IX_StockSlips_WarehouseId_SlipDateUtc] ON [StockSlips] ([WarehouseId], [SlipDateUtc]);

CREATE INDEX [IX_StockTransferLines_ProductId] ON [StockTransferLines] ([ProductId]);

CREATE INDEX [IX_StockTransferLines_ProductVariantId] ON [StockTransferLines] ([ProductVariantId]);

CREATE UNIQUE INDEX [IX_StockTransferLines_RecordId] ON [StockTransferLines] ([RecordId]);

CREATE UNIQUE INDEX [IX_StockTransferLines_StockTransferId_LineNumber] ON [StockTransferLines] ([StockTransferId], [LineNumber]);

CREATE INDEX [IX_StockTransfers_FromWarehouseId_TransferDateUtc] ON [StockTransfers] ([FromWarehouseId], [TransferDateUtc]);

CREATE UNIQUE INDEX [IX_StockTransfers_RecordId] ON [StockTransfers] ([RecordId]);

CREATE INDEX [IX_StockTransfers_ToWarehouseId_TransferDateUtc] ON [StockTransfers] ([ToWarehouseId], [TransferDateUtc]);

CREATE UNIQUE INDEX [IX_StockTransfers_TransferNumber] ON [StockTransfers] ([TransferNumber]);

CREATE UNIQUE INDEX [IX_TaxRates_Code] ON [TaxRates] ([Code]);

CREATE UNIQUE INDEX [IX_TaxRates_RecordId] ON [TaxRates] ([RecordId]);

CREATE UNIQUE INDEX [IX_UnitsOfMeasure_Code] ON [UnitsOfMeasure] ([Code]);

CREATE UNIQUE INDEX [IX_UnitsOfMeasure_RecordId] ON [UnitsOfMeasure] ([RecordId]);

CREATE INDEX [IX_Warehouses_BranchId_Name] ON [Warehouses] ([BranchId], [Name]);

CREATE UNIQUE INDEX [IX_Warehouses_Code] ON [Warehouses] ([Code]);

CREATE UNIQUE INDEX [IX_Warehouses_RecordId] ON [Warehouses] ([RecordId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260727194141_InitialSqlServer2022', N'10.0.10');

COMMIT;
GO
