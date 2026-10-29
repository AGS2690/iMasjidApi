-- ============================================================================
-- SCRIPT: CreateLocalesTable.sql
-- DESCRIPTION: DDL and Seed DML for IM_Locales table (Supported Application Languages & Locales)
-- DATABASE: PostgreSQL 13+
-- ============================================================================

-- 1. Create IM_Locales Table
CREATE TABLE IF NOT EXISTS "IM_Locales" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DisplayOrder" INT NOT NULL DEFAULT 1,
    "CountryName" VARCHAR(100) NOT NULL,
    "PhoneCode" VARCHAR(20) NOT NULL,
    "LanguageName" VARCHAR(100) NOT NULL,
    "CultureCode" VARCHAR(20) NOT NULL,
    "CurrencySymbol" VARCHAR(20) NOT NULL,
    "CurrencyName" VARCHAR(100) NOT NULL,
    "CurrencyCode" VARCHAR(10) NOT NULL,
    "Direction" VARCHAR(10) NOT NULL DEFAULT 'LTR',
    "FlagIconUrl" VARCHAR(255) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedOn" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" VARCHAR(100) NULL,
    "ModifiedOn" TIMESTAMPTZ NULL,
    "ModifiedBy" VARCHAR(100) NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedOn" TIMESTAMPTZ NULL,
    "DeletedBy" VARCHAR(100) NULL
);

-- 2. Indexes
CREATE UNIQUE INDEX IF NOT EXISTS "IX_IM_Locales_CultureCode" ON "IM_Locales" ("CultureCode");
CREATE INDEX IF NOT EXISTS "IX_IM_Locales_DisplayOrder" ON "IM_Locales" ("DisplayOrder");
CREATE INDEX IF NOT EXISTS "IX_IM_Locales_IsActive" ON "IM_Locales" ("IsActive");

-- 3. Initial Supported Locales Seed Data
INSERT INTO "IM_Locales" (
    "Id",
    "DisplayOrder",
    "CountryName",
    "PhoneCode",
    "LanguageName",
    "CultureCode",
    "CurrencySymbol",
    "CurrencyName",
    "CurrencyCode",
    "Direction",
    "FlagIconUrl",
    "IsActive",
    "CreatedBy"
)
VALUES
(
    '9d0e2b08-2365-42bb-b2b8-aaffa57ec2fe'::UUID,
    1,
    'Indonesia',
    '+62',
    'Bahasa',
    'id-ID',
    'Rp',
    'Indonesian Rupiah',
    'IDR',
    'LTR',
    'assets/flags/BI.png',
    TRUE,
    'SystemSetup'
),
(
    '7185939a-a861-4435-b04a-75a004831027'::UUID,
    2,
    'Saudi Arabia',
    '+966',
    'عربي',
    'ar-SA',
    '﷼',
    'Saudi Riyal',
    'SAR',
    'RTL',
    'assets/flags/AR.svg',
    TRUE,
    'SystemSetup'
),
(
    '5147bd8b-79a2-4e72-95ca-4a33fae51d6d'::UUID,
    3,
    'United States',
    '+1',
    'English (US)',
    'en-US',
    '$',
    'US Dollar',
    'USD',
    'LTR',
    'assets/flags/EN.svg',
    TRUE,
    'SystemSetup'
),
(
    '0cb80eed-e33f-4067-a2b4-21dd38782367'::UUID,
    4,
    'India',
    '+91',
    'English (IN)',
    'en-IN',
    '₹',
    'Indian Rupee',
    'INR',
    'LTR',
    'assets/flags/IN.svg',
    FALSE,
    'SystemSetup'
)
ON CONFLICT ("Id") DO UPDATE SET
    "DisplayOrder"   = EXCLUDED."DisplayOrder",
    "CountryName"    = EXCLUDED."CountryName",
    "PhoneCode"      = EXCLUDED."PhoneCode",
    "LanguageName"   = EXCLUDED."LanguageName",
    "CultureCode"    = EXCLUDED."CultureCode",
    "CurrencySymbol" = EXCLUDED."CurrencySymbol",
    "CurrencyName"   = EXCLUDED."CurrencyName",
    "CurrencyCode"   = EXCLUDED."CurrencyCode",
    "Direction"      = EXCLUDED."Direction",
    "FlagIconUrl"    = EXCLUDED."FlagIconUrl",
    "IsActive"       = EXCLUDED."IsActive",
    "ModifiedOn"     = CURRENT_TIMESTAMP,
    "ModifiedBy"     = 'SystemSetup';
