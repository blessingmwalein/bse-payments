# BSE Payment Service

Backend payment gateway for Botswana Stock Exchange mobile money integration.

## Architecture

- **Adapter Pattern**: Easily switch between payment providers (BTC, Orange, Mascom)
- **Repository Pattern**: Clean data access layer
- **Token Management**: Automatic token caching and refresh
- **Generic Transaction Tracking**: Provider-agnostic transaction storage

## Features

- ✅ Deposit (Merchant Pay)
- ✅ Withdraw (Disbursement/Cash-in)
- ✅ Transaction Status Check
- ✅ Multi-provider support (currently BTC)
- ✅ Automatic token management
- ✅ Comprehensive Swagger documentation

## Database Schema

### PaymentProviderConfig
Stores credentials and configuration for each MNO provider.

### ProviderToken
Caches authentication tokens with expiry tracking.

### PaymentTransaction
Generic transaction log supporting all providers and transaction types.

## API Endpoints

### POST /api/payments/deposit
Initiate a deposit transaction (subscriber pays merchant).

**Request:** (Simplified - only subscriber number needed!)
```json
{
  "provider": "BTC",
  "amount": 100.00,
  "subscriberMsisdn": "73001762"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Your transaction request has been initiated, and is now pending confirmation",
  "transactionReference": "MP251209.155210620.000321",
  "originalTransactionReference": "DEP-20251209160530123-A1B2C3",
  "status": "PAUSED",
  "amount": 100.00
}
```

**What happens automatically:**
- Transaction reference is auto-generated with timestamp
- Merchant number pulled from config (70383747)
- Merchant PIN pulled from config (4827)

### POST /api/payments/withdraw
Initiate a withdrawal (merchant pays subscriber).

**Request:** (Simplified - only subscriber number needed!)
```json
{
  "provider": "BTC",
  "amount": 50.00,
  "subscriberMsisdn": "73001762"
}
```

### POST /api/payments/status
Check transaction status.

**Request:**
```json
{
  "provider": 1,
  "transactionReference": "MP251209.155210620.000321",
  "useOriginalReference": false
}
```

## Provider Options

Use these string values in the `provider` field:
- `"BTC"` - Botswana Telecommunications Corporation
- `"ORANGE"` - Orange Botswana (coming soon)
- `"MASCOM"` - Mascom Wireless (coming soon)

## Transaction Status Values

The API returns status as a string:

- `"PENDING"` - Transaction initiated
- `"PAUSED"` - Awaiting USSD confirmation from subscriber
- `"SUCCESS"` - Transaction completed successfully
- `"FAILED"` - Transaction failed
- `"CANCELLED"` - Transaction cancelled

## Database Setup

The service connects to SQL Server on `DESKTOP-9RRSD5S`.

**Option 1: Using EF Migrations**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**Option 2: Manual SQL Script**
Run the script in `Migrations/InitialCreate.sql` on your SQL Server.

## Running the Service

```bash
dotnet run
```

Access Swagger UI at: `https://localhost:5001/swagger`

## Adding New Providers

1. Create new adapter implementing `IPaymentAdapter`
2. Add provider enum to `PaymentProvider`
3. Register adapter in `Program.cs`
4. Add configuration to database
5. Update `PaymentService.GetAdapter()` switch

## Security Notes

⚠️ **Production Considerations:**
- Store credentials in Azure Key Vault or similar
- Encrypt merchant PIN in database
- Implement API authentication (JWT/OAuth)
- Add rate limiting
- Enable HTTPS only
- Implement webhook callbacks for async status updates
# bse-payments
