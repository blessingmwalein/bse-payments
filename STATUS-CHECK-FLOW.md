# Status Check Flow

## Complete Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│ Client calls: POST /api/payments/status                         │
│ { "provider": "BTC", "transactionReference": "MP251209..." }   │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ PaymentService.GetTransactionStatusAsync()                      │
│ - Maps provider string to enum                                  │
│ - Gets appropriate adapter (BtcAdapter)                         │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ BtcAdapter.GetTransactionStatusAsync()                          │
│ Step 1: Query BTC API for current status                        │
│   GET https://btcapps.btc.bw/.../transactions/internal/MP...   │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ BTC API Response:                                                │
│ {                                                                │
│   "amount": "50.0",                                             │
│   "descriptionText": "Success",                                 │
│   "transactionReference": "MP251209.203547632.000325"          │
│ }                                                                │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ BtcAdapter: Parse response and determine status                 │
│ - description contains "success" → Status = Success             │
│ - otherwise → Status = Pending                                  │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ BtcAdapter: Update database transaction                         │
│ Step 2: Look up transaction in PaymentTransactions table        │
│   WHERE ProviderTransactionReference = "MP251209..."           │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ Found transaction:                                               │
│ - Current status: Paused                                        │
│ - New status: Success                                           │
│ - Status changed? YES                                           │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ BtcAdapter: UPDATE PaymentTransactions                          │
│ Step 3: Update status in database                               │
│   UPDATE PaymentTransactions                                    │
│   SET Status = 3 (Success),                                     │
│       Description = "Success",                                  │
│       UpdatedAt = NOW()                                         │
│   WHERE Id = 2                                                  │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ BtcAdapter: Return response to PaymentService                   │
│ {                                                                │
│   Success = true,                                               │
│   Status = Success,                                             │
│   TransactionReference = "MP251209...",                        │
│   Amount = 50                                                   │
│ }                                                                │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ PaymentService: Check if needs posting to CashTrans             │
│ Step 4: Is response.Status == Success? YES                      │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ PaymentService: Look up transaction again                       │
│ Step 5: Get fresh transaction from database                     │
│   (Now has Status = Success from Step 3)                        │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ PaymentService: Check posting conditions                        │
│ - Transaction found? YES                                        │
│ - PostedToCashTrans? NO                                         │
│ - Status = Success? YES                                         │
│ → Proceed with posting                                          │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ CashTransService.PostTransactionAsync()                         │
│ Step 6: Validate transaction                                    │
│ - Has CDS Number? YES (CV00002GK)                              │
│ - Status = Success? YES                                         │
│ - Already posted? NO                                            │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ CashTransService: INSERT into BO.dbo.CashTrans                 │
│ Step 7: Create CashTrans record                                 │
│   INSERT INTO CashTrans (                                       │
│     Description = "Mobile Money Deposit - BTC",                │
│     TransType = "Account Deposit",                             │
│     Amount = 50.00,                                            │
│     CDS_Number = "CV00002GK",                                  │
│     Paid = 1,                                                  │
│     Reference = "Batch_bse-api09122025...",                   │
│     PostedBy = "bse-api",                                      │
│     Currency = "BWP",                                          │
│     Ref2 = "MP251209.203547632.000325"                        │
│   )                                                             │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ PaymentService: Mark as posted                                  │
│ Step 8: UPDATE PaymentTransactions                              │
│   UPDATE PaymentTransactions                                    │
│   SET PostedToCashTrans = 1,                                   │
│       UpdatedAt = NOW()                                         │
│   WHERE Id = 2                                                  │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ Return response to client:                                      │
│ {                                                                │
│   "success": true,                                              │
│   "message": "Success",                                         │
│   "transactionReference": "MP251209.203547632.000325",         │
│   "status": "SUCCESS",                                          │
│   "amount": 50                                                  │
│ }                                                                │
└─────────────────────────────────────────────────────────────────┘
```

## Key Points

### ✅ Step 3: Database Status Update
**CRITICAL:** The transaction status is updated in the database BEFORE checking if it needs posting.

```csharp
// In BtcAdapter.GetTransactionStatusAsync()
if (transaction != null && transaction.Status != status)
{
    transaction.Status = status;  // Update from Paused to Success
    transaction.Description = description;
    await _repository.UpdateTransactionAsync(transaction);
}
```

### ✅ Step 5: Fresh Transaction Lookup
The PaymentService looks up the transaction again, getting the UPDATED status:

```csharp
// In PaymentService.GetTransactionStatusAsync()
var transaction = await _repository.GetTransactionByReferenceAsync(
    request.TransactionReference, 
    request.UseOriginalReference);
// This now returns transaction with Status = Success
```

### ✅ Step 7: CashTrans Posting
Only happens if:
1. Response status is Success ✓
2. Transaction found in database ✓
3. Transaction status is Success ✓ (updated in Step 3)
4. Not already posted ✓
5. Has CDS Number ✓

## Logs You Should See

```
[INFO] Updating transaction MP251209.203547632.000325 status from Paused to Success
[INFO] Transaction MP251209.203547632.000325 is successful, checking if needs posting to CashTrans
[INFO] Posting transaction MP251209.203547632.000325 to CashTrans for CDS CV00002GK
[INFO] Posting to CashTrans: CDS=CV00002GK, Amount=50, Ref=Batch_bse-api...
[INFO] Successfully posted transaction DEP-20251209183546038-3010FF to CashTrans for CDS CV00002GK
[INFO] Successfully posted transaction MP251209.203547632.000325 to CashTrans
```

## Verification

After calling status API, verify:

```bash
# 1. Check transaction was updated
GET /api/diagnostics/transaction/MP251209.203547632.000325
# Should show: status: "Success", postedToCashTrans: true

# 2. Check CashTrans record exists
GET /api/diagnostics/cashtrans/check/CV00002GK
# Should show the new record

# 3. Or query database directly
SELECT * FROM BSEPayments.dbo.PaymentTransactions 
WHERE ProviderTransactionReference = 'MP251209.203547632.000325';
-- Status should be 3 (Success), PostedToCashTrans should be 1

SELECT * FROM BO.dbo.CashTrans 
WHERE CDS_Number = 'CV00002GK' 
ORDER BY DateCreated DESC;
-- Should see the new record
```

## Second Status Check

If you call status API again for the same transaction:

```
[INFO] Transaction MP251209.203547632.000325 is successful, checking if needs posting to CashTrans
[INFO] Transaction MP251209.203547632.000325 already posted to CashTrans
```

No duplicate posting occurs because `PostedToCashTrans` flag is true.
