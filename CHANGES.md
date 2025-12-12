# API Simplification Changes

## What Changed

### 1. Simplified Request Payload
**Before:**
```json
{
  "provider": 1,
  "type": 1,
  "originalTransactionReference": "BSE-DEP-001",
  "amount": 100.00,
  "debitPartyMsisdn": "73001762",
  "creditPartyMsisdn": "70383747"
}
```

**After:**
```json
{
  "provider": 1,
  "amount": 100.00,
  "subscriberMsisdn": "73001762"
}
```

### 2. Auto-Generated Transaction References
- System now generates unique transaction references automatically
- Format: `{PREFIX}-{TIMESTAMP}-{RANDOM}`
- Example: `DEP-20251209160530123-A1B2C3`
- Prefix: `DEP` for deposits, `WD` for withdrawals

### 3. Merchant Details from Configuration
- Merchant number (70383747) pulled from database config
- Merchant PIN (4827) pulled from database config
- No need to pass these in API requests

### 4. Database Configuration
- Changed from InMemory to SQL Server
- Server: `DESKTOP-9RRSD5S`
- Database: `BSEPayments`
- Connection uses Windows Authentication (Trusted_Connection)

## API Endpoints

### Deposit (Subscriber → Merchant)
```
POST /api/payments/deposit
{
  "provider": "BTC",
  "amount": 100.00,
  "subscriberMsisdn": "73001762"
}
```

### Withdraw (Merchant → Subscriber)
```
POST /api/payments/withdraw
{
  "provider": "BTC",
  "amount": 50.00,
  "subscriberMsisdn": "73001762"
}
```

### Check Status
```
POST /api/payments/status
{
  "provider": "BTC",
  "transactionReference": "MP251209.155210620.000321",
  "useOriginalReference": false
}
```

## Setup Instructions

1. **Install SQL Server packages:**
   ```bash
   dotnet restore
   ```

2. **Create database (choose one):**
   
   **Option A - EF Migrations:**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
   
   **Option B - Manual SQL:**
   Run `Migrations/InitialCreate.sql` on DESKTOP-9RRSD5S

3. **Run the service:**
   ```bash
   dotnet run
   ```

4. **Test via Swagger:**
   Navigate to `https://localhost:5001/swagger`

## Benefits

✅ Simpler API for frontend developers
✅ Centralized merchant configuration
✅ Automatic transaction tracking
✅ Unique reference generation prevents duplicates
✅ Persistent SQL Server storage
