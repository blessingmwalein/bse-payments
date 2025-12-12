# Troubleshooting Guide

## Transaction Not Posting to CashTrans

If you see a successful transaction but it's not appearing in the CashTrans table, follow these steps:

### Step 1: Check Transaction in Database

Use the diagnostics endpoint to check if the transaction exists:

```bash
GET http://localhost:5000/api/diagnostics/transaction/MP251209.203547632.000325
```

**What to check:**
- Does the transaction exist?
- Is `cdsNumber` populated?
- Is `postedToCashTrans` false?
- Is `status` "Success"?

### Step 2: Check Application Logs

Look for these log messages:

```
Transaction {Ref} is successful, checking if needs posting to CashTrans
Posting transaction {Ref} to CashTrans for CDS {CdsNumber}
Posting to CashTrans: CDS={CdsNumber}, Amount={Amount}, Ref={Ref}
Successfully posted transaction {TransactionRef} to CashTrans
```

**Common Issues:**

#### Issue: "Transaction not found in database"
**Cause:** The transaction reference doesn't match what's in the database.
**Solution:** 
- Check if you're using the correct reference (provider vs original)
- Try with `useOriginalReference: true` in the status request

#### Issue: "Transaction has no CDS Number"
**Cause:** CDS Number wasn't provided in the original deposit/withdraw request.
**Solution:** Always include `cdsNumber` in your requests:
```json
{
  "provider": "BTC",
  "cdsNumber": "CV00002GK",
  "amount": 100.00,
  "subscriberMsisdn": "73001762"
}
```

#### Issue: "Transaction status is Paused, not posting"
**Cause:** Transaction hasn't been confirmed via USSD yet.
**Solution:** Wait for user to confirm on their phone, then check status again.

### Step 3: Check CashTrans Table

Query the BO database directly:

```sql
-- Check if record exists
SELECT TOP 10 * 
FROM BO.dbo.CashTrans 
WHERE CDS_Number = 'CV00002GK'
ORDER BY DateCreated DESC;

-- Check recent BSE API posts
SELECT TOP 10 * 
FROM BO.dbo.CashTrans 
WHERE PostedBy = 'bse-api'
ORDER BY DateCreated DESC;
```

Or use the diagnostics endpoint:

```bash
GET http://localhost:5000/api/diagnostics/cashtrans/check/CV00002GK
```

### Step 4: Manual Posting Test

If automatic posting isn't working, you can test the BO database connection:

```sql
-- Test insert into CashTrans
INSERT INTO BO.dbo.CashTrans (
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
    CaptureDate
) VALUES (
    'Test Mobile Money Deposit',
    'Account Deposit',
    100.00,
    GETDATE(),
    1,
    'CV00002GK',
    1,
    'TEST-001',
    'manual-test',
    'BWP',
    GETDATE()
);

-- Verify
SELECT * FROM BO.dbo.CashTrans WHERE Reference = 'TEST-001';
```

### Step 5: Check Database Connections

Verify both connection strings in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-9RRSD5S;Database=BSEPayments;...",
    "BoConnection": "Server=DESKTOP-9RRSD5S;Database=BO;..."
  }
}
```

Test connections:

```sql
-- Test BSEPayments connection
SELECT COUNT(*) FROM BSEPayments.dbo.PaymentTransactions;

-- Test BO connection
SELECT COUNT(*) FROM BO.dbo.CashTrans;
```

## Common Scenarios

### Scenario 1: Transaction Shows Success but Not in CashTrans

**Diagnosis:**
```bash
# 1. Get transaction details
GET /api/diagnostics/transaction/MP251209.203547632.000325

# 2. Check CashTrans
GET /api/diagnostics/cashtrans/check/CV00002GK

# 3. Check recent transactions
GET /api/diagnostics/transactions/recent?count=20
```

**Possible Causes:**
1. Transaction found but `postedToCashTrans` is false → Check logs for posting errors
2. Transaction found but `cdsNumber` is empty → Re-submit with CDS number
3. Transaction not found → Wrong reference or database issue

### Scenario 2: Multiple Status Checks

If you check status multiple times, the system should:
- First check: Post to CashTrans if successful
- Subsequent checks: Skip posting (already posted)

**Verify:**
```bash
# Check the same transaction twice
POST /api/payments/status
{
  "provider": "BTC",
  "transactionReference": "MP251209.203547632.000325"
}
```

Logs should show:
- First time: "Successfully posted transaction..."
- Second time: "Transaction already posted to CashTrans"

### Scenario 3: originalTransactionReference is null

This happens when checking status by provider reference. The response will show:
```json
{
  "originalTransactionReference": null
}
```

This is normal - the provider doesn't return our internal reference. But posting should still work because we look up the transaction in our database.

## Diagnostic Endpoints

### Get Transaction Details
```
GET /api/diagnostics/transaction/{reference}
```

Returns full transaction details including posting status.

### Check CashTrans Records
```
GET /api/diagnostics/cashtrans/check/{cdsNumber}
```

Returns recent CashTrans records for a CDS number.

### Get Recent Transactions
```
GET /api/diagnostics/transactions/recent?count=10
```

Returns recent transactions with posting status.

## Quick Fixes

### Fix 1: Transaction Exists but Not Posted

```sql
-- Find the transaction
SELECT * FROM BSEPayments.dbo.PaymentTransactions 
WHERE ProviderTransactionReference = 'MP251209.203547632.000325';

-- If PostedToCashTrans is 0 and Status is 3 (Success), manually post:
-- (Use the diagnostics endpoint or check status again)
```

### Fix 2: Missing CDS Number

```sql
-- Update transaction with CDS number
UPDATE BSEPayments.dbo.PaymentTransactions
SET CdsNumber = 'CV00002GK'
WHERE ProviderTransactionReference = 'MP251209.203547632.000325';

-- Then check status again to trigger posting
```

### Fix 3: Reset Posted Flag

```sql
-- If you need to re-post a transaction
UPDATE BSEPayments.dbo.PaymentTransactions
SET PostedToCashTrans = 0
WHERE ProviderTransactionReference = 'MP251209.203547632.000325';

-- Then check status again
```

## Need More Help?

Enable detailed logging in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "bse_payments": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

Then restart the service and check the console output for detailed logs.
