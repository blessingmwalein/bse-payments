-- Run this on DESKTOP-9RRSD5S SQL Server to create the database manually
-- Or use: dotnet ef migrations add InitialCreate
-- Then: dotnet ef database update

CREATE DATABASE BSEPayments;
GO

USE BSEPayments;
GO

-- PaymentProviderConfigs table
CREATE TABLE PaymentProviderConfigs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Provider INT NOT NULL UNIQUE,
    Username NVARCHAR(255) NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    MerchantNumber NVARCHAR(50) NOT NULL,
    MerchantPin NVARCHAR(50) NOT NULL,
    BaseUrl NVARCHAR(500) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- ProviderTokens table
CREATE TABLE ProviderTokens (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Provider INT NOT NULL,
    AccessToken NVARCHAR(MAX) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
CREATE INDEX IX_ProviderTokens_Provider ON ProviderTokens(Provider);

-- PaymentTransactions table
CREATE TABLE PaymentTransactions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Provider INT NOT NULL,
    Type INT NOT NULL,
    Status INT NOT NULL,
    CdsNumber NVARCHAR(50) NOT NULL,
    OriginalTransactionReference NVARCHAR(100) NOT NULL,
    ProviderTransactionReference NVARCHAR(100),
    Amount DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(10) NOT NULL DEFAULT 'BWP',
    DebitPartyMsisdn NVARCHAR(50) NOT NULL,
    CreditPartyMsisdn NVARCHAR(50) NOT NULL,
    Description NVARCHAR(MAX),
    ErrorMessage NVARCHAR(MAX),
    RawRequest NVARCHAR(MAX),
    RawResponse NVARCHAR(MAX),
    PostedToCashTrans BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
CREATE INDEX IX_PaymentTransactions_OriginalTransactionReference ON PaymentTransactions(OriginalTransactionReference);
CREATE INDEX IX_PaymentTransactions_ProviderTransactionReference ON PaymentTransactions(ProviderTransactionReference);
CREATE INDEX IX_PaymentTransactions_CdsNumber ON PaymentTransactions(CdsNumber);

-- Seed BTC configuration
INSERT INTO PaymentProviderConfigs (Provider, Username, Password, MerchantNumber, MerchantPin, BaseUrl, IsActive)
VALUES (1, 'btc-dealer-1000212', 'pass1234', '70383747', '4827', 'https://btcapps.btc.bw', 1);
GO
