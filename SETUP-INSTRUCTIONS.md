# Database Setup Instructions

## Prerequisites

- SQL Server installed on DESKTOP-9RRSD5S
- SQL Server Management Studio (SSMS) or Azure Data Studio

## Step-by-Step Setup

### Option 1: Fresh Installation (Recommended)

If you haven't created the database yet:

1. **Open SQL Server Management Studio**
2. **Connect to DESKTOP-9RRSD5S**
3. **Run the complete setup script:**

```sql
-- Open and execute: Migrations/InitialCreate.sql
```

This will:
- Create the `BSEPayments` database
- Create all tables (PaymentProviderConfigs, ProviderTokens, PaymentTransactions)
- Seed BTC configuration

### Option 2: Update Existing Database

If you already have the database but need to add new columns:

1. **Open SQL Server Management Studio**
2. **Connect to DESKTOP-9RRSD5S**
3. **Run the update script:**

```sql
-- Open and execute: Migrations/AddCdsNumberAndPostedFlag.sql
```

This will:
- Add `CdsNumber` column to PaymentTransactions
- Add `PostedToCashTrans` column to PaymentTransactions
- Create necessary indexes

### Option 3: Using Entity Framework Migrations

If you prefer using EF Core migrations:

```bash
# Create migration
dotnet ef migrations add AddCdsNumberAndPostedFlag

# Apply migration
dotnet ef database update
```

## Verify Setup

After running the scripts, verify the tables exist:

```sql
USE BSEPayments;
GO

-- Check tables
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';

-- Check PaymentTransactions columns
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'PaymentTransactions'
ORDER BY ORDINAL_POSITION;

-- Check if BTC config exists
SELECT * FROM PaymentProviderConfigs;
```

Expected output:
- 3 tables: PaymentProviderConfigs, ProviderTokens, PaymentTransactions
- PaymentTransactions should have CdsNumber and PostedToCashTrans columns
- 1 row in PaymentProviderConfigs for BTC

## BO Database

The BO database should already exist with the CashTrans table. No changes needed there.

Verify BO connection:

```sql
USE BO;
GO

-- Check CashTrans table exists
SELECT TOP 5 * FROM CashTrans ORDER BY DateCreated DESC;
```

## Connection Strings

Verify your `appsettings.json` has both connections:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-9RRSD5S;Database=BSEPayments;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
    "BoConnection": "Server=DESKTOP-9RRSD5S;Database=BO;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

## Troubleshooting

### Error: "Invalid object name 'PaymentProviderConfigs'"

**Solution:** Run `Migrations/InitialCreate.sql` to create all tables.

### Error: "Invalid column name 'CdsNumber'"

**Solution:** Run `Migrations/AddCdsNumberAndPostedFlag.sql` to add missing columns.

### Error: "Cannot open database 'BSEPayments'"

**Solution:** The database doesn't exist. Run `Migrations/InitialCreate.sql`.

### Error: "Login failed for user"

**Solution:** Check Windows Authentication is enabled and your user has permissions.

## Quick Start Command

After database setup, start the service:

```bash
dotnet run
```

Then navigate to: `http://localhost:5000/swagger`

## Test the Setup

Once running, test with a deposit:

```bash
curl -X POST http://localhost:5000/api/payments/deposit \
  -H "Content-Type: application/json" \
  -d '{
    "provider": "BTC",
    "cdsNumber": "CV00002GK",
    "amount": 100.00,
    "subscriberMsisdn": "73001762"
  }'
```

Expected response:
```json
{
  "success": true,
  "message": "Your transaction request has been initiated...",
  "transactionReference": "MP251209.155210620.000321",
  "originalTransactionReference": "DEP-20251209160530123-A1B2C3",
  "status": "PAUSED",
  "amount": 100.00
}
```
