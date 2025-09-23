CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "AspNetRoles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
    "Name" TEXT NULL,
    "NormalizedName" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL
);

CREATE TABLE "AspNetUsers" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
    "UserName" TEXT NULL,
    "NormalizedUserName" TEXT NULL,
    "Email" TEXT NULL,
    "NormalizedEmail" TEXT NULL,
    "EmailConfirmed" INTEGER NOT NULL,
    "PasswordHash" TEXT NULL,
    "SecurityStamp" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "PhoneNumberConfirmed" INTEGER NOT NULL,
    "TwoFactorEnabled" INTEGER NOT NULL,
    "LockoutEnd" TEXT NULL,
    "LockoutEnabled" INTEGER NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL
);

CREATE TABLE "Customers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Customers" PRIMARY KEY AUTOINCREMENT,
    "CompanyName" TEXT NOT NULL,
    "ContactPerson" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Phone" TEXT NOT NULL,
    "Address" TEXT NOT NULL,
    "City" TEXT NOT NULL,
    "Country" TEXT NOT NULL,
    "Website" TEXT NULL,
    "TaxNumber" TEXT NULL,
    "TaxOffice" TEXT NULL,
    "Notes" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL
);

CREATE TABLE "AspNetRoleClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
    "RoleId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
    "UserId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserRoles" (
    "UserId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserTokens" (
    "UserId" TEXT NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Quotes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Quotes" PRIMARY KEY AUTOINCREMENT,
    "QuoteNumber" TEXT NOT NULL,
    "CustomerId" INTEGER NOT NULL,
    "Title" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "SubTotal" decimal(18,2) NOT NULL,
    "VatRate" TEXT NOT NULL,
    "VatAmount" decimal(18,2) NOT NULL,
    "TotalAmount" decimal(18,2) NOT NULL,
    "QuoteDate" TEXT NOT NULL,
    "ValidUntil" TEXT NOT NULL,
    "Status" INTEGER NOT NULL,
    "Notes" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    CONSTRAINT "FK_Quotes_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "QuoteItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_QuoteItems" PRIMARY KEY AUTOINCREMENT,
    "QuoteId" INTEGER NOT NULL,
    "ItemName" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Quantity" TEXT NOT NULL,
    "UnitPrice" decimal(18,2) NOT NULL,
    "Unit" TEXT NOT NULL,
    "DiscountPercentage" TEXT NOT NULL,
    "DiscountAmount" decimal(18,2) NOT NULL,
    "Total" decimal(18,2) NOT NULL,
    "SortOrder" INTEGER NOT NULL,
    CONSTRAINT "FK_QuoteItems_Quotes_QuoteId" FOREIGN KEY ("QuoteId") REFERENCES "Quotes" ("Id") ON DELETE CASCADE
);

INSERT INTO "Customers" ("Id", "Address", "City", "CompanyName", "ContactPerson", "Country", "CreatedAt", "Email", "IsActive", "Notes", "Phone", "TaxNumber", "TaxOffice", "UpdatedAt", "Website")
VALUES (1, 'Ataşehir Mah. Mustafa Kemal Cad. No:123', 'İstanbul', 'ABC Teknoloji Ltd. Şti.', 'Ahmet Yılmaz', 'Türkiye', '2024-07-10 00:00:00', 'ahmet@abcteknoloji.com', 1, NULL, '+90 212 555 0101', '1234567890', 'Ataşehir Vergi Dairesi', '2024-07-10 00:00:00', NULL);
SELECT changes();

INSERT INTO "Customers" ("Id", "Address", "City", "CompanyName", "ContactPerson", "Country", "CreatedAt", "Email", "IsActive", "Notes", "Phone", "TaxNumber", "TaxOffice", "UpdatedAt", "Website")
VALUES (2, 'Çankaya Mah. Atatürk Bulvarı No:456', 'Ankara', 'XYZ Mühendislik A.Ş.', 'Fatma Demir', 'Türkiye', '2024-07-25 00:00:00', 'fatma@xyzmuhendislik.com', 1, NULL, '+90 312 555 0202', '0987654321', 'Çankaya Vergi Dairesi', '2024-07-25 00:00:00', NULL);
SELECT changes();


CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");

CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

CREATE INDEX "IX_QuoteItems_QuoteId" ON "QuoteItems" ("QuoteId");

CREATE INDEX "IX_Quotes_CustomerId" ON "Quotes" ("CustomerId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250809054931_InitialCreate', '9.0.0');

ALTER TABLE "Quotes" ADD "Currency" TEXT NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250826141605_AddCurrencyToQuote', '9.0.0');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250826141649_AddCurrencyFieldToQuote', '9.0.0');

UPDATE Quotes SET Currency = 'EUR' WHERE Currency IS NULL OR Currency = '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250826144217_UpdateExistingQuotesCurrency', '9.0.0');

