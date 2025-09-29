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
    [FirstName] nvarchar(max) NOT NULL,
    [LastName] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [Department] nvarchar(max) NULL,
    [Position] nvarchar(max) NULL,
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

CREATE TABLE [ContactMessages] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [Message] nvarchar(1000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [EmailSent] bit NOT NULL,
    [EmailError] nvarchar(max) NULL,
    CONSTRAINT [PK_ContactMessages] PRIMARY KEY ([Id])
);

CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [CompanyName] nvarchar(200) NOT NULL,
    [ContactPerson] nvarchar(100) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    [Address] nvarchar(500) NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [Country] nvarchar(100) NOT NULL,
    [Website] nvarchar(max) NULL,
    [TaxNumber] nvarchar(50) NULL,
    [TaxOffice] nvarchar(100) NULL,
    [Notes] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
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
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
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
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Quotes] (
    [Id] int NOT NULL IDENTITY,
    [QuoteNumber] nvarchar(50) NOT NULL,
    [CustomerId] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NOT NULL,
    [SubTotal] decimal(18,2) NOT NULL,
    [VatRate] decimal(5,2) NOT NULL,
    [VatAmount] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [QuoteDate] datetime2 NOT NULL,
    [ValidUntil] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [Currency] nvarchar(3) NOT NULL,
    [Notes] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NOT NULL,
    [CreatedByUserId] nvarchar(450) NULL,
    [LastModifiedBy] nvarchar(100) NULL,
    [LastModifiedByUserId] nvarchar(450) NULL,
    CONSTRAINT [PK_Quotes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Quotes_AspNetUsers_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_Quotes_AspNetUsers_LastModifiedByUserId] FOREIGN KEY ([LastModifiedByUserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_Quotes_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [QuoteItems] (
    [Id] int NOT NULL IDENTITY,
    [QuoteId] int NOT NULL,
    [ItemName] nvarchar(200) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [Quantity] decimal(18,2) NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Unit] nvarchar(20) NOT NULL,
    [DiscountPercentage] decimal(5,2) NOT NULL,
    [DiscountAmount] decimal(18,2) NOT NULL,
    [Total] decimal(18,2) NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_QuoteItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_QuoteItems_Quotes_QuoteId] FOREIGN KEY ([QuoteId]) REFERENCES [Quotes] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_QuoteItems_QuoteId] ON [QuoteItems] ([QuoteId]);

CREATE INDEX [IX_Quotes_CreatedByUserId] ON [Quotes] ([CreatedByUserId]);

CREATE INDEX [IX_Quotes_CustomerId] ON [Quotes] ([CustomerId]);

CREATE INDEX [IX_Quotes_LastModifiedByUserId] ON [Quotes] ([LastModifiedByUserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250923093157_SQLServerInitialFixed', N'9.0.0');

COMMIT;
GO

