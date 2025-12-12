# BSE Payment API - Quick Start Guide

## Provider Names (String-Based)

Instead of numbers, use these provider names in your API requests:

| Provider | String Value |
|----------|-------------|
| Botswana Telecommunications Corporation | `"BTC"` |
| Orange Botswana | `"ORANGE"` |
| Mascom Wireless | `"MASCOM"` |

## API Examples

### 1. Deposit (Subscriber pays Merchant)

**Endpoint:** `POST /api/payments/deposit`

**Request:**
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

**What happens:**
- System auto-generates transaction reference: `DEP-20251209160530123-A1B2C3`
- Merchant number (70383747) pulled from config
- Merchant PIN (4827) pulled from config
- Subscriber receives USSD prompt to confirm payment
- Status returned as readable string: "PAUSED"

---

### 2. Withdraw (Merchant pays Subscriber)

**Endpoint:** `POST /api/payments/withdraw`

**Request:**
```json
{
  "provider": "BTC",
  "cdsNumber": "CV00002GK",
  "amount": 50.00,
  "subscriberMsisdn": "73001762"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Cash In transaction of BWP 50 has been successfully completed",
  "transactionReference": "CI251209.155944712.000322",
  "originalTransactionReference": "WD-20251209160645789-B2C3D4",
  "status": "SUCCESS",
  "amount": 50.00
}
```

**What happens:**
- System auto-generates transaction reference: `WD-20251209160645789-B2C3D4`
- Merchant number (70383747) pulled from config
- Merchant PIN (4827) pulled from config
- Money sent immediately to subscriber

---

### 3. Check Transaction Status

**Endpoint:** `POST /api/payments/status`

**By Provider Reference:**
```json
{
  "provider": "BTC",
  "transactionReference": "MP251209.155210620.000321",
  "useOriginalReference": false
}
```

**By Your Original Reference:**
```json
{
  "provider": "BTC",
  "transactionReference": "DEP-20251209160530123-A1B2C3",
  "useOriginalReference": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "Success",
  "transactionReference": "MP251209.155210620.000321",
  "status": "SUCCESS",
  "amount": 100.00
}
```

---

## Transaction Status Values

The API returns status as readable strings:

| Status String | Description |
|---------------|-------------|
| `"PENDING"` | Transaction initiated |
| `"PAUSED"` | Awaiting USSD confirmation from subscriber |
| `"SUCCESS"` | Transaction completed successfully |
| `"FAILED"` | Transaction failed |
| `"CANCELLED"` | Transaction cancelled |

---

## Error Responses

**Invalid Provider:**
```json
{
  "success": false,
  "message": "Invalid provider: INVALID. Valid options: BTC, ORANGE, MASCOM"
}
```

**Provider Not Configured:**
```json
{
  "success": false,
  "message": "BTC provider not configured"
}
```

**Transaction Failed:**
```json
{
  "success": false,
  "message": "Transaction failed",
  "status": 4,
  "errorCode": "BadRequest"
}
```

---

## Frontend Integration Tips

### JavaScript/TypeScript Example

```typescript
// Deposit
const depositResponse = await fetch('https://api.bse.com/api/payments/deposit', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    provider: 'BTC',
    amount: 100.00,
    subscriberMsisdn: '73001762'
  })
});

const result = await depositResponse.json();
if (result.success) {
  console.log('Transaction Reference:', result.originalTransactionReference);
  // Save this reference to check status later
}
```

### React Example

```jsx
const handleDeposit = async (amount, msisdn) => {
  try {
    const response = await fetch('/api/payments/deposit', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        provider: 'BTC',
        amount: parseFloat(amount),
        subscriberMsisdn: msisdn
      })
    });
    
    const data = await response.json();
    
    if (data.success) {
      alert(`Transaction initiated! Reference: ${data.originalTransactionReference}`);
      // Poll for status or wait for webhook
    } else {
      alert(`Error: ${data.message}`);
    }
  } catch (error) {
    console.error('Payment error:', error);
  }
};
```

---

## Testing

Use Swagger UI at `https://localhost:5001/swagger` for interactive testing.

Or use the provided `.http` file in `Examples/api-examples.http` with REST Client extension in VS Code.
