# CashTrans Integration

## Overview

The payment service now automatically posts successful transactions to the `CashTrans` table in the BO database.

## What Changed

### 1. CDS Number Required

All API requests now require a `cdsNumber` field (client account number):

**Before:**
```json
{
  "provider": "BTC",
  "amount": 100.00,
  "subscriberMsisdn": "73001762"
}
```

**After:**
```json
{
  "provider": "BTC",
  "cdsNumber": "CV00002GK",
  "amount": 100.00,
  "subscriberMsisdn": "73001762"
}
```

### 2. Automatic CashTrans Posting

When you check transaction status and the transaction is **successful**, the system automatically:
1. Checks if the transaction has already been posted
2. If not, creates a record in `BO.dbo.CashTrans`
3. Marks the transaction as posted to prevent duplicates

### 3. CashTrans Record Format

```sql
INSERT INTO CashTrans (
    Description,
    TransType,
    Amount,
    DateCreated,
    TransStatus,
    CDS_Number,
    Paid,
    Reference,
    PostedBy,
    Currency,
    CaptureDate,
    Ref2
) VALUES (
    'Mobile Money Deposit - BTC',
    'Account Deposit',
    100.00,
    GETDATE(),
    1,
    'CV00002GK',
    1,
    'Batch_bse-api09122025033PM',
    'bse-api',
    'BWP',
    GETDATE(),
    'MP251209.155210620.000321'
)
```

## Workflow

### Deposit Flow

1. **Client initiates deposit:**
```http
POST /api/payments/deposit
{
  "provider": "BTC",
  "cdsNumber": "CV00002GK",
  "amount": 100.00,
  "subscriberMsisdn": "73001762"
}
```

2. **System response:**
```json
{
  "success": true,
  "message": "Your transaction request has been initiated, and is now pending confirmation",
  "transactionReference": "MP251209.155210620.000321",
  "originalTransactionReference": "DEP-20251209160530123-A1B2C3",
  "status": 2,
  "amount": 100.00
}
```

3. **User confirms via USSD on their phone**

4. **Client checks status:**
```http
POST /api/payments/status
{
  "provider": "BTC",
  "transactionReference": "MP251209.155210620.000321",
  "useOriginalReference": false
}
```

5. **If successful, system automatically:**
   - Returns success status
   - Posts to `CashTrans` table
   - Marks transaction as posted

```json
{
  "success": true,
  "message": "Success",
  "transactionReference": "MP251209.155210620.000321",
  "status": 3,
  "amount": 100.00
}
```

## Database Schema

### PaymentTransactions (BSEPayments DB)

```sql
CREATE TABLE PaymentTransactions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Provider INT NOT NULL,
    Type INT NOT NULL,
    Status INT NOT NULL,
    CdsNumber NVARCHAR(50) NOT NULL,  -- NEW FIELD
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
    PostedToCashTrans BIT NOT NULL DEFAULT 0,  -- NEW FIELD
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
```

### CashTrans (BO DB)

Existing table - no changes needed. The service posts to this table automatically.

## Configuration

### Connection Strings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-9RRSD5S;Database=BSEPayments;...",
    "BoConnection": "Server=DESKTOP-9RRSD5S;Database=BO;..."
  }
}
```

## API Examples

### Deposit with CDS Number

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

### Withdraw with CDS Number

```bash
curl -X POST http://localhost:5000/api/payments/withdraw \
  -H "Content-Type: application/json" \
  -d '{
    "provider": "BTC",
    "cdsNumber": "CV00003PK",
    "amount": 50.00,
    "subscriberMsisdn": "73001762"
  }'
```

### Check Status (Auto-posts if successful)

```bash
curl -X POST http://localhost:5000/api/payments/status \
  -H "Content-Type: application/json" \
  -d '{
    "provider": "BTC",
    "transactionReference": "MP251209.155210620.000321",
    "useOriginalReference": false
  }'
```

## Important Notes

1. **CDS Number is mandatory** - All requests must include the client's CDS account number
2. **Automatic posting** - No manual intervention needed; successful transactions are posted automatically
3. **Duplicate prevention** - The `PostedToCashTrans` flag prevents duplicate postings
4. **Reference format** - CashTrans reference follows pattern: `Batch_{postedBy}{ddMMyyyyhhmmtt}`
5. **Currency** - Defaults to BWP (Botswana Pula) but can be configured per transaction
6. **Posted by** - Defaults to "bse-api" but can be customized

## Troubleshooting

### Transaction not posting to CashTrans?

Check:
1. Transaction status is `Success` (status = 3)
2. `PostedToCashTrans` flag is false
3. BO database connection is working
4. CDS_Number is valid

### View posted transactions

```sql
SELECT * FROM BO.dbo.CashTrans 
WHERE PostedBy = 'bse-api' 
ORDER BY DateCreated DESC
```

### Check posting status

```sql
SELECT 
    CdsNumber,
    OriginalTransactionReference,
    Status,
    PostedToCashTrans,
    Amount
FROM BSEPayments.dbo.PaymentTransactions
WHERE Status = 3
ORDER BY CreatedAt DESC
```
