SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(128) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(128) NOT NULL,
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(128) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(128) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(128) NOT NULL,
        [ProviderKey] nvarchar(128) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(128) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(128) NOT NULL,
        [RoleId] nvarchar(128) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(128) NOT NULL,
        [LoginProvider] nvarchar(128) NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
        CONSTRAINT [FK_BusinessOrderLines_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BusinessOrderLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
        CONSTRAINT [FK_InvoiceLines_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InvoiceLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
        CONSTRAINT [FK_DispatchNoteLines_ProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ProductVariants] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DispatchNoteLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'Code', N'CreatedAtUtc', N'IsActive', N'IsHeadOffice', N'Name', N'Phone', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Branches]'))
        SET IDENTITY_INSERT [Branches] ON;
    EXEC(N'INSERT INTO [Branches] ([Id], [Address], [Code], [CreatedAtUtc], [IsActive], [IsHeadOffice], [Name], [Phone], [UpdatedAtUtc])
    VALUES (1, NULL, N''MERKEZ'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), N''Merkez Şube'', NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'Code', N'CreatedAtUtc', N'IsActive', N'IsHeadOffice', N'Name', N'Phone', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Branches]'))
        SET IDENTITY_INSERT [Branches] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'BankName', N'CompanyName', N'CreatedAtUtc', N'Email', N'Iban', N'LogoPath', N'Phone', N'TaxNumber', N'TaxOffice', N'UpdatedAtUtc', N'Website') AND [object_id] = OBJECT_ID(N'[CompanySettings]'))
        SET IDENTITY_INSERT [CompanySettings] ON;
    EXEC(N'INSERT INTO [CompanySettings] ([Id], [Address], [BankName], [CompanyName], [CreatedAtUtc], [Email], [Iban], [LogoPath], [Phone], [TaxNumber], [TaxOffice], [UpdatedAtUtc], [Website])
    VALUES (1, NULL, NULL, N''ŞahinSoft'', ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'BankName', N'CompanyName', N'CreatedAtUtc', N'Email', N'Iban', N'LogoPath', N'Phone', N'TaxNumber', N'TaxOffice', N'UpdatedAtUtc', N'Website') AND [object_id] = OBJECT_ID(N'[CompanySettings]'))
        SET IDENTITY_INSERT [CompanySettings] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'IsActive', N'IsBaseCurrency', N'Name', N'Symbol', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Currencies]'))
        SET IDENTITY_INSERT [Currencies] ON;
    EXEC(N'INSERT INTO [Currencies] ([Id], [Code], [CreatedAtUtc], [IsActive], [IsBaseCurrency], [Name], [Symbol], [UpdatedAtUtc])
    VALUES (1, N''TRY'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), N''Türk Lirası'', N''₺'', NULL),
    (2, N''USD'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(0 AS bit), N''Amerikan Doları'', N''$'', NULL),
    (3, N''EUR'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(0 AS bit), N''Euro'', N''€'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'IsActive', N'IsBaseCurrency', N'Name', N'Symbol', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Currencies]'))
        SET IDENTITY_INSERT [Currencies] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AllowNegativeStock', N'AllowSaleBelowCost', N'AllowSaleWhenOutOfStock', N'AutoGenerateBarcode', N'CreatedAtUtc', N'DefaultBarcodeType', N'DefaultScalePrefix', N'EnableMinimumStockWarning', N'EnforceStockLevel', N'RequireBarcode', N'RequireProductVariant', N'RequireTransferApproval', N'TrackStockByVariant', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[InventorySettings]'))
        SET IDENTITY_INSERT [InventorySettings] ON;
    EXEC(N'INSERT INTO [InventorySettings] ([Id], [AllowNegativeStock], [AllowSaleBelowCost], [AllowSaleWhenOutOfStock], [AutoGenerateBarcode], [CreatedAtUtc], [DefaultBarcodeType], [DefaultScalePrefix], [EnableMinimumStockWarning], [EnforceStockLevel], [RequireBarcode], [RequireProductVariant], [RequireTransferApproval], [TrackStockByVariant], [UpdatedAtUtc])
    VALUES (1, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), ''2026-07-27T00:00:00.0000000Z'', N''EAN13'', N''27'', CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AllowNegativeStock', N'AllowSaleBelowCost', N'AllowSaleWhenOutOfStock', N'AutoGenerateBarcode', N'CreatedAtUtc', N'DefaultBarcodeType', N'DefaultScalePrefix', N'EnableMinimumStockWarning', N'EnforceStockLevel', N'RequireBarcode', N'RequireProductVariant', N'RequireTransferApproval', N'TrackStockByVariant', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[InventorySettings]'))
        SET IDENTITY_INSERT [InventorySettings] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
        SET IDENTITY_INSERT [NumberSequences] ON;
    EXEC(N'INSERT INTO [NumberSequences] ([Id], [CreatedAtUtc], [Key], [NextNumber], [Padding], [Prefix], [UpdatedAtUtc])
    VALUES (1, ''2026-07-27T00:00:00.0000000Z'', N''STOCK'', CAST(1 AS bigint), 3, N''SHN.'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
        SET IDENTITY_INSERT [NumberSequences] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'IsActive', N'Name', N'UpdatedAtUtc', N'WebsitePath') AND [object_id] = OBJECT_ID(N'[ProductCategories]'))
        SET IDENTITY_INSERT [ProductCategories] ON;
    EXEC(N'INSERT INTO [ProductCategories] ([Id], [Code], [CreatedAtUtc], [IsActive], [Name], [UpdatedAtUtc], [WebsitePath])
    VALUES (1, N''YAZARKASA'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), N''Yazar Kasa POS'', NULL, N''yazarkasa-pos.html''),
    (2, N''TERAZI'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), N''Teraziler'', NULL, N''teraziler.html''),
    (3, N''BARKOD'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), N''Barkod Okuyucular'', NULL, N''barkod-okuyucular.html''),
    (4, N''YAZICI'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), N''Yazıcılar'', NULL, N''yazicilar.html''),
    (5, N''ELTERM'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), N''El Terminalleri'', NULL, N''el-terminali.html''),
    (6, N''POSPC'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), N''Dokunmatik POS'', NULL, N''dokunmatik-pos.html''),
    (7, N''YAZILIM'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), N''Yazılım ve Entegrasyon'', NULL, N''kurumsal-yazilim.html''),
    (8, N''POSEKIP'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), N''POS Çevre Birimleri'', NULL, N''index.html'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'IsActive', N'Name', N'UpdatedAtUtc', N'WebsitePath') AND [object_id] = OBJECT_ID(N'[ProductCategories]'))
        SET IDENTITY_INSERT [ProductCategories] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'IsActive', N'IsExempt', N'Name', N'Rate', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[TaxRates]'))
        SET IDENTITY_INSERT [TaxRates] ON;
    EXEC(N'INSERT INTO [TaxRates] ([Id], [Code], [CreatedAtUtc], [IsActive], [IsExempt], [Name], [Rate], [UpdatedAtUtc])
    VALUES (1, N''KDV10'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(0 AS bit), N''KDV %10'', 10.0, NULL),
    (2, N''KDV20'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(0 AS bit), N''KDV %20'', 20.0, NULL),
    (3, N''KDV0'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), N''KDV %0'', 0.0, NULL),
    (4, N''KDV1'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(0 AS bit), N''KDV %1'', 1.0, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'IsActive', N'IsExempt', N'Name', N'Rate', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[TaxRates]'))
        SET IDENTITY_INSERT [TaxRates] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'DecimalPlaces', N'IsActive', N'Name', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[UnitsOfMeasure]'))
        SET IDENTITY_INSERT [UnitsOfMeasure] ON;
    EXEC(N'INSERT INTO [UnitsOfMeasure] ([Id], [Code], [CreatedAtUtc], [DecimalPlaces], [IsActive], [Name], [UpdatedAtUtc])
    VALUES (1, N''ADET'', ''2026-07-27T00:00:00.0000000Z'', 0, CAST(1 AS bit), N''Adet'', NULL),
    (2, N''KG'', ''2026-07-27T00:00:00.0000000Z'', 3, CAST(1 AS bit), N''Kilogram'', NULL),
    (3, N''PAKET'', ''2026-07-27T00:00:00.0000000Z'', 0, CAST(1 AS bit), N''Paket'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'DecimalPlaces', N'IsActive', N'Name', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[UnitsOfMeasure]'))
        SET IDENTITY_INSERT [UnitsOfMeasure] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Barcode', N'Brand', N'CategoryId', N'CreatedAtUtc', N'Description', N'ImagePath', N'IsActive', N'MinimumStockQuantity', N'Model', N'Name', N'ProductType', N'PurchasePrice', N'SalePrice', N'StockCode', N'StockQuantity', N'TaxRateId', N'TrackLots', N'TrackSerialNumbers', N'TrackStock', N'Unit', N'UpdatedAtUtc', N'WebsitePath') AND [object_id] = OBJECT_ID(N'[Products]'))
        SET IDENTITY_INSERT [Products] ON;
    EXEC(N'INSERT INTO [Products] ([Id], [Barcode], [Brand], [CategoryId], [CreatedAtUtc], [Description], [ImagePath], [IsActive], [MinimumStockQuantity], [Model], [Name], [ProductType], [PurchasePrice], [SalePrice], [StockCode], [StockQuantity], [TaxRateId], [TrackLots], [TrackSerialNumbers], [TrackStock], [Unit], [UpdatedAtUtc], [WebsitePath])
    VALUES (1, N''2000000000015'', N''Ingenico'', 1, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''IDE280'', N''Ingenico IDE280'', N''Donanım'', 0.0, 0.0, N''YK-0001'', 0.0, 1, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''yazarkasa-pos.html''),
    (2, N''2000000000022'', N''Ingenico'', 1, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''Move 5000F'', N''Ingenico Move 5000F'', N''Donanım'', 0.0, 0.0, N''YK-0002'', 0.0, 1, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''yazarkasa-pos.html''),
    (3, N''2000000000039'', N''PayGo'', 1, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''SP630PRO ECR'', N''PAYGO SP630PRO ECR'', N''Donanım'', 0.0, 0.0, N''YK-0003'', 0.0, 1, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''yazarkasa-pos.html''),
    (4, N''2000000000046'', N''Profilo'', 1, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''S900'', N''Profilo S900'', N''Donanım'', 0.0, 0.0, N''YK-0004'', 0.0, 1, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''yazarkasa-pos.html''),
    (5, N''2000000000053'', N''inPOS'', 1, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''m530'', N''inPOS m530 Mobil POS'', N''Donanım'', 0.0, 0.0, N''YK-0005'', 0.0, 1, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''yazarkasa-pos.html''),
    (6, N''2000000000060'', N''CAS'', 2, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''CL3000'', N''CAS CL3000 Market Terazisi'', N''Donanım'', 0.0, 0.0, N''TR-0001'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''teraziler.html''),
    (7, N''2000000000077'', N''CAS'', 2, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''CL8000'', N''CAS CL8000 Dokunmatik Terazi'', N''Donanım'', 0.0, 0.0, N''TR-0002'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''teraziler.html''),
    (8, N''2000000000084'', N''CAS'', 2, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''CN-1'', N''CAS CN-1 Sistem Terazisi'', N''Donanım'', 0.0, 0.0, N''TR-0003'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''teraziler.html''),
    (9, N''2000000000091'', N''Digi'', 2, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''SM100P'', N''Digi SM100P Boyunlu Terazi'', N''Donanım'', 0.0, 0.0, N''TR-0004'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''teraziler.html''),
    (10, N''2000000000107'', N''Digi'', 2, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''SM-120T'', N''Digi SM-120T Dokunmatik Terazi'', N''Donanım'', 0.0, 0.0, N''TR-0005'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''teraziler.html''),
    (11, N''2000000000114'', N''CAS'', 2, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''ER-JR'', N''CAS ER-JR Masaüstü Terazi'', N''Donanım'', 0.0, 0.0, N''TR-0006'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''teraziler.html''),
    (12, N''2000000000121'', N''CAS'', 2, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''FW-500'', N''CAS FW-500 Su Geçirmez Terazi'', N''Donanım'', 0.0, 0.0, N''TR-0007'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''teraziler.html''),
    (13, N''2000000000138'', N''CAS'', 2, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''PDI'', N''CAS PDI Ankastre Kasa Terazisi'', N''Donanım'', 0.0, 0.0, N''TR-0008'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''teraziler.html''),
    (14, N''2000000000145'', N''Hillpos'', 3, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''HRS-28'', N''Hillpos HRS-28'', N''Donanım'', 0.0, 0.0, N''BO-0001'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''barkod-okuyucular.html''),
    (15, N''2000000000152'', N''Hillpos'', 3, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''HSC-82'', N''Hillpos HSC-82'', N''Donanım'', 0.0, 0.0, N''BO-0002'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''barkod-okuyucular.html''),
    (16, N''2000000000169'', N''Hillpos'', 3, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''HSD-92'', N''Hillpos HSD-92'', N''Donanım'', 0.0, 0.0, N''BO-0003'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''barkod-okuyucular.html''),
    (17, N''2000000000176'', N''Newland'', 3, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''TP-13'', N''Newland TP-13'', N''Donanım'', 0.0, 0.0, N''BO-0004'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''barkod-okuyucular.html''),
    (18, N''2000000000183'', N''Newland'', 3, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''TP-14'', N''Newland TP-14'', N''Donanım'', 0.0, 0.0, N''BO-0005'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''barkod-okuyucular.html''),
    (19, N''2000000000190'', N''Hillpos'', 3, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''HS-6700'', N''Hillpos HS-6700'', N''Donanım'', 0.0, 0.0, N''BO-0006'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''barkod-okuyucular.html''),
    (20, N''2000000000206'', N''Hillpos'', 3, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''VS-6800'', N''Hillpos VS-6800'', N''Donanım'', 0.0, 0.0, N''BO-0007'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''barkod-okuyucular.html''),
    (21, N''2000000000213'', N''Argox'', 4, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''OS-214 Plus'', N''Argox OS-214 Plus Barkod Yazıcı'', N''Donanım'', 0.0, 0.0, N''YZ-0001'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''yazicilar.html''),
    (22, N''2000000000220'', N''Hillpos'', 4, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''HDT-400'', N''Hillpos HDT-400 Barkod Yazıcı'', N''Donanım'', 0.0, 0.0, N''YZ-0002'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''yazicilar.html''),
    (23, N''2000000000237'', N''Hillpos'', 4, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''HTT-440'', N''Hillpos HTT-440 Barkod Yazıcı'', N''Donanım'', 0.0, 0.0, N''YZ-0003'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''yazicilar.html''),
    (24, N''2000000000244'', N''TSC'', 4, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''TTP-244CE'', N''TSC TTP-244CE Barkod Yazıcı'', N''Donanım'', 0.0, 0.0, N''YZ-0004'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''yazicilar.html''),
    (25, N''2000000000251'', N''Xprinter'', 4, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''XP-470B'', N''Xprinter XP-470B Barkod Yazıcı'', N''Donanım'', 0.0, 0.0, N''YZ-0005'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''yazicilar.html''),
    (26, N''2000000000268'', N''Hillpos'', 4, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''H380'', N''Hillpos H380 Fiş Yazıcı'', N''Donanım'', 0.0, 0.0, N''YZ-0006'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''yazicilar.html''),
    (27, N''2000000000275'', N''Hillpos'', 4, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''Q800'', N''Hillpos Q800 Fiş Yazıcı'', N''Donanım'', 0.0, 0.0, N''YZ-0007'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''yazicilar.html''),
    (28, N''2000000000282'', N''Bixolon'', 4, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''SPP-R310'', N''Bixolon SPP-R310 Mobil Fiş Yazıcı'', N''Donanım'', 0.0, 0.0, N''YZ-0008'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''yazicilar.html''),
    (29, N''2000000000299'', N''Chainway'', 5, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''C61'', N''Chainway C61'', N''Donanım'', 0.0, 0.0, N''ET-0001'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''el-terminali.html''),
    (30, N''2000000000305'', N''Chainway'', 5, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''C66'', N''Chainway C66'', N''Donanım'', 0.0, 0.0, N''ET-0002'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''el-terminali.html''),
    (31, N''2000000000312'', N''Hillpos'', 5, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''C7X'', N''Hillpos C7X Tablet'', N''Donanım'', 0.0, 0.0, N''ET-0003'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''el-terminali.html''),
    (32, N''2000000000329'', N''Hillpos'', 5, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''CM550X'', N''Hillpos CM550X'', N''Donanım'', 0.0, 0.0, N''ET-0004'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''el-terminali.html''),
    (33, N''2000000000336'', N''Hillpos'', 5, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''HT42'', N''Hillpos HT42'', N''Donanım'', 0.0, 0.0, N''ET-0005'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''el-terminali.html''),
    (34, N''2000000000343'', N''Hillpos'', 5, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''HT42K'', N''Hillpos HT42K'', N''Donanım'', 0.0, 0.0, N''ET-0006'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''el-terminali.html''),
    (35, N''2000000000350'', N''Hillpos'', 5, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''HT44'', N''Hillpos HT44'', N''Donanım'', 0.0, 0.0, N''ET-0007'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''el-terminali.html''),
    (36, N''2000000000367'', N''Hillpos'', 6, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''Touch Pro 15'', N''Hillpos Touch Pro 15'', N''Donanım'', 0.0, 0.0, N''PC-0001'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''dokunmatik-pos.html''),
    (37, N''2000000000374'', N''Hillpos'', 6, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''All-in-One Dual POS'', N''Hillpos All-in-One Dual POS'', N''Donanım'', 0.0, 0.0, N''PC-0002'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''dokunmatik-pos.html''),
    (38, N''2000000000381'', N''Hillpos'', 6, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''Slim Touch 15'', N''Hillpos Slim Touch 15'', N''Donanım'', 0.0, 0.0, N''PC-0003'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''dokunmatik-pos.html''),
    (39, N''2000000000398'', N''Hillpos'', 6, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''Kiosk POS 21.5'', N''Hillpos Kiosk POS 21.5'', N''Donanım'', 0.0, 0.0, N''PC-0004'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''dokunmatik-pos.html''),
    (40, N''2000000000404'', NULL, 7, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, NULL, N''Özel ERP & CRM Yazılımları'', N''Yazılım'', 0.0, 0.0, N''YW-0001'', 0.0, 3, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N''Adet'', NULL, N''kurumsal-yazilim.html''),
    (41, N''2000000000411'', NULL, 7, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, NULL, N''Stok ve Depo Yönetimi Yazılımı'', N''Yazılım'', 0.0, 0.0, N''YW-0002'', 0.0, 3, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N''Adet'', NULL, N''kurumsal-yazilim.html''),
    (42, N''2000000000428'', NULL, 7, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, NULL, N''Fabrika ve Üretim Takibi Yazılımı'', N''Yazılım'', 0.0, 0.0, N''YW-0003'', 0.0, 3, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N''Adet'', NULL, N''kurumsal-yazilim.html'');
    INSERT INTO [Products] ([Id], [Barcode], [Brand], [CategoryId], [CreatedAtUtc], [Description], [ImagePath], [IsActive], [MinimumStockQuantity], [Model], [Name], [ProductType], [PurchasePrice], [SalePrice], [StockCode], [StockQuantity], [TaxRateId], [TrackLots], [TrackSerialNumbers], [TrackStock], [Unit], [UpdatedAtUtc], [WebsitePath])
    VALUES (43, N''2000000000435'', NULL, 7, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, NULL, N''API ve Donanım Entegrasyonları'', N''Yazılım'', 0.0, 0.0, N''YW-0004'', 0.0, 3, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N''Adet'', NULL, N''kurumsal-yazilim.html''),
    (44, N''2000000000442'', NULL, 7, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, NULL, N''GİB & E-Fatura Çözümleri'', N''Yazılım'', 0.0, 0.0, N''YW-0005'', 0.0, 3, CAST(0 AS bit), CAST(0 AS bit), CAST(0 AS bit), N''Adet'', NULL, N''kurumsal-yazilim.html''),
    (45, N''2000000000459'', N''Genel'', 8, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''Metal Kasa'', N''Para Çekmecesi'', N''Donanım'', 0.0, 0.0, N''PE-0001'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''index.html''),
    (46, N''2000000000466'', N''Genel'', 8, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''Fiyat Sorgulama Terminali'', N''Fiyat Gör Cihazı'', N''Donanım'', 0.0, 0.0, N''PE-0002'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''index.html''),
    (47, N''2000000000473'', N''Genel'', 8, ''2026-07-27T00:00:00.0000000Z'', NULL, NULL, CAST(1 AS bit), 0.0, N''Mobil Fiş Yazıcı'', N''Mobil Yazıcı'', N''Donanım'', 0.0, 0.0, N''PE-0003'', 0.0, 2, CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), N''Adet'', NULL, N''index.html'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Barcode', N'Brand', N'CategoryId', N'CreatedAtUtc', N'Description', N'ImagePath', N'IsActive', N'MinimumStockQuantity', N'Model', N'Name', N'ProductType', N'PurchasePrice', N'SalePrice', N'StockCode', N'StockQuantity', N'TaxRateId', N'TrackLots', N'TrackSerialNumbers', N'TrackStock', N'Unit', N'UpdatedAtUtc', N'WebsitePath') AND [object_id] = OBJECT_ID(N'[Products]'))
        SET IDENTITY_INSERT [Products] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BranchId', N'Code', N'CreatedAtUtc', N'IsActive', N'IsDefault', N'Name', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Warehouses]'))
        SET IDENTITY_INSERT [Warehouses] ON;
    EXEC(N'INSERT INTO [Warehouses] ([Id], [BranchId], [Code], [CreatedAtUtc], [IsActive], [IsDefault], [Name], [UpdatedAtUtc])
    VALUES (1, 1, N''MERKEZ'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), N''Merkez Depo'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BranchId', N'Code', N'CreatedAtUtc', N'IsActive', N'IsDefault', N'Name', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[Warehouses]'))
        SET IDENTITY_INSERT [Warehouses] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Barcode', N'BarcodeType', N'CreatedAtUtc', N'IsActive', N'IsPrimary', N'ProductId', N'ProductVariantId', N'UnitMultiplier', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[ProductBarcodes]'))
        SET IDENTITY_INSERT [ProductBarcodes] ON;
    EXEC(N'INSERT INTO [ProductBarcodes] ([Id], [Barcode], [BarcodeType], [CreatedAtUtc], [IsActive], [IsPrimary], [ProductId], [ProductVariantId], [UnitMultiplier], [UpdatedAtUtc])
    VALUES (1, N''2000000000015'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 1, NULL, 1.0, NULL),
    (2, N''2000000000022'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 2, NULL, 1.0, NULL),
    (3, N''2000000000039'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 3, NULL, 1.0, NULL),
    (4, N''2000000000046'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 4, NULL, 1.0, NULL),
    (5, N''2000000000053'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 5, NULL, 1.0, NULL),
    (6, N''2000000000060'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 6, NULL, 1.0, NULL),
    (7, N''2000000000077'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 7, NULL, 1.0, NULL),
    (8, N''2000000000084'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 8, NULL, 1.0, NULL),
    (9, N''2000000000091'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 9, NULL, 1.0, NULL),
    (10, N''2000000000107'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 10, NULL, 1.0, NULL),
    (11, N''2000000000114'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 11, NULL, 1.0, NULL),
    (12, N''2000000000121'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 12, NULL, 1.0, NULL),
    (13, N''2000000000138'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 13, NULL, 1.0, NULL),
    (14, N''2000000000145'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 14, NULL, 1.0, NULL),
    (15, N''2000000000152'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 15, NULL, 1.0, NULL),
    (16, N''2000000000169'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 16, NULL, 1.0, NULL),
    (17, N''2000000000176'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 17, NULL, 1.0, NULL),
    (18, N''2000000000183'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 18, NULL, 1.0, NULL),
    (19, N''2000000000190'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 19, NULL, 1.0, NULL),
    (20, N''2000000000206'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 20, NULL, 1.0, NULL),
    (21, N''2000000000213'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 21, NULL, 1.0, NULL),
    (22, N''2000000000220'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 22, NULL, 1.0, NULL),
    (23, N''2000000000237'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 23, NULL, 1.0, NULL),
    (24, N''2000000000244'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 24, NULL, 1.0, NULL),
    (25, N''2000000000251'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 25, NULL, 1.0, NULL),
    (26, N''2000000000268'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 26, NULL, 1.0, NULL),
    (27, N''2000000000275'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 27, NULL, 1.0, NULL),
    (28, N''2000000000282'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 28, NULL, 1.0, NULL),
    (29, N''2000000000299'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 29, NULL, 1.0, NULL),
    (30, N''2000000000305'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 30, NULL, 1.0, NULL),
    (31, N''2000000000312'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 31, NULL, 1.0, NULL),
    (32, N''2000000000329'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 32, NULL, 1.0, NULL),
    (33, N''2000000000336'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 33, NULL, 1.0, NULL),
    (34, N''2000000000343'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 34, NULL, 1.0, NULL),
    (35, N''2000000000350'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 35, NULL, 1.0, NULL),
    (36, N''2000000000367'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 36, NULL, 1.0, NULL),
    (37, N''2000000000374'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 37, NULL, 1.0, NULL),
    (38, N''2000000000381'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 38, NULL, 1.0, NULL),
    (39, N''2000000000398'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 39, NULL, 1.0, NULL),
    (40, N''2000000000404'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 40, NULL, 1.0, NULL),
    (41, N''2000000000411'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 41, NULL, 1.0, NULL),
    (42, N''2000000000428'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 42, NULL, 1.0, NULL);
    INSERT INTO [ProductBarcodes] ([Id], [Barcode], [BarcodeType], [CreatedAtUtc], [IsActive], [IsPrimary], [ProductId], [ProductVariantId], [UnitMultiplier], [UpdatedAtUtc])
    VALUES (43, N''2000000000435'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 43, NULL, 1.0, NULL),
    (44, N''2000000000442'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 44, NULL, 1.0, NULL),
    (45, N''2000000000459'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 45, NULL, 1.0, NULL),
    (46, N''2000000000466'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 46, NULL, 1.0, NULL),
    (47, N''2000000000473'', N''EAN13'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), CAST(1 AS bit), 47, NULL, 1.0, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Barcode', N'BarcodeType', N'CreatedAtUtc', N'IsActive', N'IsPrimary', N'ProductId', N'ProductVariantId', N'UnitMultiplier', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[ProductBarcodes]'))
        SET IDENTITY_INSERT [ProductBarcodes] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityName_EntityId] ON [AuditLogs] ([EntityName], [EntityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AuditLogs_RecordId] ON [AuditLogs] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserId_CreatedAtUtc] ON [AuditLogs] ([UserId], [CreatedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Branches_Code] ON [Branches] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Branches_RecordId] ON [Branches] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BusinessOrderLines_BusinessOrderId_LineNumber] ON [BusinessOrderLines] ([BusinessOrderId], [LineNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_BusinessOrderLines_ProductId] ON [BusinessOrderLines] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_BusinessOrderLines_ProductVariantId] ON [BusinessOrderLines] ([ProductVariantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BusinessOrderLines_RecordId] ON [BusinessOrderLines] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_BusinessOrders_CustomerId_OrderDateUtc] ON [BusinessOrders] ([CustomerId], [OrderDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BusinessOrders_OrderType_OrderNumber] ON [BusinessOrders] ([OrderType], [OrderNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_BusinessOrders_QuoteId] ON [BusinessOrders] ([QuoteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BusinessOrders_RecordId] ON [BusinessOrders] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BusinessProjects_Code] ON [BusinessProjects] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BusinessProjects_RecordId] ON [BusinessProjects] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CompanySettings_RecordId] ON [CompanySettings] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CostCenters_Code] ON [CostCenters] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CostCenters_RecordId] ON [CostCenters] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Currencies_Code] ON [Currencies] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Currencies_RecordId] ON [Currencies] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_CurrentAccountTransactions_CustomerId_TransactionDateUtc] ON [CurrentAccountTransactions] ([CustomerId], [TransactionDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_CurrentAccountTransactions_DocumentNumber] ON [CurrentAccountTransactions] ([DocumentNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_CurrentAccountTransactions_InvoiceId] ON [CurrentAccountTransactions] ([InvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_CurrentAccountTransactions_QuoteId] ON [CurrentAccountTransactions] ([QuoteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CurrentAccountTransactions_RecordId] ON [CurrentAccountTransactions] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_CustomerAddresses_CustomerId_AddressType] ON [CustomerAddresses] ([CustomerId], [AddressType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CustomerAddresses_RecordId] ON [CustomerAddresses] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_CustomerContacts_CustomerId] ON [CustomerContacts] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CustomerContacts_RecordId] ON [CustomerContacts] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Customers_Code] ON [Customers] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Customers_Name] ON [Customers] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Customers_RecordId] ON [Customers] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Customers_TaxNumber] ON [Customers] ([TaxNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DispatchNoteLines_DispatchNoteId_LineNumber] ON [DispatchNoteLines] ([DispatchNoteId], [LineNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_DispatchNoteLines_ProductId] ON [DispatchNoteLines] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_DispatchNoteLines_ProductVariantId] ON [DispatchNoteLines] ([ProductVariantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DispatchNoteLines_RecordId] ON [DispatchNoteLines] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_DispatchNotes_BusinessOrderId] ON [DispatchNotes] ([BusinessOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_DispatchNotes_CustomerId_DispatchDateUtc] ON [DispatchNotes] ([CustomerId], [DispatchDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DispatchNotes_DispatchType_DispatchNumber] ON [DispatchNotes] ([DispatchType], [DispatchNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_DispatchNotes_InvoiceId] ON [DispatchNotes] ([InvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DispatchNotes_RecordId] ON [DispatchNotes] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_DispatchNotes_WarehouseId] ON [DispatchNotes] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExchangeRates_CurrencyId_RateDateUtc] ON [ExchangeRates] ([CurrencyId], [RateDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExchangeRates_RecordId] ON [ExchangeRates] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExpenseCategories_Code] ON [ExpenseCategories] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExpenseCategories_RecordId] ON [ExpenseCategories] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Expenses_BusinessProjectId] ON [Expenses] ([BusinessProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Expenses_CostCenterId] ON [Expenses] ([CostCenterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Expenses_CustomerId] ON [Expenses] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Expenses_DocumentNumber] ON [Expenses] ([DocumentNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Expenses_ExpenseCategoryId_ExpenseDateUtc] ON [Expenses] ([ExpenseCategoryId], [ExpenseDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Expenses_FinancialAccountId] ON [Expenses] ([FinancialAccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Expenses_RecordId] ON [Expenses] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Expenses_TaxRateId] ON [Expenses] ([TaxRateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_ExternalRecordMappings_EntityType_InternalId] ON [ExternalRecordMappings] ([EntityType], [InternalId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExternalRecordMappings_RecordId] ON [ExternalRecordMappings] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExternalRecordMappings_SourceSystem_EntityType_ExternalId] ON [ExternalRecordMappings] ([SourceSystem], [EntityType], [ExternalId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FinancialAccounts_Code] ON [FinancialAccounts] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_FinancialAccounts_Iban] ON [FinancialAccounts] ([Iban]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FinancialAccounts_RecordId] ON [FinancialAccounts] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_FinancialTransactions_CurrentAccountTransactionId] ON [FinancialTransactions] ([CurrentAccountTransactionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_FinancialTransactions_CustomerId] ON [FinancialTransactions] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_FinancialTransactions_DocumentNumber] ON [FinancialTransactions] ([DocumentNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_FinancialTransactions_FinancialAccountId_TransactionDateUtc] ON [FinancialTransactions] ([FinancialAccountId], [TransactionDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FinancialTransactions_RecordId] ON [FinancialTransactions] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_IntegrationOutboxMessages_ProcessedAtUtc_OccurredAtUtc] ON [IntegrationOutboxMessages] ([ProcessedAtUtc], [OccurredAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_IntegrationOutboxMessages_RecordId] ON [IntegrationOutboxMessages] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_InventoryCountLines_InventoryCountId_ProductId_ProductVariantId] ON [InventoryCountLines] ([InventoryCountId], [ProductId], [ProductVariantId]) WHERE [ProductVariantId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_InventoryCountLines_ProductId] ON [InventoryCountLines] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_InventoryCountLines_ProductVariantId] ON [InventoryCountLines] ([ProductVariantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InventoryCountLines_RecordId] ON [InventoryCountLines] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InventoryCounts_CountNumber] ON [InventoryCounts] ([CountNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InventoryCounts_RecordId] ON [InventoryCounts] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_InventoryCounts_WarehouseId_CountDateUtc] ON [InventoryCounts] ([WarehouseId], [CountDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InventorySettings_RecordId] ON [InventorySettings] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InvoiceLines_InvoiceId_LineNumber] ON [InvoiceLines] ([InvoiceId], [LineNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_InvoiceLines_ProductId] ON [InvoiceLines] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_InvoiceLines_ProductVariantId] ON [InvoiceLines] ([ProductVariantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InvoiceLines_RecordId] ON [InvoiceLines] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_InvoicePaymentSchedules_DueDateUtc] ON [InvoicePaymentSchedules] ([DueDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InvoicePaymentSchedules_InvoiceId_InstallmentNumber] ON [InvoicePaymentSchedules] ([InvoiceId], [InstallmentNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InvoicePaymentSchedules_RecordId] ON [InvoicePaymentSchedules] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Invoices_BusinessProjectId] ON [Invoices] ([BusinessProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Invoices_CostCenterId] ON [Invoices] ([CostCenterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Invoices_CustomerId_InvoiceDateUtc] ON [Invoices] ([CustomerId], [InvoiceDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Invoices_InvoiceType_InvoiceNumber] ON [Invoices] ([InvoiceType], [InvoiceNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Invoices_InvoiceType_Status_InvoiceDateUtc] ON [Invoices] ([InvoiceType], [Status], [InvoiceDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Invoices_PurchasePriceListId] ON [Invoices] ([PurchasePriceListId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Invoices_QuoteId] ON [Invoices] ([QuoteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Invoices_RecordId] ON [Invoices] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Invoices_WarehouseId] ON [Invoices] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_NegotiableInstruments_CustomerId_DueDateUtc] ON [NegotiableInstruments] ([CustomerId], [DueDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_NegotiableInstruments_FinancialAccountId] ON [NegotiableInstruments] ([FinancialAccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NegotiableInstruments_InstrumentType_InstrumentNumber] ON [NegotiableInstruments] ([InstrumentType], [InstrumentNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NegotiableInstruments_RecordId] ON [NegotiableInstruments] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_NegotiableInstruments_Status_DueDateUtc] ON [NegotiableInstruments] ([Status], [DueDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NumberSequences_Key] ON [NumberSequences] ([Key]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NumberSequences_RecordId] ON [NumberSequences] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_PaymentReceiptLines_CurrentAccountTransactionId] ON [PaymentReceiptLines] ([CurrentAccountTransactionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_PaymentReceiptLines_FinancialAccountId] ON [PaymentReceiptLines] ([FinancialAccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_PaymentReceiptLines_FinancialTransactionId] ON [PaymentReceiptLines] ([FinancialTransactionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PaymentReceiptLines_PaymentReceiptId_LineNumber] ON [PaymentReceiptLines] ([PaymentReceiptId], [LineNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PaymentReceiptLines_RecordId] ON [PaymentReceiptLines] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_PaymentReceipts_BusinessProjectId] ON [PaymentReceipts] ([BusinessProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_PaymentReceipts_CostCenterId] ON [PaymentReceipts] ([CostCenterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_PaymentReceipts_CustomerId_ReceiptDateUtc] ON [PaymentReceipts] ([CustomerId], [ReceiptDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PaymentReceipts_ReceiptType_ReceiptNumber] ON [PaymentReceipts] ([ReceiptType], [ReceiptNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PaymentReceipts_RecordId] ON [PaymentReceipts] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductBarcodes_Barcode] ON [ProductBarcodes] ([Barcode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_ProductBarcodes_ProductId_ProductVariantId] ON [ProductBarcodes] ([ProductId], [ProductVariantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_ProductBarcodes_ProductVariantId] ON [ProductBarcodes] ([ProductVariantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductBarcodes_RecordId] ON [ProductBarcodes] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductCategories_Code] ON [ProductCategories] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductCategories_RecordId] ON [ProductCategories] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductColors_Code] ON [ProductColors] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductColors_RecordId] ON [ProductColors] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_ProductImages_ProductId_DisplayOrder] ON [ProductImages] ([ProductId], [DisplayOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_ProductImages_ProductVariantId] ON [ProductImages] ([ProductVariantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductImages_RecordId] ON [ProductImages] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Products_Barcode] ON [Products] ([Barcode]) WHERE [Barcode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Products_RecordId] ON [Products] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Products_StockCode] ON [Products] ([StockCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Products_TaxRateId] ON [Products] ([TaxRateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_ProductSerialNumbers_ProductId_LotNumber_ExpirationDateUtc] ON [ProductSerialNumbers] ([ProductId], [LotNumber], [ExpirationDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_ProductSerialNumbers_ProductVariantId] ON [ProductSerialNumbers] ([ProductVariantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductSerialNumbers_RecordId] ON [ProductSerialNumbers] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductSerialNumbers_SerialNumber] ON [ProductSerialNumbers] ([SerialNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_ProductSerialNumbers_WarehouseId_IsInStock] ON [ProductSerialNumbers] ([WarehouseId], [IsInStock]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductUnitConversions_ProductId_UnitOfMeasureId] ON [ProductUnitConversions] ([ProductId], [UnitOfMeasureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductUnitConversions_RecordId] ON [ProductUnitConversions] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_ProductUnitConversions_UnitOfMeasureId] ON [ProductUnitConversions] ([UnitOfMeasureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_ProductVariants_ColorId] ON [ProductVariants] ([ColorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_ProductVariants_ProductId_ColorId] ON [ProductVariants] ([ProductId], [ColorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductVariants_RecordId] ON [ProductVariants] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductVariants_VariantCode] ON [ProductVariants] ([VariantCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_PurchasePriceListItems_ProductId] ON [PurchasePriceListItems] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PurchasePriceListItems_PurchasePriceListId_ProductId_MinimumQuantity] ON [PurchasePriceListItems] ([PurchasePriceListId], [ProductId], [MinimumQuantity]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PurchasePriceListItems_RecordId] ON [PurchasePriceListItems] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PurchasePriceLists_Code] ON [PurchasePriceLists] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_PurchasePriceLists_CustomerId_ValidFromUtc_ValidUntilUtc] ON [PurchasePriceLists] ([CustomerId], [ValidFromUtc], [ValidUntilUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PurchasePriceLists_RecordId] ON [PurchasePriceLists] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_QuoteLines_ProductId] ON [QuoteLines] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_QuoteLines_QuoteId_LineNumber] ON [QuoteLines] ([QuoteId], [LineNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_QuoteLines_RecordId] ON [QuoteLines] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Quotes_CustomerId_QuoteDateUtc] ON [Quotes] ([CustomerId], [QuoteDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Quotes_QuoteNumber] ON [Quotes] ([QuoteNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Quotes_RecordId] ON [Quotes] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Quotes_Status] ON [Quotes] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_SalesPriceListItems_ProductId] ON [SalesPriceListItems] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_SalesPriceListItems_ProductVariantId] ON [SalesPriceListItems] ([ProductVariantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SalesPriceListItems_RecordId] ON [SalesPriceListItems] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_SalesPriceListItems_SalesPriceListId_ProductId_ProductVariantId_MinimumQuantity] ON [SalesPriceListItems] ([SalesPriceListId], [ProductId], [ProductVariantId], [MinimumQuantity]) WHERE [ProductVariantId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SalesPriceLists_Code] ON [SalesPriceLists] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_SalesPriceLists_CustomerId_ValidFromUtc_ValidUntilUtc] ON [SalesPriceLists] ([CustomerId], [ValidFromUtc], [ValidUntilUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SalesPriceLists_RecordId] ON [SalesPriceLists] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ScaleProductSettings_Prefix_PluCode] ON [ScaleProductSettings] ([Prefix], [PluCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ScaleProductSettings_ProductId] ON [ScaleProductSettings] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ScaleProductSettings_RecordId] ON [ScaleProductSettings] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockMovements_BusinessProjectId] ON [StockMovements] ([BusinessProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockMovements_CostCenterId] ON [StockMovements] ([CostCenterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockMovements_DocumentNumber] ON [StockMovements] ([DocumentNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockMovements_InventoryCountLineId] ON [StockMovements] ([InventoryCountLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockMovements_InvoiceLineId] ON [StockMovements] ([InvoiceLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockMovements_ProductId_WarehouseId_MovementDateUtc] ON [StockMovements] ([ProductId], [WarehouseId], [MovementDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockMovements_ProductVariantId] ON [StockMovements] ([ProductVariantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockMovements_RecordId] ON [StockMovements] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockMovements_StockSlipLineId] ON [StockMovements] ([StockSlipLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockMovements_StockTransferLineId] ON [StockMovements] ([StockTransferLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockMovements_WarehouseId] ON [StockMovements] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockReservations_ProductId_ProductVariantId_WarehouseId_Status] ON [StockReservations] ([ProductId], [ProductVariantId], [WarehouseId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockReservations_ProductVariantId] ON [StockReservations] ([ProductVariantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockReservations_QuoteLineId] ON [StockReservations] ([QuoteLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockReservations_RecordId] ON [StockReservations] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockReservations_Status_ReservedUntilUtc] ON [StockReservations] ([Status], [ReservedUntilUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockReservations_WarehouseId] ON [StockReservations] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockSlipLines_ProductId] ON [StockSlipLines] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockSlipLines_ProductVariantId] ON [StockSlipLines] ([ProductVariantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockSlipLines_RecordId] ON [StockSlipLines] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockSlipLines_StockSlipId_LineNumber] ON [StockSlipLines] ([StockSlipId], [LineNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockSlips_BusinessProjectId] ON [StockSlips] ([BusinessProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockSlips_CostCenterId] ON [StockSlips] ([CostCenterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockSlips_RecordId] ON [StockSlips] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockSlips_SlipNumber] ON [StockSlips] ([SlipNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockSlips_WarehouseId_SlipDateUtc] ON [StockSlips] ([WarehouseId], [SlipDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockTransferLines_ProductId] ON [StockTransferLines] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockTransferLines_ProductVariantId] ON [StockTransferLines] ([ProductVariantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockTransferLines_RecordId] ON [StockTransferLines] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockTransferLines_StockTransferId_LineNumber] ON [StockTransferLines] ([StockTransferId], [LineNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockTransfers_FromWarehouseId_TransferDateUtc] ON [StockTransfers] ([FromWarehouseId], [TransferDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockTransfers_RecordId] ON [StockTransfers] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_StockTransfers_ToWarehouseId_TransferDateUtc] ON [StockTransfers] ([ToWarehouseId], [TransferDateUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockTransfers_TransferNumber] ON [StockTransfers] ([TransferNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TaxRates_Code] ON [TaxRates] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TaxRates_RecordId] ON [TaxRates] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UnitsOfMeasure_Code] ON [UnitsOfMeasure] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UnitsOfMeasure_RecordId] ON [UnitsOfMeasure] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE INDEX [IX_Warehouses_BranchId_Name] ON [Warehouses] ([BranchId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Warehouses_Code] ON [Warehouses] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Warehouses_RecordId] ON [Warehouses] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727194647_InitialSqlServer2022'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727194647_InitialSqlServer2022', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentReceipts]') AND [c].[name] = N'IsApproved');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [PaymentReceipts] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [PaymentReceipts] DROP COLUMN [IsApproved];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [StockMovements] ADD [ReversalOfId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [PaymentReceipts] ADD [ApprovedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [PaymentReceipts] ADD [ApprovedByUserId] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [PaymentReceipts] ADD [CancellationReason] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [PaymentReceipts] ADD [CancelledAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [PaymentReceipts] ADD [CancelledByUserId] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [PaymentReceipts] ADD [Status] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [Invoices] ADD [ApprovedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [Invoices] ADD [ApprovedByUserId] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [Invoices] ADD [CancellationReason] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [Invoices] ADD [CancelledAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [Invoices] ADD [CancelledByUserId] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [FinancialTransactions] ADD [ReversalOfId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [CurrentAccountTransactions] ADD [ReversalOfId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    CREATE INDEX [IX_StockMovements_ReversalOfId] ON [StockMovements] ([ReversalOfId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    CREATE INDEX [IX_FinancialTransactions_ReversalOfId] ON [FinancialTransactions] ([ReversalOfId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    CREATE INDEX [IX_CurrentAccountTransactions_ReversalOfId] ON [CurrentAccountTransactions] ([ReversalOfId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [CurrentAccountTransactions] ADD CONSTRAINT [FK_CurrentAccountTransactions_CurrentAccountTransactions_ReversalOfId] FOREIGN KEY ([ReversalOfId]) REFERENCES [CurrentAccountTransactions] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [FinancialTransactions] ADD CONSTRAINT [FK_FinancialTransactions_FinancialTransactions_ReversalOfId] FOREIGN KEY ([ReversalOfId]) REFERENCES [FinancialTransactions] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    ALTER TABLE [StockMovements] ADD CONSTRAINT [FK_StockMovements_StockMovements_ReversalOfId] FOREIGN KEY ([ReversalOfId]) REFERENCES [StockMovements] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210203_AddDocumentCancellationAndReversalTracking'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727210203_AddDocumentCancellationAndReversalTracking', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210558_SeedInvoiceReceiptNumberSequencesAndDefaultFinancialAccount'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AccountType', N'BankName', N'BranchName', N'Code', N'CreatedAtUtc', N'CurrencyCode', N'Iban', N'IsActive', N'Name', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[FinancialAccounts]'))
        SET IDENTITY_INSERT [FinancialAccounts] ON;
    EXEC(N'INSERT INTO [FinancialAccounts] ([Id], [AccountType], [BankName], [BranchName], [Code], [CreatedAtUtc], [CurrencyCode], [Iban], [IsActive], [Name], [UpdatedAtUtc])
    VALUES (1, 1, NULL, NULL, N''KASA'', ''2026-07-27T00:00:00.0000000Z'', N''TRY'', NULL, CAST(1 AS bit), N''Merkez Kasa'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AccountType', N'BankName', N'BranchName', N'Code', N'CreatedAtUtc', N'CurrencyCode', N'Iban', N'IsActive', N'Name', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[FinancialAccounts]'))
        SET IDENTITY_INSERT [FinancialAccounts] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210558_SeedInvoiceReceiptNumberSequencesAndDefaultFinancialAccount'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
        SET IDENTITY_INSERT [NumberSequences] ON;
    EXEC(N'INSERT INTO [NumberSequences] ([Id], [CreatedAtUtc], [Key], [NextNumber], [Padding], [Prefix], [UpdatedAtUtc])
    VALUES (2, ''2026-07-27T00:00:00.0000000Z'', N''SALES_INVOICE'', CAST(1 AS bigint), 5, N''SF.'', NULL),
    (3, ''2026-07-27T00:00:00.0000000Z'', N''PURCHASE_INVOICE'', CAST(1 AS bigint), 5, N''AF.'', NULL),
    (4, ''2026-07-27T00:00:00.0000000Z'', N''COLLECTION_RECEIPT'', CAST(1 AS bigint), 5, N''TAH.'', NULL),
    (5, ''2026-07-27T00:00:00.0000000Z'', N''PAYMENT_RECEIPT'', CAST(1 AS bigint), 5, N''TED.'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
        SET IDENTITY_INSERT [NumberSequences] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727210558_SeedInvoiceReceiptNumberSequencesAndDefaultFinancialAccount'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727210558_SeedInvoiceReceiptNumberSequencesAndDefaultFinancialAccount', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727223327_SetCompanyLogoPath'
)
BEGIN
    EXEC(N'UPDATE [CompanySettings] SET [LogoPath] = N''/images/logo.png''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727223327_SetCompanyLogoPath'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727223327_SetCompanyLogoPath', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727231449_AddDispatchNoteApprovalAndStockLinkPlusNewSequences'
)
BEGIN
    ALTER TABLE [StockMovements] ADD [DispatchNoteLineId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727231449_AddDispatchNoteApprovalAndStockLinkPlusNewSequences'
)
BEGIN
    ALTER TABLE [DispatchNotes] ADD [ApprovedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727231449_AddDispatchNoteApprovalAndStockLinkPlusNewSequences'
)
BEGIN
    ALTER TABLE [DispatchNotes] ADD [ApprovedByUserId] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727231449_AddDispatchNoteApprovalAndStockLinkPlusNewSequences'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
        SET IDENTITY_INSERT [NumberSequences] ON;
    EXEC(N'INSERT INTO [NumberSequences] ([Id], [CreatedAtUtc], [Key], [NextNumber], [Padding], [Prefix], [UpdatedAtUtc])
    VALUES (6, ''2026-07-27T00:00:00.0000000Z'', N''STOCK_RECEIPT'', CAST(1 AS bigint), 5, N''SGF.'', NULL),
    (7, ''2026-07-27T00:00:00.0000000Z'', N''STOCK_ISSUE'', CAST(1 AS bigint), 5, N''SCF.'', NULL),
    (8, ''2026-07-27T00:00:00.0000000Z'', N''STOCK_COUNT'', CAST(1 AS bigint), 5, N''SAY.'', NULL),
    (9, ''2026-07-27T00:00:00.0000000Z'', N''SALES_DISPATCH'', CAST(1 AS bigint), 5, N''SIRS.'', NULL),
    (10, ''2026-07-27T00:00:00.0000000Z'', N''PURCHASE_DISPATCH'', CAST(1 AS bigint), 5, N''AIRS.'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
        SET IDENTITY_INSERT [NumberSequences] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727231449_AddDispatchNoteApprovalAndStockLinkPlusNewSequences'
)
BEGIN
    CREATE INDEX [IX_StockMovements_DispatchNoteLineId] ON [StockMovements] ([DispatchNoteLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727231449_AddDispatchNoteApprovalAndStockLinkPlusNewSequences'
)
BEGIN
    ALTER TABLE [StockMovements] ADD CONSTRAINT [FK_StockMovements_DispatchNoteLines_DispatchNoteLineId] FOREIGN KEY ([DispatchNoteLineId]) REFERENCES [DispatchNoteLines] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727231449_AddDispatchNoteApprovalAndStockLinkPlusNewSequences'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727231449_AddDispatchNoteApprovalAndStockLinkPlusNewSequences', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728073922_SeedStockTransferSequence'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
        SET IDENTITY_INSERT [NumberSequences] ON;
    EXEC(N'INSERT INTO [NumberSequences] ([Id], [CreatedAtUtc], [Key], [NextNumber], [Padding], [Prefix], [UpdatedAtUtc])
    VALUES (11, ''2026-07-27T00:00:00.0000000Z'', N''STOCK_TRANSFER'', CAST(1 AS bigint), 5, N''TRF.'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
        SET IDENTITY_INSERT [NumberSequences] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728073922_SeedStockTransferSequence'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728073922_SeedStockTransferSequence', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728075343_AddCancelSupportForStockDocuments'
)
BEGIN
    ALTER TABLE [StockTransfers] ADD [CancellationReason] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728075343_AddCancelSupportForStockDocuments'
)
BEGIN
    ALTER TABLE [StockTransfers] ADD [CancelledAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728075343_AddCancelSupportForStockDocuments'
)
BEGIN
    ALTER TABLE [StockTransfers] ADD [CancelledByUserId] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728075343_AddCancelSupportForStockDocuments'
)
BEGIN
    ALTER TABLE [StockSlips] ADD [CancellationReason] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728075343_AddCancelSupportForStockDocuments'
)
BEGIN
    ALTER TABLE [StockSlips] ADD [CancelledAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728075343_AddCancelSupportForStockDocuments'
)
BEGIN
    ALTER TABLE [StockSlips] ADD [CancelledByUserId] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728075343_AddCancelSupportForStockDocuments'
)
BEGIN
    ALTER TABLE [InventoryCounts] ADD [CancellationReason] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728075343_AddCancelSupportForStockDocuments'
)
BEGIN
    ALTER TABLE [InventoryCounts] ADD [CancelledAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728075343_AddCancelSupportForStockDocuments'
)
BEGIN
    ALTER TABLE [InventoryCounts] ADD [CancelledByUserId] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728075343_AddCancelSupportForStockDocuments'
)
BEGIN
    ALTER TABLE [DispatchNotes] ADD [CancellationReason] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728075343_AddCancelSupportForStockDocuments'
)
BEGIN
    ALTER TABLE [DispatchNotes] ADD [CancelledAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728075343_AddCancelSupportForStockDocuments'
)
BEGIN
    ALTER TABLE [DispatchNotes] ADD [CancelledByUserId] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728075343_AddCancelSupportForStockDocuments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728075343_AddCancelSupportForStockDocuments', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728080931_SeedExpenseNegotiableOrderQuoteSequences'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
        SET IDENTITY_INSERT [NumberSequences] ON;
    EXEC(N'INSERT INTO [NumberSequences] ([Id], [CreatedAtUtc], [Key], [NextNumber], [Padding], [Prefix], [UpdatedAtUtc])
    VALUES (12, ''2026-07-27T00:00:00.0000000Z'', N''EXPENSE'', CAST(1 AS bigint), 5, N''MAS.'', NULL),
    (13, ''2026-07-27T00:00:00.0000000Z'', N''NEGOTIABLE_CHEQUE'', CAST(1 AS bigint), 5, N''CEK.'', NULL),
    (14, ''2026-07-27T00:00:00.0000000Z'', N''NEGOTIABLE_NOTE'', CAST(1 AS bigint), 5, N''SEN.'', NULL),
    (15, ''2026-07-27T00:00:00.0000000Z'', N''SALES_ORDER'', CAST(1 AS bigint), 5, N''SSIP.'', NULL),
    (16, ''2026-07-27T00:00:00.0000000Z'', N''PURCHASE_ORDER'', CAST(1 AS bigint), 5, N''ASIP.'', NULL),
    (17, ''2026-07-27T00:00:00.0000000Z'', N''QUOTE'', CAST(1 AS bigint), 5, N''TEK.'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
        SET IDENTITY_INSERT [NumberSequences] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728080931_SeedExpenseNegotiableOrderQuoteSequences'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728080931_SeedExpenseNegotiableOrderQuoteSequences', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [Address] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [BankAccountNumber] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [BranchId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [BreakDurationMinutes] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [CommissionRate] decimal(5,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [Deduction] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [DeductionNote] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [DefaultFinancialAccountId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [HireDateUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [Iban] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [JobTitle] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [LicensePlate] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [PersonnelCode] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [Salary] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [TerminationDateUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_BranchId] ON [AspNetUsers] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_DefaultFinancialAccountId] ON [AspNetUsers] ([DefaultFinancialAccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_FinancialAccounts_DefaultFinancialAccountId] FOREIGN KEY ([DefaultFinancialAccountId]) REFERENCES [FinancialAccounts] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728110614_AddPersonnelProfileFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728110614_AddPersonnelProfileFields', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD [AlternateName] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD [CountryOfOrigin] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD [PricesIncludeTax] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD [ShelfLifeDays] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD [UnitOfMeasureId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [DefaultPriceListId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    CREATE TABLE [PriceLists] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PriceLists] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
        SET IDENTITY_INSERT [NumberSequences] ON;
    EXEC(N'INSERT INTO [NumberSequences] ([Id], [CreatedAtUtc], [Key], [NextNumber], [Padding], [Prefix], [UpdatedAtUtc])
    VALUES (18, ''2026-07-28T00:00:00.0000000Z'', N''PERSONNEL'', CAST(1 AS bigint), 3, N''PRSNL.'', NULL),
    (19, ''2026-07-28T00:00:00.0000000Z'', N''CUSTOMER'', CAST(1 AS bigint), 5, N''CARI.'', NULL),
    (20, ''2026-07-28T00:00:00.0000000Z'', N''FINANCIAL_ACCOUNT_CASH'', CAST(1 AS bigint), 3, N''KASA.'', NULL),
    (21, ''2026-07-28T00:00:00.0000000Z'', N''FINANCIAL_ACCOUNT_BANK'', CAST(1 AS bigint), 3, N''BANKA.'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
        SET IDENTITY_INSERT [NumberSequences] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'IsActive', N'Name', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[PriceLists]'))
        SET IDENTITY_INSERT [PriceLists] ON;
    EXEC(N'INSERT INTO [PriceLists] ([Id], [Code], [CreatedAtUtc], [IsActive], [Name], [UpdatedAtUtc])
    VALUES (1, N''MERKEZ'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), N''Merkez Fiyat'', NULL),
    (2, N''SUBE'', ''2026-07-27T00:00:00.0000000Z'', CAST(1 AS bit), N''Şube Fiyat'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'IsActive', N'Name', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[PriceLists]'))
        SET IDENTITY_INSERT [PriceLists] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 10;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 11;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 12;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 13;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 14;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 15;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 16;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 17;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 18;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 19;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 20;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 21;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 22;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 23;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 24;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 25;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 26;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 27;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 28;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 29;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 30;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 31;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 32;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 33;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 34;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 35;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 36;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 37;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 38;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 39;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 40;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 41;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 42;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 43;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 44;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 45;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 46;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [AlternateName] = NULL, [CountryOfOrigin] = NULL, [PricesIncludeTax] = CAST(0 AS bit), [ShelfLifeDays] = NULL, [UnitOfMeasureId] = NULL
    WHERE [Id] = 47;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'DecimalPlaces', N'IsActive', N'Name', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[UnitsOfMeasure]'))
        SET IDENTITY_INSERT [UnitsOfMeasure] ON;
    EXEC(N'INSERT INTO [UnitsOfMeasure] ([Id], [Code], [CreatedAtUtc], [DecimalPlaces], [IsActive], [Name], [UpdatedAtUtc])
    VALUES (4, N''OZEL'', ''2026-07-28T00:00:00.0000000Z'', 2, CAST(1 AS bit), N''Özel Fiyat'', NULL),
    (5, N''METRE'', ''2026-07-28T00:00:00.0000000Z'', 2, CAST(1 AS bit), N''Metre'', NULL),
    (6, N''KOLI'', ''2026-07-28T00:00:00.0000000Z'', 0, CAST(1 AS bit), N''Koli'', NULL),
    (7, N''LITRE'', ''2026-07-28T00:00:00.0000000Z'', 2, CAST(1 AS bit), N''Litre'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAtUtc', N'DecimalPlaces', N'IsActive', N'Name', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[UnitsOfMeasure]'))
        SET IDENTITY_INSERT [UnitsOfMeasure] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    CREATE INDEX [IX_Products_UnitOfMeasureId] ON [Products] ([UnitOfMeasureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_DefaultPriceListId] ON [AspNetUsers] ([DefaultPriceListId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PriceLists_Code] ON [PriceLists] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PriceLists_RecordId] ON [PriceLists] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_PriceLists_DefaultPriceListId] FOREIGN KEY ([DefaultPriceListId]) REFERENCES [PriceLists] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_UnitsOfMeasure_UnitOfMeasureId] FOREIGN KEY ([UnitOfMeasureId]) REFERENCES [UnitsOfMeasure] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728130156_AddPriceListsUnitsAndProductFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728130156_AddPriceListsUnitsAndProductFields', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728201711_AddNameIndexesToMasterData'
)
BEGIN
    CREATE INDEX [IX_UnitsOfMeasure_Name] ON [UnitsOfMeasure] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728201711_AddNameIndexesToMasterData'
)
BEGIN
    CREATE INDEX [IX_TaxRates_Name] ON [TaxRates] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728201711_AddNameIndexesToMasterData'
)
BEGIN
    CREATE INDEX [IX_Products_Name] ON [Products] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728201711_AddNameIndexesToMasterData'
)
BEGIN
    CREATE INDEX [IX_ProductCategories_Name] ON [ProductCategories] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728201711_AddNameIndexesToMasterData'
)
BEGIN
    CREATE INDEX [IX_PriceLists_Name] ON [PriceLists] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728201711_AddNameIndexesToMasterData'
)
BEGIN
    CREATE INDEX [IX_FinancialAccounts_Name] ON [FinancialAccounts] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728201711_AddNameIndexesToMasterData'
)
BEGIN
    CREATE INDEX [IX_ExpenseCategories_Name] ON [ExpenseCategories] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728201711_AddNameIndexesToMasterData'
)
BEGIN
    CREATE INDEX [IX_Branches_Name] ON [Branches] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728201711_AddNameIndexesToMasterData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728201711_AddNameIndexesToMasterData', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728224620_AddInvoiceMikroStyleFields'
)
BEGIN
    ALTER TABLE [Invoices] ADD [IsReturn] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728224620_AddInvoiceMikroStyleFields'
)
BEGIN
    ALTER TABLE [Invoices] ADD [PaymentTerm] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728224620_AddInvoiceMikroStyleFields'
)
BEGIN
    ALTER TABLE [Invoices] ADD [ReferenceNumber] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728224620_AddInvoiceMikroStyleFields'
)
BEGIN
    ALTER TABLE [Invoices] ADD [SalespersonUserId] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728224620_AddInvoiceMikroStyleFields'
)
BEGIN
    ALTER TABLE [Invoices] ADD [SettlementFinancialAccountId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728224620_AddInvoiceMikroStyleFields'
)
BEGIN
    ALTER TABLE [Invoices] ADD [TradeType] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728224620_AddInvoiceMikroStyleFields'
)
BEGIN
    CREATE INDEX [IX_Invoices_SettlementFinancialAccountId] ON [Invoices] ([SettlementFinancialAccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728224620_AddInvoiceMikroStyleFields'
)
BEGIN
    ALTER TABLE [Invoices] ADD CONSTRAINT [FK_Invoices_FinancialAccounts_SettlementFinancialAccountId] FOREIGN KEY ([SettlementFinancialAccountId]) REFERENCES [FinancialAccounts] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728224620_AddInvoiceMikroStyleFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728224620_AddInvoiceMikroStyleFields', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728232813_RemoveProductPricesIncludeTax'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'PricesIncludeTax');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [Products] DROP COLUMN [PricesIncludeTax];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260728232813_RemoveProductPricesIncludeTax'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260728232813_RemoveProductPricesIncludeTax', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729002054_AddQuoteAmountDiscountAndPriceHistory'
)
BEGIN
    DROP INDEX [IX_SalesPriceListItems_SalesPriceListId_ProductId_ProductVariantId_MinimumQuantity] ON [SalesPriceListItems];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729002054_AddQuoteAmountDiscountAndPriceHistory'
)
BEGIN
    ALTER TABLE [Quotes] DROP CONSTRAINT [CK_Quotes_Totals];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729002054_AddQuoteAmountDiscountAndPriceHistory'
)
BEGIN
    ALTER TABLE [Quotes] ADD [AmountDiscount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729002054_AddQuoteAmountDiscountAndPriceHistory'
)
BEGIN
    CREATE INDEX [IX_SalesPriceListItems_SalesPriceListId_ProductId_ProductVariantId_MinimumQuantity] ON [SalesPriceListItems] ([SalesPriceListId], [ProductId], [ProductVariantId], [MinimumQuantity]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729002054_AddQuoteAmountDiscountAndPriceHistory'
)
BEGIN
    EXEC(N'ALTER TABLE [Quotes] ADD CONSTRAINT [CK_Quotes_Totals] CHECK ([Subtotal] >= 0 AND [DiscountTotal] >= 0 AND [AmountDiscount] >= 0 AND [TaxTotal] >= 0 AND [GrandTotal] >= 0)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729002054_AddQuoteAmountDiscountAndPriceHistory'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260729002054_AddQuoteAmountDiscountAndPriceHistory', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729010048_AddInvoiceAmountDiscount'
)
BEGIN
    ALTER TABLE [Invoices] ADD [AmountDiscount] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260729010048_AddInvoiceAmountDiscount'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260729010048_AddInvoiceAmountDiscount', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731074102_AddCustomerProfileFields'
)
BEGIN
    ALTER TABLE [Customers] ADD [AccountType] int NOT NULL DEFAULT 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731074102_AddCustomerProfileFields'
)
BEGIN
    ALTER TABLE [Customers] ADD [AuthorizedPerson] nvarchar(150) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731074102_AddCustomerProfileFields'
)
BEGIN
    ALTER TABLE [Customers] ADD [CustomerGroup] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731074102_AddCustomerProfileFields'
)
BEGIN
    ALTER TABLE [Customers] ADD [DefaultPaymentTermDays] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731074102_AddCustomerProfileFields'
)
BEGIN
    ALTER TABLE [Customers] ADD [RiskLimit] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731074102_AddCustomerProfileFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260731074102_AddCustomerProfileFields', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731213115_WidenNumberSequenceKeyForPerSeriesCounters'
)
BEGIN
    DROP INDEX [IX_NumberSequences_Key] ON [NumberSequences];
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[NumberSequences]') AND [c].[name] = N'Key');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [NumberSequences] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [NumberSequences] ALTER COLUMN [Key] nvarchar(60) NOT NULL;
    CREATE UNIQUE INDEX [IX_NumberSequences_Key] ON [NumberSequences] ([Key]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731213115_WidenNumberSequenceKeyForPerSeriesCounters'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260731213115_WidenNumberSequenceKeyForPerSeriesCounters', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731222401_AddKapaliFaturaOtomatikTahsilat'
)
BEGIN
    ALTER TABLE [PaymentReceipts] ADD [InvoiceId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731222401_AddKapaliFaturaOtomatikTahsilat'
)
BEGIN
    ALTER TABLE [Invoices] ADD [IsClosedInvoice] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731222401_AddKapaliFaturaOtomatikTahsilat'
)
BEGIN
    ALTER TABLE [Invoices] ADD [SettlementPaymentMethod] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731222401_AddKapaliFaturaOtomatikTahsilat'
)
BEGIN
    CREATE INDEX [IX_PaymentReceipts_InvoiceId] ON [PaymentReceipts] ([InvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731222401_AddKapaliFaturaOtomatikTahsilat'
)
BEGIN
    ALTER TABLE [PaymentReceipts] ADD CONSTRAINT [FK_PaymentReceipts_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260731222401_AddKapaliFaturaOtomatikTahsilat'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260731222401_AddKapaliFaturaOtomatikTahsilat', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    ALTER TABLE [Products] ADD [KitchenPrinterName] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    ALTER TABLE [Products] ADD [LoyaltyPoints] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    ALTER TABLE [Products] ADD [ShowAsShortcut] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    ALTER TABLE [Products] ADD [ShowInMobile] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    ALTER TABLE [Products] ADD [ShowInOnlineOrder] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    ALTER TABLE [InventorySettings] ADD [IsRestaurantModuleEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [InventorySettings] SET [IsRestaurantModuleEnabled] = CAST(0 AS bit)
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 10;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 11;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 12;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 13;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 14;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 15;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 16;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 17;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 18;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 19;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 20;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 21;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 22;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 23;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 24;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 25;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 26;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 27;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 28;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 29;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 30;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 31;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 32;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 33;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 34;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 35;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 36;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 37;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 38;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 39;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 40;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 41;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 42;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 43;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 44;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 45;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 46;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [KitchenPrinterName] = NULL, [LoyaltyPoints] = 0, [ShowAsShortcut] = CAST(1 AS bit), [ShowInMobile] = CAST(1 AS bit), [ShowInOnlineOrder] = CAST(1 AS bit)
    WHERE [Id] = 47;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801191941_AddRestoranModuluVeKdvSelect'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801191941_AddRestoranModuluVeKdvSelect', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801213827_AddUniqueIndexExpenseDocumentNumber'
)
BEGIN
    DROP INDEX [IX_Expenses_DocumentNumber] ON [Expenses];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801213827_AddUniqueIndexExpenseDocumentNumber'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Expenses_DocumentNumber] ON [Expenses] ([DocumentNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801213827_AddUniqueIndexExpenseDocumentNumber'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801213827_AddUniqueIndexExpenseDocumentNumber', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801220442_AddInvoiceSubmissionKey'
)
BEGIN
    ALTER TABLE [Invoices] ADD [SubmissionKey] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801220442_AddInvoiceSubmissionKey'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Invoices_SubmissionKey] ON [Invoices] ([SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260801220442_AddInvoiceSubmissionKey'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801220442_AddInvoiceSubmissionKey', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    ALTER TABLE [StockTransfers] ADD [SubmissionKey] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    ALTER TABLE [StockSlips] ADD [SubmissionKey] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    ALTER TABLE [PaymentReceipts] ADD [SubmissionKey] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    ALTER TABLE [NegotiableInstruments] ADD [CreatedByUserId] nvarchar(450) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    ALTER TABLE [NegotiableInstruments] ADD [SubmissionKey] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    ALTER TABLE [InventoryCounts] ADD [SubmissionKey] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    ALTER TABLE [Expenses] ADD [CreatedByUserId] nvarchar(450) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    ALTER TABLE [Expenses] ADD [SubmissionKey] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    ALTER TABLE [DispatchNotes] ADD [SubmissionKey] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    ALTER TABLE [BusinessOrders] ADD [SubmissionKey] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_StockTransfers_CreatedByUserId_SubmissionKey] ON [StockTransfers] ([CreatedByUserId], [SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_StockSlips_CreatedByUserId_SubmissionKey] ON [StockSlips] ([CreatedByUserId], [SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PaymentReceipts_CreatedByUserId_SubmissionKey] ON [PaymentReceipts] ([CreatedByUserId], [SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_NegotiableInstruments_CreatedByUserId_SubmissionKey] ON [NegotiableInstruments] ([CreatedByUserId], [SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_InventoryCounts_CreatedByUserId_SubmissionKey] ON [InventoryCounts] ([CreatedByUserId], [SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Expenses_CreatedByUserId_SubmissionKey] ON [Expenses] ([CreatedByUserId], [SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_DispatchNotes_CreatedByUserId_SubmissionKey] ON [DispatchNotes] ([CreatedByUserId], [SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_BusinessOrders_CreatedByUserId_SubmissionKey] ON [BusinessOrders] ([CreatedByUserId], [SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802082011_AddSubmissionKeyToAllEvrakTypes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260802082011_AddSubmissionKeyToAllEvrakTypes', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802094636_AddBarcodeGeneratorNumberSequences'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
        SET IDENTITY_INSERT [NumberSequences] ON;
    EXEC(N'INSERT INTO [NumberSequences] ([Id], [CreatedAtUtc], [Key], [NextNumber], [Padding], [Prefix], [UpdatedAtUtc])
    VALUES (1001, ''2026-08-02T00:00:00.0000000Z'', N''BARCODE_EAN13'', CAST(1 AS bigint), 8, N''1989'', NULL),
    (1002, ''2026-08-02T00:00:00.0000000Z'', N''BARCODE_EAN8'', CAST(1 AS bigint), 3, N''1989'', NULL),
    (1003, ''2026-08-02T00:00:00.0000000Z'', N''BARCODE_ASCII'', CAST(1 AS bigint), 6, N''AS'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAtUtc', N'Key', N'NextNumber', N'Padding', N'Prefix', N'UpdatedAtUtc') AND [object_id] = OBJECT_ID(N'[NumberSequences]'))
        SET IDENTITY_INSERT [NumberSequences] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802094636_AddBarcodeGeneratorNumberSequences'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260802094636_AddBarcodeGeneratorNumberSequences', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802114205_AddCustomerSubmissionKey'
)
BEGIN
    ALTER TABLE [Customers] ADD [CreatedByUserId] nvarchar(450) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802114205_AddCustomerSubmissionKey'
)
BEGIN
    ALTER TABLE [Customers] ADD [SubmissionKey] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802114205_AddCustomerSubmissionKey'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Customers_CreatedByUserId_SubmissionKey] ON [Customers] ([CreatedByUserId], [SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802114205_AddCustomerSubmissionKey'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260802114205_AddCustomerSubmissionKey', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802132212_AddOrderDispatchInvoiceConversionLinks'
)
BEGIN
    ALTER TABLE [InvoiceLines] ADD [BusinessOrderLineId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802132212_AddOrderDispatchInvoiceConversionLinks'
)
BEGIN
    ALTER TABLE [InvoiceLines] ADD [DispatchNoteLineId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802132212_AddOrderDispatchInvoiceConversionLinks'
)
BEGIN
    ALTER TABLE [DispatchNoteLines] ADD [BusinessOrderLineId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802132212_AddOrderDispatchInvoiceConversionLinks'
)
BEGIN
    ALTER TABLE [DispatchNoteLines] ADD [InvoicedQuantity] decimal(18,3) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802132212_AddOrderDispatchInvoiceConversionLinks'
)
BEGIN
    CREATE INDEX [IX_InvoiceLines_BusinessOrderLineId] ON [InvoiceLines] ([BusinessOrderLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802132212_AddOrderDispatchInvoiceConversionLinks'
)
BEGIN
    CREATE INDEX [IX_InvoiceLines_DispatchNoteLineId] ON [InvoiceLines] ([DispatchNoteLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802132212_AddOrderDispatchInvoiceConversionLinks'
)
BEGIN
    CREATE INDEX [IX_DispatchNoteLines_BusinessOrderLineId] ON [DispatchNoteLines] ([BusinessOrderLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802132212_AddOrderDispatchInvoiceConversionLinks'
)
BEGIN
    EXEC(N'ALTER TABLE [DispatchNoteLines] ADD CONSTRAINT [CK_DispatchNoteLines_InvoicedQuantity] CHECK ([InvoicedQuantity] >= 0 AND [InvoicedQuantity] <= [Quantity])');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802132212_AddOrderDispatchInvoiceConversionLinks'
)
BEGIN
    ALTER TABLE [DispatchNoteLines] ADD CONSTRAINT [FK_DispatchNoteLines_BusinessOrderLines_BusinessOrderLineId] FOREIGN KEY ([BusinessOrderLineId]) REFERENCES [BusinessOrderLines] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802132212_AddOrderDispatchInvoiceConversionLinks'
)
BEGIN
    ALTER TABLE [InvoiceLines] ADD CONSTRAINT [FK_InvoiceLines_BusinessOrderLines_BusinessOrderLineId] FOREIGN KEY ([BusinessOrderLineId]) REFERENCES [BusinessOrderLines] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802132212_AddOrderDispatchInvoiceConversionLinks'
)
BEGIN
    ALTER TABLE [InvoiceLines] ADD CONSTRAINT [FK_InvoiceLines_DispatchNoteLines_DispatchNoteLineId] FOREIGN KEY ([DispatchNoteLineId]) REFERENCES [DispatchNoteLines] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802132212_AddOrderDispatchInvoiceConversionLinks'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260802132212_AddOrderDispatchInvoiceConversionLinks', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802144613_AddConversionAutoApprovalSettings'
)
BEGIN
    ALTER TABLE [InventorySettings] ADD [DispatchToInvoicePurchaseAutoApprove] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802144613_AddConversionAutoApprovalSettings'
)
BEGIN
    ALTER TABLE [InventorySettings] ADD [DispatchToInvoiceSalesAutoApprove] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802144613_AddConversionAutoApprovalSettings'
)
BEGIN
    ALTER TABLE [InventorySettings] ADD [OrderToDispatchPurchaseAutoApprove] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802144613_AddConversionAutoApprovalSettings'
)
BEGIN
    ALTER TABLE [InventorySettings] ADD [OrderToDispatchSalesAutoApprove] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802144613_AddConversionAutoApprovalSettings'
)
BEGIN
    ALTER TABLE [InventorySettings] ADD [OrderToInvoicePurchaseAutoApprove] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802144613_AddConversionAutoApprovalSettings'
)
BEGIN
    ALTER TABLE [InventorySettings] ADD [OrderToInvoiceSalesAutoApprove] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260802144613_AddConversionAutoApprovalSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260802144613_AddConversionAutoApprovalSettings', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    ALTER TABLE [NegotiableInstruments] ADD [EndorsedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    ALTER TABLE [NegotiableInstruments] ADD [EndorsedToCustomerId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    ALTER TABLE [NegotiableInstruments] ADD [SettledAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    ALTER TABLE [NegotiableInstruments] ADD [SettlementFinancialAccountId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    ALTER TABLE [FinancialTransactions] ADD [NegotiableInstrumentId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    ALTER TABLE [CurrentAccountTransactions] ADD [NegotiableInstrumentId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    CREATE INDEX [IX_NegotiableInstruments_EndorsedToCustomerId] ON [NegotiableInstruments] ([EndorsedToCustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    CREATE INDEX [IX_NegotiableInstruments_SettlementFinancialAccountId] ON [NegotiableInstruments] ([SettlementFinancialAccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    CREATE INDEX [IX_FinancialTransactions_NegotiableInstrumentId] ON [FinancialTransactions] ([NegotiableInstrumentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    CREATE INDEX [IX_CurrentAccountTransactions_NegotiableInstrumentId] ON [CurrentAccountTransactions] ([NegotiableInstrumentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    ALTER TABLE [CurrentAccountTransactions] ADD CONSTRAINT [FK_CurrentAccountTransactions_NegotiableInstruments_NegotiableInstrumentId] FOREIGN KEY ([NegotiableInstrumentId]) REFERENCES [NegotiableInstruments] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    ALTER TABLE [FinancialTransactions] ADD CONSTRAINT [FK_FinancialTransactions_NegotiableInstruments_NegotiableInstrumentId] FOREIGN KEY ([NegotiableInstrumentId]) REFERENCES [NegotiableInstruments] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    ALTER TABLE [NegotiableInstruments] ADD CONSTRAINT [FK_NegotiableInstruments_Customers_EndorsedToCustomerId] FOREIGN KEY ([EndorsedToCustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    ALTER TABLE [NegotiableInstruments] ADD CONSTRAINT [FK_NegotiableInstruments_FinancialAccounts_SettlementFinancialAccountId] FOREIGN KEY ([SettlementFinancialAccountId]) REFERENCES [FinancialAccounts] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170218_AddNegotiableInstrumentAccountingLinks'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260803170218_AddNegotiableInstrumentAccountingLinks', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170520_AddNegotiableInstrumentCancellationAudit'
)
BEGIN
    ALTER TABLE [NegotiableInstruments] ADD [CancellationReason] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170520_AddNegotiableInstrumentCancellationAudit'
)
BEGIN
    ALTER TABLE [NegotiableInstruments] ADD [CancelledAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170520_AddNegotiableInstrumentCancellationAudit'
)
BEGIN
    ALTER TABLE [NegotiableInstruments] ADD [CancelledByUserId] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803170520_AddNegotiableInstrumentCancellationAudit'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260803170520_AddNegotiableInstrumentCancellationAudit', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    ALTER TABLE [Products] ADD [DefaultKitchenStationId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [KitchenStations] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [PrinterName] nvarchar(150) NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [BranchId] int NOT NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_KitchenStations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KitchenStations_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [ProductPortions] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [PriceOverride] decimal(18,2) NULL,
        [IsDefault] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [ProductId] int NOT NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ProductPortions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductPortions_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [RestaurantCashShifts] (
        [Id] int NOT NULL IDENTITY,
        [CashierUserId] nvarchar(450) NOT NULL,
        [Status] int NOT NULL,
        [OpenedAtUtc] datetime2 NOT NULL,
        [OpeningBalance] decimal(18,2) NOT NULL,
        [ClosedAtUtc] datetime2 NULL,
        [ClosingBalanceExpected] decimal(18,2) NULL,
        [ClosingBalanceCounted] decimal(18,2) NULL,
        [SubmissionKey] uniqueidentifier NULL,
        [BranchId] int NOT NULL,
        [FinancialAccountId] int NOT NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RestaurantCashShifts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RestaurantCashShifts_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RestaurantCashShifts_FinancialAccounts_FinancialAccountId] FOREIGN KEY ([FinancialAccountId]) REFERENCES [FinancialAccounts] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [RestaurantSections] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [BranchId] int NOT NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RestaurantSections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RestaurantSections_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [ProductRecipeHeaders] (
        [Id] int NOT NULL IDENTITY,
        [Version] int NOT NULL,
        [ValidFromUtc] datetime2 NOT NULL,
        [ValidToUtc] datetime2 NULL,
        [YieldQuantity] decimal(18,3) NOT NULL,
        [IsActive] bit NOT NULL,
        [ProductId] int NOT NULL,
        [ProductPortionId] int NULL,
        [BranchId] int NULL,
        [WarehouseId] int NULL,
        [ProductPortionId1] int NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ProductRecipeHeaders] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ProductRecipeHeaders_Yield] CHECK ([YieldQuantity] > 0),
        CONSTRAINT [FK_ProductRecipeHeaders_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductRecipeHeaders_ProductPortions_ProductPortionId] FOREIGN KEY ([ProductPortionId]) REFERENCES [ProductPortions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductRecipeHeaders_ProductPortions_ProductPortionId1] FOREIGN KEY ([ProductPortionId1]) REFERENCES [ProductPortions] ([Id]),
        CONSTRAINT [FK_ProductRecipeHeaders_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductRecipeHeaders_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [RestaurantTables] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [Capacity] int NOT NULL,
        [PosX] decimal(10,2) NULL,
        [PosY] decimal(10,2) NULL,
        [IsActive] bit NOT NULL,
        [RestaurantSectionId] int NOT NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RestaurantTables] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RestaurantTables_Capacity] CHECK ([Capacity] >= 0),
        CONSTRAINT [FK_RestaurantTables_RestaurantSections_RestaurantSectionId] FOREIGN KEY ([RestaurantSectionId]) REFERENCES [RestaurantSections] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [ProductRecipeLines] (
        [Id] int NOT NULL IDENTITY,
        [Quantity] decimal(18,3) NOT NULL,
        [WastagePercent] decimal(5,2) NOT NULL,
        [ProductRecipeHeaderId] int NOT NULL,
        [IngredientProductId] int NOT NULL,
        [UnitOfMeasureId] int NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ProductRecipeLines] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ProductRecipeLines_Quantity] CHECK ([Quantity] > 0),
        CONSTRAINT [CK_ProductRecipeLines_Wastage] CHECK ([WastagePercent] >= 0 AND [WastagePercent] <= 100),
        CONSTRAINT [FK_ProductRecipeLines_ProductRecipeHeaders_ProductRecipeHeaderId] FOREIGN KEY ([ProductRecipeHeaderId]) REFERENCES [ProductRecipeHeaders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProductRecipeLines_Products_IngredientProductId] FOREIGN KEY ([IngredientProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductRecipeLines_UnitsOfMeasure_UnitOfMeasureId] FOREIGN KEY ([UnitOfMeasureId]) REFERENCES [UnitsOfMeasure] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [RestaurantTableSessions] (
        [Id] int NOT NULL IDENTITY,
        [Status] int NOT NULL,
        [OpenedAtUtc] datetime2 NOT NULL,
        [OpenedByUserId] nvarchar(450) NOT NULL,
        [GuestCount] int NOT NULL,
        [WaiterUserId] nvarchar(450) NULL,
        [ClosedAtUtc] datetime2 NULL,
        [ClosedByUserId] nvarchar(450) NULL,
        [SubmissionKey] uniqueidentifier NULL,
        [RestaurantTableId] int NOT NULL,
        [MergedIntoSessionId] int NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RestaurantTableSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RestaurantTableSessions_RestaurantTableSessions_MergedIntoSessionId] FOREIGN KEY ([MergedIntoSessionId]) REFERENCES [RestaurantTableSessions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RestaurantTableSessions_RestaurantTables_RestaurantTableId] FOREIGN KEY ([RestaurantTableId]) REFERENCES [RestaurantTables] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [RestaurantTableSessionMoves] (
        [Id] int NOT NULL IDENTITY,
        [MovedAtUtc] datetime2 NOT NULL,
        [MovedByUserId] nvarchar(450) NOT NULL,
        [Reason] nvarchar(300) NULL,
        [RestaurantTableSessionId] int NOT NULL,
        [FromRestaurantTableId] int NOT NULL,
        [ToRestaurantTableId] int NOT NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RestaurantTableSessionMoves] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RestaurantTableSessionMoves_RestaurantTableSessions_RestaurantTableSessionId] FOREIGN KEY ([RestaurantTableSessionId]) REFERENCES [RestaurantTableSessions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RestaurantTableSessionMoves_RestaurantTables_FromRestaurantTableId] FOREIGN KEY ([FromRestaurantTableId]) REFERENCES [RestaurantTables] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RestaurantTableSessionMoves_RestaurantTables_ToRestaurantTableId] FOREIGN KEY ([ToRestaurantTableId]) REFERENCES [RestaurantTables] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [KitchenTicketLines] (
        [Id] int NOT NULL IDENTITY,
        [Status] int NOT NULL,
        [KitchenTicketId] int NOT NULL,
        [RestaurantOrderLineId] int NOT NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_KitchenTicketLines] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [KitchenTickets] (
        [Id] int NOT NULL IDENTITY,
        [TicketNumber] nvarchar(30) NULL,
        [Status] int NOT NULL,
        [SentAtUtc] datetime2 NOT NULL,
        [SubmissionKey] uniqueidentifier NULL,
        [RestaurantOrderId] int NOT NULL,
        [KitchenStationId] int NOT NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_KitchenTickets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KitchenTickets_KitchenStations_KitchenStationId] FOREIGN KEY ([KitchenStationId]) REFERENCES [KitchenStations] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [RestaurantChecks] (
        [Id] int NOT NULL IDENTITY,
        [CheckNumber] nvarchar(30) NOT NULL,
        [Status] int NOT NULL,
        [OpenedAtUtc] datetime2 NOT NULL,
        [ClosedAtUtc] datetime2 NULL,
        [SubtotalAmount] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [ServiceChargeAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [GrandTotal] decimal(18,2) NOT NULL,
        [CancelledByUserId] nvarchar(450) NULL,
        [CancelledAtUtc] datetime2 NULL,
        [CancellationReason] nvarchar(500) NULL,
        [SubmissionKey] uniqueidentifier NULL,
        [RestaurantTableSessionId] int NOT NULL,
        [LinkedInvoiceId] int NULL,
        [LinkedRetailSaleId] int NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RestaurantChecks] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RestaurantChecks_Amounts] CHECK ([SubtotalAmount] >= 0 AND [DiscountAmount] >= 0 AND [ServiceChargeAmount] >= 0 AND [TaxAmount] >= 0 AND [GrandTotal] >= 0),
        CONSTRAINT [FK_RestaurantChecks_Invoices_LinkedInvoiceId] FOREIGN KEY ([LinkedInvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RestaurantChecks_RestaurantTableSessions_RestaurantTableSessionId] FOREIGN KEY ([RestaurantTableSessionId]) REFERENCES [RestaurantTableSessions] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [RestaurantOrders] (
        [Id] int NOT NULL IDENTITY,
        [OrderedAtUtc] datetime2 NOT NULL,
        [OrderedByUserId] nvarchar(450) NOT NULL,
        [SubmissionKey] uniqueidentifier NULL,
        [RestaurantCheckId] int NOT NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RestaurantOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RestaurantOrders_RestaurantChecks_RestaurantCheckId] FOREIGN KEY ([RestaurantCheckId]) REFERENCES [RestaurantChecks] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [RestaurantPayments] (
        [Id] int NOT NULL IDENTITY,
        [PaymentMethod] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaidAtUtc] datetime2 NOT NULL,
        [IsReversal] bit NOT NULL,
        [ReversalOfId] int NULL,
        [SubmissionKey] uniqueidentifier NULL,
        [RestaurantCheckId] int NOT NULL,
        [FinancialAccountId] int NOT NULL,
        [FinancialTransactionId] int NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RestaurantPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RestaurantPayments_Amount] CHECK ([Amount] > 0),
        CONSTRAINT [FK_RestaurantPayments_FinancialAccounts_FinancialAccountId] FOREIGN KEY ([FinancialAccountId]) REFERENCES [FinancialAccounts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RestaurantPayments_FinancialTransactions_FinancialTransactionId] FOREIGN KEY ([FinancialTransactionId]) REFERENCES [FinancialTransactions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RestaurantPayments_RestaurantChecks_RestaurantCheckId] FOREIGN KEY ([RestaurantCheckId]) REFERENCES [RestaurantChecks] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RestaurantPayments_RestaurantPayments_ReversalOfId] FOREIGN KEY ([ReversalOfId]) REFERENCES [RestaurantPayments] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [RetailSales] (
        [Id] int NOT NULL IDENTITY,
        [DocumentNumber] nvarchar(30) NOT NULL,
        [Status] int NOT NULL,
        [IssuedAtUtc] datetime2 NOT NULL,
        [SubtotalAmount] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [ServiceChargeAmount] decimal(18,2) NOT NULL,
        [TaxAmount] decimal(18,2) NOT NULL,
        [GrandTotal] decimal(18,2) NOT NULL,
        [FiscalDeviceSerialNumber] nvarchar(50) NULL,
        [FiscalReceiptNumber] nvarchar(50) NULL,
        [ZReportNumber] nvarchar(50) NULL,
        [FiscalizationStatus] int NOT NULL,
        [FiscalTransactionId] nvarchar(100) NULL,
        [EInvoiceUuid] nvarchar(100) NULL,
        [CancelledByUserId] nvarchar(450) NULL,
        [CancelledAtUtc] datetime2 NULL,
        [CancellationReason] nvarchar(500) NULL,
        [RestaurantCheckId] int NOT NULL,
        [CustomerId] int NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RetailSales] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RetailSales_Amounts] CHECK ([SubtotalAmount] >= 0 AND [DiscountAmount] >= 0 AND [ServiceChargeAmount] >= 0 AND [TaxAmount] >= 0 AND [GrandTotal] >= 0),
        CONSTRAINT [FK_RetailSales_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RetailSales_RestaurantChecks_RestaurantCheckId] FOREIGN KEY ([RestaurantCheckId]) REFERENCES [RestaurantChecks] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [RestaurantOrderLines] (
        [Id] int NOT NULL IDENTITY,
        [Quantity] decimal(18,3) NOT NULL,
        [ProductNameSnapshot] nvarchar(200) NOT NULL,
        [PortionNameSnapshot] nvarchar(50) NULL,
        [UnitPriceSnapshot] decimal(18,2) NOT NULL,
        [TaxRateSnapshot] decimal(5,2) NOT NULL,
        [DiscountAmountSnapshot] decimal(18,2) NOT NULL,
        [RecipeVersionUsed] int NULL,
        [KitchenNote] nvarchar(500) NULL,
        [Status] int NOT NULL,
        [CancelledByUserId] nvarchar(450) NULL,
        [CancelledAtUtc] datetime2 NULL,
        [CancellationReason] nvarchar(500) NULL,
        [RestaurantOrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [ProductPortionId] int NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RestaurantOrderLines] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RestaurantOrderLines_Quantity] CHECK ([Quantity] > 0),
        CONSTRAINT [FK_RestaurantOrderLines_ProductPortions_ProductPortionId] FOREIGN KEY ([ProductPortionId]) REFERENCES [ProductPortions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RestaurantOrderLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RestaurantOrderLines_RestaurantOrders_RestaurantOrderId] FOREIGN KEY ([RestaurantOrderId]) REFERENCES [RestaurantOrders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [RetailSaleLines] (
        [Id] int NOT NULL IDENTITY,
        [ProductNameSnapshot] nvarchar(200) NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [UnitPriceSnapshot] decimal(18,2) NOT NULL,
        [TaxRateSnapshot] decimal(5,2) NOT NULL,
        [DiscountAmountSnapshot] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        [RetailSaleId] int NOT NULL,
        [ProductId] int NOT NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RetailSaleLines] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RetailSaleLines_Quantity] CHECK ([Quantity] > 0),
        CONSTRAINT [FK_RetailSaleLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RetailSaleLines_RetailSales_RetailSaleId] FOREIGN KEY ([RetailSaleId]) REFERENCES [RetailSales] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE TABLE [RestaurantOrderLineModifiers] (
        [Id] int NOT NULL IDENTITY,
        [NameSnapshot] nvarchar(150) NOT NULL,
        [PriceSnapshot] decimal(18,2) NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [RestaurantOrderLineId] int NOT NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RestaurantOrderLineModifiers] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RestaurantOrderLineModifiers_Quantity] CHECK ([Quantity] > 0),
        CONSTRAINT [FK_RestaurantOrderLineModifiers_RestaurantOrderLines_RestaurantOrderLineId] FOREIGN KEY ([RestaurantOrderLineId]) REFERENCES [RestaurantOrderLines] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 10;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 11;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 12;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 13;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 14;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 15;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 16;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 17;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 18;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 19;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 20;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 21;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 22;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 23;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 24;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 25;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 26;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 27;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 28;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 29;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 30;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 31;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 32;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 33;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 34;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 35;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 36;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 37;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 38;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 39;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 40;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 41;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 42;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 43;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 44;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 45;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 46;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'UPDATE [Products] SET [DefaultKitchenStationId] = NULL
    WHERE [Id] = 47;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_Products_DefaultKitchenStationId] ON [Products] ([DefaultKitchenStationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_KitchenStations_BranchId_Name] ON [KitchenStations] ([BranchId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_KitchenStations_RecordId] ON [KitchenStations] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_KitchenTicketLines_KitchenTicketId_RestaurantOrderLineId] ON [KitchenTicketLines] ([KitchenTicketId], [RestaurantOrderLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_KitchenTicketLines_RecordId] ON [KitchenTicketLines] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_KitchenTicketLines_RestaurantOrderLineId] ON [KitchenTicketLines] ([RestaurantOrderLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_KitchenTickets_KitchenStationId] ON [KitchenTickets] ([KitchenStationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_KitchenTickets_RecordId] ON [KitchenTickets] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_KitchenTickets_RestaurantOrderId_KitchenStationId] ON [KitchenTickets] ([RestaurantOrderId], [KitchenStationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_KitchenTickets_SubmissionKey] ON [KitchenTickets] ([SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_ProductPortions_ProductId_Name] ON [ProductPortions] ([ProductId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductPortions_RecordId] ON [ProductPortions] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_ProductRecipeHeaders_BranchId] ON [ProductRecipeHeaders] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ProductRecipeHeaders_OneActiveVersion] ON [ProductRecipeHeaders] ([ProductId], [ProductPortionId], [BranchId]) WHERE [ValidToUtc] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_ProductRecipeHeaders_ProductPortionId] ON [ProductRecipeHeaders] ([ProductPortionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_ProductRecipeHeaders_ProductPortionId1] ON [ProductRecipeHeaders] ([ProductPortionId1]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductRecipeHeaders_RecordId] ON [ProductRecipeHeaders] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_ProductRecipeHeaders_WarehouseId] ON [ProductRecipeHeaders] ([WarehouseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_ProductRecipeLines_IngredientProductId] ON [ProductRecipeLines] ([IngredientProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_ProductRecipeLines_ProductRecipeHeaderId] ON [ProductRecipeLines] ([ProductRecipeHeaderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductRecipeLines_RecordId] ON [ProductRecipeLines] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_ProductRecipeLines_UnitOfMeasureId] ON [ProductRecipeLines] ([UnitOfMeasureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantCashShifts_BranchId] ON [RestaurantCashShifts] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RestaurantCashShifts_OneOpenPerAccount] ON [RestaurantCashShifts] ([FinancialAccountId]) WHERE [Status] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RestaurantCashShifts_RecordId] ON [RestaurantCashShifts] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RestaurantCashShifts_SubmissionKey] ON [RestaurantCashShifts] ([SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RestaurantChecks_CheckNumber] ON [RestaurantChecks] ([CheckNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantChecks_LinkedInvoiceId] ON [RestaurantChecks] ([LinkedInvoiceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantChecks_LinkedRetailSaleId] ON [RestaurantChecks] ([LinkedRetailSaleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RestaurantChecks_RecordId] ON [RestaurantChecks] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantChecks_RestaurantTableSessionId_OpenedAtUtc] ON [RestaurantChecks] ([RestaurantTableSessionId], [OpenedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RestaurantChecks_SubmissionKey] ON [RestaurantChecks] ([SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RestaurantOrderLineModifiers_RecordId] ON [RestaurantOrderLineModifiers] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantOrderLineModifiers_RestaurantOrderLineId] ON [RestaurantOrderLineModifiers] ([RestaurantOrderLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantOrderLines_ProductId] ON [RestaurantOrderLines] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantOrderLines_ProductPortionId] ON [RestaurantOrderLines] ([ProductPortionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RestaurantOrderLines_RecordId] ON [RestaurantOrderLines] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantOrderLines_RestaurantOrderId] ON [RestaurantOrderLines] ([RestaurantOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantOrderLines_Status] ON [RestaurantOrderLines] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RestaurantOrders_RecordId] ON [RestaurantOrders] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantOrders_RestaurantCheckId_OrderedAtUtc] ON [RestaurantOrders] ([RestaurantCheckId], [OrderedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RestaurantOrders_SubmissionKey] ON [RestaurantOrders] ([SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantPayments_FinancialAccountId] ON [RestaurantPayments] ([FinancialAccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantPayments_FinancialTransactionId] ON [RestaurantPayments] ([FinancialTransactionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RestaurantPayments_RecordId] ON [RestaurantPayments] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantPayments_RestaurantCheckId] ON [RestaurantPayments] ([RestaurantCheckId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantPayments_ReversalOfId] ON [RestaurantPayments] ([ReversalOfId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RestaurantPayments_SubmissionKey] ON [RestaurantPayments] ([SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantSections_BranchId_Name] ON [RestaurantSections] ([BranchId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RestaurantSections_RecordId] ON [RestaurantSections] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RestaurantTables_RecordId] ON [RestaurantTables] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantTables_RestaurantSectionId_Name] ON [RestaurantTables] ([RestaurantSectionId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantTableSessionMoves_FromRestaurantTableId] ON [RestaurantTableSessionMoves] ([FromRestaurantTableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RestaurantTableSessionMoves_RecordId] ON [RestaurantTableSessionMoves] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantTableSessionMoves_RestaurantTableSessionId] ON [RestaurantTableSessionMoves] ([RestaurantTableSessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantTableSessionMoves_ToRestaurantTableId] ON [RestaurantTableSessionMoves] ([ToRestaurantTableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RestaurantTableSessions_MergedIntoSessionId] ON [RestaurantTableSessions] ([MergedIntoSessionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RestaurantTableSessions_OneOpenPerTable] ON [RestaurantTableSessions] ([RestaurantTableId]) WHERE [Status] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RestaurantTableSessions_RecordId] ON [RestaurantTableSessions] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RestaurantTableSessions_SubmissionKey] ON [RestaurantTableSessions] ([SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RetailSaleLines_ProductId] ON [RetailSaleLines] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RetailSaleLines_RecordId] ON [RetailSaleLines] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RetailSaleLines_RetailSaleId] ON [RetailSaleLines] ([RetailSaleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE INDEX [IX_RetailSales_CustomerId] ON [RetailSales] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RetailSales_DocumentNumber] ON [RetailSales] ([DocumentNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RetailSales_RecordId] ON [RetailSales] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RetailSales_RestaurantCheckId] ON [RetailSales] ([RestaurantCheckId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_KitchenStations_DefaultKitchenStationId] FOREIGN KEY ([DefaultKitchenStationId]) REFERENCES [KitchenStations] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    ALTER TABLE [KitchenTicketLines] ADD CONSTRAINT [FK_KitchenTicketLines_KitchenTickets_KitchenTicketId] FOREIGN KEY ([KitchenTicketId]) REFERENCES [KitchenTickets] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    ALTER TABLE [KitchenTicketLines] ADD CONSTRAINT [FK_KitchenTicketLines_RestaurantOrderLines_RestaurantOrderLineId] FOREIGN KEY ([RestaurantOrderLineId]) REFERENCES [RestaurantOrderLines] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    ALTER TABLE [KitchenTickets] ADD CONSTRAINT [FK_KitchenTickets_RestaurantOrders_RestaurantOrderId] FOREIGN KEY ([RestaurantOrderId]) REFERENCES [RestaurantOrders] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    ALTER TABLE [RestaurantChecks] ADD CONSTRAINT [FK_RestaurantChecks_RetailSales_LinkedRetailSaleId] FOREIGN KEY ([LinkedRetailSaleId]) REFERENCES [RetailSales] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803205902_AddRestaurantModulePhase1Schema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260803205902_AddRestaurantModulePhase1Schema', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803212547_FixProductRecipeHeaderPortionShadowFk'
)
BEGIN
    ALTER TABLE [ProductRecipeHeaders] DROP CONSTRAINT [FK_ProductRecipeHeaders_ProductPortions_ProductPortionId1];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803212547_FixProductRecipeHeaderPortionShadowFk'
)
BEGIN
    DROP INDEX [IX_ProductRecipeHeaders_ProductPortionId1] ON [ProductRecipeHeaders];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803212547_FixProductRecipeHeaderPortionShadowFk'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProductRecipeHeaders]') AND [c].[name] = N'ProductPortionId1');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [ProductRecipeHeaders] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [ProductRecipeHeaders] DROP COLUMN [ProductPortionId1];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260803212547_FixProductRecipeHeaderPortionShadowFk'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260803212547_FixProductRecipeHeaderPortionShadowFk', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807054438_RestaurantModulePhase2Prereqs'
)
BEGIN
    ALTER TABLE [RestaurantOrderLines] ADD [IsComplimentary] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807054438_RestaurantModulePhase2Prereqs'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM [NumberSequences] WHERE [Key] = N'RESTAURANT_CHECK')
    BEGIN
        INSERT INTO [NumberSequences] ([Key], [Prefix], [NextNumber], [Padding], [RecordId], [CreatedAtUtc])
        VALUES (N'RESTAURANT_CHECK', N'AD.', 1, 5, NEWID(), SYSUTCDATETIME());
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260807054438_RestaurantModulePhase2Prereqs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260807054438_RestaurantModulePhase2Prereqs', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808162239_AddBranchApiKeyForHybridSync'
)
BEGIN
    ALTER TABLE [Branches] ADD [ApiKey] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808162239_AddBranchApiKeyForHybridSync'
)
BEGIN
    EXEC(N'UPDATE [Branches] SET [ApiKey] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808162239_AddBranchApiKeyForHybridSync'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Branches_ApiKey] ON [Branches] ([ApiKey]) WHERE [ApiKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808162239_AddBranchApiKeyForHybridSync'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260808162239_AddBranchApiKeyForHybridSync', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808165300_RestaurantModulePhase3RetailSaleTradeType'
)
BEGIN
    ALTER TABLE [RetailSales] ADD [TradeType] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808165300_RestaurantModulePhase3RetailSaleTradeType'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM [Customers] WHERE [Code] = N'PERAKENDE-SATIS')
    BEGIN
        INSERT INTO [Customers] ([Code], [Name], [AccountType], [IsCustomer], [IsSupplier], [IsActive], [RiskLimit], [CreatedByUserId], [CreatedAtUtc], [RecordId])
        VALUES (N'PERAKENDE-SATIS', N'Perakende Satışlar Carisi', 2, 1, 0, 1, 0, N'SYSTEM', SYSUTCDATETIME(), NEWID());
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808165300_RestaurantModulePhase3RetailSaleTradeType'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260808165300_RestaurantModulePhase3RetailSaleTradeType', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 10;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 11;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 12;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 13;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 14;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 15;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 16;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 17;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 18;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 19;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 20;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 21;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 22;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 23;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 24;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 25;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 26;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 27;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 28;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 29;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 30;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 31;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 32;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 33;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 34;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 35;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 36;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 37;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 38;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 39;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 40;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 41;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 42;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 43;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 44;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 45;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 46;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [ProductBarcodes]
    WHERE [Id] = 47;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 10;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 11;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 12;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 13;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 14;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 15;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 16;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 17;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 18;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 19;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 20;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 21;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 22;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 23;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 24;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 25;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 26;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 27;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 28;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 29;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 30;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 31;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 32;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 33;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 34;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 35;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 36;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 37;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 38;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 39;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 40;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 41;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 42;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 43;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 44;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 45;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 46;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    EXEC(N'DELETE FROM [Products]
    WHERE [Id] = 47;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [CategoryId] = 1)
    BEGIN
        DELETE FROM [ProductCategories] WHERE [Id] = 1;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [CategoryId] = 2)
    BEGIN
        DELETE FROM [ProductCategories] WHERE [Id] = 2;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [CategoryId] = 3)
    BEGIN
        DELETE FROM [ProductCategories] WHERE [Id] = 3;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [CategoryId] = 4)
    BEGIN
        DELETE FROM [ProductCategories] WHERE [Id] = 4;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [CategoryId] = 5)
    BEGIN
        DELETE FROM [ProductCategories] WHERE [Id] = 5;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [CategoryId] = 6)
    BEGIN
        DELETE FROM [ProductCategories] WHERE [Id] = 6;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [CategoryId] = 7)
    BEGIN
        DELETE FROM [ProductCategories] WHERE [Id] = 7;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [CategoryId] = 8)
    BEGIN
        DELETE FROM [ProductCategories] WHERE [Id] = 8;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    ALTER TABLE [ProductCategories] ADD [DefaultKitchenStationId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    ALTER TABLE [ProductCategories] ADD [TaxRateId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    CREATE INDEX [IX_ProductCategories_DefaultKitchenStationId] ON [ProductCategories] ([DefaultKitchenStationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    CREATE INDEX [IX_ProductCategories_TaxRateId] ON [ProductCategories] ([TaxRateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    ALTER TABLE [ProductCategories] ADD CONSTRAINT [FK_ProductCategories_KitchenStations_DefaultKitchenStationId] FOREIGN KEY ([DefaultKitchenStationId]) REFERENCES [KitchenStations] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    ALTER TABLE [ProductCategories] ADD CONSTRAINT [FK_ProductCategories_TaxRates_TaxRateId] FOREIGN KEY ([TaxRateId]) REFERENCES [TaxRates] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260808205846_CategoryKitchenStationAndCleanDemoCatalog', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    ALTER TABLE [Products] ADD [DiscountNotApplicable] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    ALTER TABLE [Products] ADD [PromotionNotApplicable] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    ALTER TABLE [Products] ADD [VisibleInBranches] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    ALTER TABLE [ProductCategories] ADD [Color] nvarchar(7) NOT NULL DEFAULT N'#6c757d';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    ALTER TABLE [ProductCategories] ADD [DiscountNotApplicable] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    ALTER TABLE [ProductCategories] ADD [DisplayOrder] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    ALTER TABLE [ProductCategories] ADD [ParentCategoryId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    ALTER TABLE [ProductCategories] ADD [PromotionNotApplicable] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    ALTER TABLE [ProductCategories] ADD [ShowAsShortcut] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    ALTER TABLE [ProductCategories] ADD [ShowInMobile] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    ALTER TABLE [ProductCategories] ADD [ShowInOnlineOrder] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    ALTER TABLE [ProductCategories] ADD [VisibleInBranches] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    CREATE INDEX [IX_ProductCategories_ParentCategoryId] ON [ProductCategories] ([ParentCategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    ALTER TABLE [ProductCategories] ADD CONSTRAINT [FK_ProductCategories_ProductCategories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [ProductCategories] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808215413_CategoryAndProductVisibilityRestrictions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260808215413_CategoryAndProductVisibilityRestrictions', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808222907_PersonnelRestaurantPinAndDiscountLimits'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [DiscountLowerLimitPercent] decimal(5,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808222907_PersonnelRestaurantPinAndDiscountLimits'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [DiscountUpperLimitPercent] decimal(5,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808222907_PersonnelRestaurantPinAndDiscountLimits'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [RestaurantPinHash] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260808222907_PersonnelRestaurantPinAndDiscountLimits'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260808222907_PersonnelRestaurantPinAndDiscountLimits', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809141759_RestaurantPackageOrdersAndSelfSaleRework'
)
BEGIN
    CREATE TABLE [PackageOrders] (
        [Id] int NOT NULL IDENTITY,
        [PackageNumber] nvarchar(30) NOT NULL,
        [Channel] int NOT NULL,
        [CustomerName] nvarchar(200) NOT NULL,
        [CustomerPhone] nvarchar(30) NULL,
        [DeliveryAddress] nvarchar(500) NULL,
        [Status] int NOT NULL,
        [ReadyAtUtc] datetime2 NULL,
        [DispatchedAtUtc] datetime2 NULL,
        [DeliveredAtUtc] datetime2 NULL,
        [SubmissionKey] uniqueidentifier NULL,
        [RestaurantCheckId] int NOT NULL,
        [RecordId] uniqueidentifier NOT NULL DEFAULT (NEWSEQUENTIALID()),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PackageOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PackageOrders_RestaurantChecks_RestaurantCheckId] FOREIGN KEY ([RestaurantCheckId]) REFERENCES [RestaurantChecks] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809141759_RestaurantPackageOrdersAndSelfSaleRework'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PackageOrders_PackageNumber] ON [PackageOrders] ([PackageNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809141759_RestaurantPackageOrdersAndSelfSaleRework'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PackageOrders_RecordId] ON [PackageOrders] ([RecordId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809141759_RestaurantPackageOrdersAndSelfSaleRework'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PackageOrders_RestaurantCheckId] ON [PackageOrders] ([RestaurantCheckId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809141759_RestaurantPackageOrdersAndSelfSaleRework'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PackageOrders_SubmissionKey] ON [PackageOrders] ([SubmissionKey]) WHERE [SubmissionKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809141759_RestaurantPackageOrdersAndSelfSaleRework'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM [NumberSequences] WHERE [Key] = N'PACKAGE_ORDER')
    BEGIN
        INSERT INTO [NumberSequences] ([Key], [Prefix], [NextNumber], [Padding], [RecordId], [CreatedAtUtc])
        VALUES (N'PACKAGE_ORDER', N'PK-', 1, 5, NEWID(), SYSUTCDATETIME());
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809141759_RestaurantPackageOrdersAndSelfSaleRework'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260809141759_RestaurantPackageOrdersAndSelfSaleRework', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809142633_SeedRetailSaleNumberSequence'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM [NumberSequences] WHERE [Key] = N'RETAIL_SALE')
    BEGIN
        INSERT INTO [NumberSequences] ([Key], [Prefix], [NextNumber], [Padding], [RecordId], [CreatedAtUtc])
        VALUES (N'RETAIL_SALE', N'PSF.', 1, 5, NEWID(), SYSUTCDATETIME());
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260809142633_SeedRetailSaleNumberSequence'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260809142633_SeedRetailSaleNumberSequence', N'10.0.10');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902085448_AddFiscalDeviceSettings'
)
BEGIN
    ALTER TABLE [InventorySettings] ADD [FiscalAgentUrl] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902085448_AddFiscalDeviceSettings'
)
BEGIN
    ALTER TABLE [InventorySettings] ADD [FiscalDeviceType] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902085448_AddFiscalDeviceSettings'
)
BEGIN
    EXEC(N'UPDATE [InventorySettings] SET [FiscalAgentUrl] = NULL, [FiscalDeviceType] = 0
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260902085448_AddFiscalDeviceSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260902085448_AddFiscalDeviceSettings', N'10.0.10');
END;

COMMIT;
GO