ALTER TABLE "Quotes" ADD "CreatedByUserId" TEXT NULL;

ALTER TABLE "Quotes" ADD "LastModifiedBy" TEXT NULL;

ALTER TABLE "Quotes" ADD "LastModifiedByUserId" TEXT NULL;

ALTER TABLE "AspNetUsers" ADD "CreatedAt" TEXT NOT NULL DEFAULT '0001-01-01 00:00:00';

ALTER TABLE "AspNetUsers" ADD "Department" TEXT NULL;

ALTER TABLE "AspNetUsers" ADD "FirstName" TEXT NOT NULL DEFAULT '';

ALTER TABLE "AspNetUsers" ADD "IsActive" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "AspNetUsers" ADD "LastName" TEXT NOT NULL DEFAULT '';

ALTER TABLE "AspNetUsers" ADD "Position" TEXT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250827124343_AddApplicationUserAndUserTracking', '9.0.0');

CREATE INDEX "IX_Quotes_CreatedByUserId" ON "Quotes" ("CreatedByUserId");

CREATE INDEX "IX_Quotes_LastModifiedByUserId" ON "Quotes" ("LastModifiedByUserId");

CREATE TABLE "ef_temp_Quotes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Quotes" PRIMARY KEY AUTOINCREMENT,
    "CreatedAt" TEXT NOT NULL,
    "CreatedBy" TEXT NOT NULL,
    "CreatedByUserId" TEXT NULL,
    "Currency" TEXT NOT NULL,
    "CustomerId" INTEGER NOT NULL,
    "Description" TEXT NOT NULL,
    "LastModifiedBy" TEXT NULL,
    "LastModifiedByUserId" TEXT NULL,
    "Notes" TEXT NULL,
    "QuoteDate" TEXT NOT NULL,
    "QuoteNumber" TEXT NOT NULL,
    "Status" INTEGER NOT NULL,
    "SubTotal" decimal(18,2) NOT NULL,
    "Title" TEXT NOT NULL,
    "TotalAmount" decimal(18,2) NOT NULL,
    "UpdatedAt" TEXT NOT NULL,
    "ValidUntil" TEXT NOT NULL,
    "VatAmount" decimal(18,2) NOT NULL,
    "VatRate" TEXT NOT NULL,
    CONSTRAINT "FK_Quotes_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Quotes_AspNetUsers_LastModifiedByUserId" FOREIGN KEY ("LastModifiedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Quotes_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Quotes" ("Id", "CreatedAt", "CreatedBy", "CreatedByUserId", "Currency", "CustomerId", "Description", "LastModifiedBy", "LastModifiedByUserId", "Notes", "QuoteDate", "QuoteNumber", "Status", "SubTotal", "Title", "TotalAmount", "UpdatedAt", "ValidUntil", "VatAmount", "VatRate")
SELECT "Id", "CreatedAt", "CreatedBy", "CreatedByUserId", "Currency", "CustomerId", "Description", "LastModifiedBy", "LastModifiedByUserId", "Notes", "QuoteDate", "QuoteNumber", "Status", "SubTotal", "Title", "TotalAmount", "UpdatedAt", "ValidUntil", "VatAmount", "VatRate"
FROM "Quotes";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Quotes";

ALTER TABLE "ef_temp_Quotes" RENAME TO "Quotes";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Quotes_CreatedByUserId" ON "Quotes" ("CreatedByUserId");

CREATE INDEX "IX_Quotes_CustomerId" ON "Quotes" ("CustomerId");

CREATE INDEX "IX_Quotes_LastModifiedByUserId" ON "Quotes" ("LastModifiedByUserId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250828131751_UpdateDatabase', '9.0.0');

BEGIN TRANSACTION;
CREATE TABLE "ContactMessages" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ContactMessages" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Message" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "EmailSent" INTEGER NOT NULL,
    "EmailError" TEXT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250903085716_AddContactMessages', '9.0.0');

DROP INDEX "UserNameIndex";

DROP INDEX "RoleNameIndex";

UPDATE "Customers" SET "CreatedAt" = '2024-07-10 00:00:00', "UpdatedAt" = '2024-07-10 00:00:00'
WHERE "Id" = 1;
SELECT changes();


UPDATE "Customers" SET "CreatedAt" = '2024-07-25 00:00:00', "UpdatedAt" = '2024-07-25 00:00:00'
WHERE "Id" = 2;
SELECT changes();


CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName") WHERE [NormalizedUserName] IS NOT NULL;

CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName") WHERE [NormalizedName] IS NOT NULL;

CREATE TABLE "ef_temp_Quotes" (
    "Id" int NOT NULL CONSTRAINT "PK_Quotes" PRIMARY KEY AUTOINCREMENT,
    "CreatedAt" datetime2 NOT NULL,
    "CreatedBy" nvarchar(100) NOT NULL,
    "CreatedByUserId" nvarchar(450) NULL,
    "Currency" nvarchar(3) NOT NULL,
    "CustomerId" int NOT NULL,
    "Description" nvarchar(1000) NOT NULL,
    "LastModifiedBy" nvarchar(100) NULL,
    "LastModifiedByUserId" nvarchar(450) NULL,
    "Notes" nvarchar(1000) NULL,
    "QuoteDate" datetime2 NOT NULL,
    "QuoteNumber" nvarchar(50) NOT NULL,
    "Status" int NOT NULL,
    "SubTotal" decimal(18,2) NOT NULL,
    "Title" nvarchar(200) NOT NULL,
    "TotalAmount" decimal(18,2) NOT NULL,
    "UpdatedAt" datetime2 NOT NULL,
    "ValidUntil" datetime2 NOT NULL,
    "VatAmount" decimal(18,2) NOT NULL,
    "VatRate" decimal(5,2) NOT NULL,
    CONSTRAINT "FK_Quotes_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Quotes_AspNetUsers_LastModifiedByUserId" FOREIGN KEY ("LastModifiedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Quotes_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Quotes" ("Id", "CreatedAt", "CreatedBy", "CreatedByUserId", "Currency", "CustomerId", "Description", "LastModifiedBy", "LastModifiedByUserId", "Notes", "QuoteDate", "QuoteNumber", "Status", "SubTotal", "Title", "TotalAmount", "UpdatedAt", "ValidUntil", "VatAmount", "VatRate")
SELECT "Id", "CreatedAt", "CreatedBy", "CreatedByUserId", "Currency", "CustomerId", "Description", "LastModifiedBy", "LastModifiedByUserId", "Notes", "QuoteDate", "QuoteNumber", "Status", "SubTotal", "Title", "TotalAmount", "UpdatedAt", "ValidUntil", "VatAmount", "VatRate"
FROM "Quotes";

CREATE TABLE "ef_temp_QuoteItems" (
    "Id" int NOT NULL CONSTRAINT "PK_QuoteItems" PRIMARY KEY AUTOINCREMENT,
    "Description" nvarchar(500) NOT NULL,
    "DiscountAmount" decimal(18,2) NOT NULL,
    "DiscountPercentage" decimal(5,2) NOT NULL,
    "ItemName" nvarchar(200) NOT NULL,
    "Quantity" decimal(18,2) NOT NULL,
    "QuoteId" int NOT NULL,
    "SortOrder" int NOT NULL,
    "Total" decimal(18,2) NOT NULL,
    "Unit" nvarchar(20) NOT NULL,
    "UnitPrice" decimal(18,2) NOT NULL,
    CONSTRAINT "FK_QuoteItems_Quotes_QuoteId" FOREIGN KEY ("QuoteId") REFERENCES "Quotes" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_QuoteItems" ("Id", "Description", "DiscountAmount", "DiscountPercentage", "ItemName", "Quantity", "QuoteId", "SortOrder", "Total", "Unit", "UnitPrice")
SELECT "Id", "Description", "DiscountAmount", "DiscountPercentage", "ItemName", "Quantity", "QuoteId", "SortOrder", "Total", "Unit", "UnitPrice"
FROM "QuoteItems";

CREATE TABLE "ef_temp_Customers" (
    "Id" int NOT NULL CONSTRAINT "PK_Customers" PRIMARY KEY AUTOINCREMENT,
    "Address" nvarchar(500) NOT NULL,
    "City" nvarchar(100) NOT NULL,
    "CompanyName" nvarchar(200) NOT NULL,
    "ContactPerson" nvarchar(100) NOT NULL,
    "Country" nvarchar(100) NOT NULL,
    "CreatedAt" datetime2 NOT NULL,
    "Email" nvarchar(max) NOT NULL,
    "IsActive" bit NOT NULL,
    "Notes" nvarchar(1000) NULL,
    "Phone" nvarchar(max) NOT NULL,
    "TaxNumber" nvarchar(50) NULL,
    "TaxOffice" nvarchar(100) NULL,
    "UpdatedAt" datetime2 NOT NULL,
    "Website" nvarchar(max) NULL
);

INSERT INTO "ef_temp_Customers" ("Id", "Address", "City", "CompanyName", "ContactPerson", "Country", "CreatedAt", "Email", "IsActive", "Notes", "Phone", "TaxNumber", "TaxOffice", "UpdatedAt", "Website")
SELECT "Id", "Address", "City", "CompanyName", "ContactPerson", "Country", "CreatedAt", "Email", "IsActive", "Notes", "Phone", "TaxNumber", "TaxOffice", "UpdatedAt", "Website"
FROM "Customers";

CREATE TABLE "ef_temp_ContactMessages" (
    "Id" int NOT NULL CONSTRAINT "PK_ContactMessages" PRIMARY KEY AUTOINCREMENT,
    "CreatedAt" datetime2 NOT NULL,
    "Email" nvarchar(max) NOT NULL,
    "EmailError" nvarchar(max) NULL,
    "EmailSent" bit NOT NULL,
    "Message" nvarchar(1000) NOT NULL,
    "Name" nvarchar(100) NOT NULL
);

INSERT INTO "ef_temp_ContactMessages" ("Id", "CreatedAt", "Email", "EmailError", "EmailSent", "Message", "Name")
SELECT "Id", "CreatedAt", "Email", "EmailError", "EmailSent", "Message", "Name"
FROM "ContactMessages";

CREATE TABLE "ef_temp_AspNetUserTokens" (
    "UserId" nvarchar(450) NOT NULL,
    "LoginProvider" nvarchar(128) NOT NULL,
    "Name" nvarchar(128) NOT NULL,
    "Value" nvarchar(max) NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_AspNetUserTokens" ("UserId", "LoginProvider", "Name", "Value")
SELECT "UserId", "LoginProvider", "Name", "Value"
FROM "AspNetUserTokens";

CREATE TABLE "ef_temp_AspNetUsers" (
    "Id" nvarchar(450) NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
    "AccessFailedCount" int NOT NULL,
    "ConcurrencyStamp" nvarchar(max) NULL,
    "CreatedAt" datetime2 NOT NULL,
    "Department" nvarchar(max) NULL,
    "Email" nvarchar(256) NULL,
    "EmailConfirmed" bit NOT NULL,
    "FirstName" nvarchar(max) NOT NULL,
    "IsActive" bit NOT NULL,
    "LastName" nvarchar(max) NOT NULL,
    "LockoutEnabled" bit NOT NULL,
    "LockoutEnd" datetimeoffset NULL,
    "NormalizedEmail" nvarchar(256) NULL,
    "NormalizedUserName" nvarchar(256) NULL,
    "PasswordHash" nvarchar(max) NULL,
    "PhoneNumber" nvarchar(max) NULL,
    "PhoneNumberConfirmed" bit NOT NULL,
    "Position" nvarchar(max) NULL,
    "SecurityStamp" nvarchar(max) NULL,
    "TwoFactorEnabled" bit NOT NULL,
    "UserName" nvarchar(256) NULL
);

INSERT INTO "ef_temp_AspNetUsers" ("Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Department", "Email", "EmailConfirmed", "FirstName", "IsActive", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Position", "SecurityStamp", "TwoFactorEnabled", "UserName")
SELECT "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Department", "Email", "EmailConfirmed", "FirstName", "IsActive", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Position", "SecurityStamp", "TwoFactorEnabled", "UserName"
FROM "AspNetUsers";

CREATE TABLE "ef_temp_AspNetUserRoles" (
    "UserId" nvarchar(450) NOT NULL,
    "RoleId" nvarchar(450) NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_AspNetUserRoles" ("UserId", "RoleId")
SELECT "UserId", "RoleId"
FROM "AspNetUserRoles";

CREATE TABLE "ef_temp_AspNetUserLogins" (
    "LoginProvider" nvarchar(128) NOT NULL,
    "ProviderKey" nvarchar(128) NOT NULL,
    "ProviderDisplayName" nvarchar(max) NULL,
    "UserId" nvarchar(450) NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_AspNetUserLogins" ("LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId")
SELECT "LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId"
FROM "AspNetUserLogins";

CREATE TABLE "ef_temp_AspNetUserClaims" (
    "Id" int NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
    "ClaimType" nvarchar(max) NULL,
    "ClaimValue" nvarchar(max) NULL,
    "UserId" nvarchar(450) NOT NULL,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_AspNetUserClaims" ("Id", "ClaimType", "ClaimValue", "UserId")
SELECT "Id", "ClaimType", "ClaimValue", "UserId"
FROM "AspNetUserClaims";

CREATE TABLE "ef_temp_AspNetRoles" (
    "Id" nvarchar(450) NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
    "ConcurrencyStamp" nvarchar(max) NULL,
    "Name" nvarchar(256) NULL,
    "NormalizedName" nvarchar(256) NULL
);

INSERT INTO "ef_temp_AspNetRoles" ("Id", "ConcurrencyStamp", "Name", "NormalizedName")
SELECT "Id", "ConcurrencyStamp", "Name", "NormalizedName"
FROM "AspNetRoles";

CREATE TABLE "ef_temp_AspNetRoleClaims" (
    "Id" int NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
    "ClaimType" nvarchar(max) NULL,
    "ClaimValue" nvarchar(max) NULL,
    "RoleId" nvarchar(450) NOT NULL,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_AspNetRoleClaims" ("Id", "ClaimType", "ClaimValue", "RoleId")
SELECT "Id", "ClaimType", "ClaimValue", "RoleId"
FROM "AspNetRoleClaims";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Quotes";

ALTER TABLE "ef_temp_Quotes" RENAME TO "Quotes";

DROP TABLE "QuoteItems";

ALTER TABLE "ef_temp_QuoteItems" RENAME TO "QuoteItems";

DROP TABLE "Customers";

ALTER TABLE "ef_temp_Customers" RENAME TO "Customers";

DROP TABLE "ContactMessages";

ALTER TABLE "ef_temp_ContactMessages" RENAME TO "ContactMessages";

DROP TABLE "AspNetUserTokens";

ALTER TABLE "ef_temp_AspNetUserTokens" RENAME TO "AspNetUserTokens";

DROP TABLE "AspNetUsers";

ALTER TABLE "ef_temp_AspNetUsers" RENAME TO "AspNetUsers";

DROP TABLE "AspNetUserRoles";

ALTER TABLE "ef_temp_AspNetUserRoles" RENAME TO "AspNetUserRoles";

DROP TABLE "AspNetUserLogins";

ALTER TABLE "ef_temp_AspNetUserLogins" RENAME TO "AspNetUserLogins";

DROP TABLE "AspNetUserClaims";

ALTER TABLE "ef_temp_AspNetUserClaims" RENAME TO "AspNetUserClaims";

DROP TABLE "AspNetRoles";

ALTER TABLE "ef_temp_AspNetRoles" RENAME TO "AspNetRoles";

DROP TABLE "AspNetRoleClaims";

ALTER TABLE "ef_temp_AspNetRoleClaims" RENAME TO "AspNetRoleClaims";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Quotes_CreatedByUserId" ON "Quotes" ("CreatedByUserId");

CREATE INDEX "IX_Quotes_CustomerId" ON "Quotes" ("CustomerId");

CREATE INDEX "IX_Quotes_LastModifiedByUserId" ON "Quotes" ("LastModifiedByUserId");

CREATE INDEX "IX_QuoteItems_QuoteId" ON "QuoteItems" ("QuoteId");

CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");

CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName") WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName") WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250923084847_InitialMSSQLMigration', '9.0.0');

