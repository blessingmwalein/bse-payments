# Provider String Mapping

## Overview

The API now accepts provider names as **strings** instead of numeric IDs, making it more intuitive for frontend developers.

## Valid Provider Strings

| String | Provider | Status |
|--------|----------|--------|
| `"BTC"` | Botswana Telecommunications Corporation | ✅ Active |
| `"ORANGE"` | Orange Botswana | 🔜 Coming Soon |
| `"MASCOM"` | Mascom Wireless | 🔜 Coming Soon |

## How It Works

### Backend Mapping

The `ProviderMapper` helper class converts string values to internal enum values:

```csharp
// Helpers/ProviderMapper.cs
public static PaymentProvider? MapToEnum(string providerName)
{
    return providerName?.ToUpper() switch
    {
        "BTC" => PaymentProvider.BTC,
        "ORANGE" => PaymentProvider.Orange,
        "MASCOM" => PaymentProvider.Mascom,
        _ => null
    };
}
```

### Case Insensitive

The API accepts provider names in any case:
- `"BTC"` ✅
- `"btc"` ✅
- `"Btc"` ✅
- `"bTc"` ✅

### Validation

Invalid provider names return a clear error:

**Request:**
```json
{
  "provider": "INVALID",
  "amount": 100.00,
  "subscriberMsisdn": "73001762"
}
```

**Response:**
```json
{
  "success": false,
  "message": "Invalid provider: INVALID. Valid options: BTC, ORANGE, MASCOM"
}
```

## Database Storage

Internally, providers are still stored as integers in the database:

| Enum Value | Database ID | String Representation |
|------------|-------------|----------------------|
| PaymentProvider.BTC | 1 | "BTC" |
| PaymentProvider.Orange | 2 | "ORANGE" |
| PaymentProvider.Mascom | 3 | "MASCOM" |

This ensures:
- Efficient database storage
- Easy querying and indexing
- Backward compatibility
- Clean API interface

## Adding New Providers

To add a new provider (e.g., "MYNETWORK"):

1. **Update Enum** (`Models/Enums/PaymentProvider.cs`):
```csharp
public enum PaymentProvider
{
    BTC = 1,
    Orange = 2,
    Mascom = 3,
    MyNetwork = 4  // Add new provider
}
```

2. **Update Mapper** (`Helpers/ProviderMapper.cs`):
```csharp
public static PaymentProvider? MapToEnum(string providerName)
{
    return providerName?.ToUpper() switch
    {
        "BTC" => PaymentProvider.BTC,
        "ORANGE" => PaymentProvider.Orange,
        "MASCOM" => PaymentProvider.Mascom,
        "MYNETWORK" => PaymentProvider.MyNetwork,  // Add mapping
        _ => null
    };
}
```

3. **Create Adapter** (`Services/Adapters/MyNetworkAdapter.cs`):
```csharp
public class MyNetworkAdapter : IPaymentAdapter
{
    // Implement interface methods
}
```

4. **Register in DI** (`Program.cs`):
```csharp
builder.Services.AddScoped<MyNetworkAdapter>();
```

5. **Update Service** (`Services/PaymentService.cs`):
```csharp
private IPaymentAdapter? GetAdapter(PaymentProvider provider)
{
    return provider switch
    {
        PaymentProvider.BTC => _serviceProvider.GetService<BtcAdapter>(),
        PaymentProvider.Orange => _serviceProvider.GetService<OrangeAdapter>(),
        PaymentProvider.Mascom => _serviceProvider.GetService<MascomAdapter>(),
        PaymentProvider.MyNetwork => _serviceProvider.GetService<MyNetworkAdapter>(),
        _ => null
    };
}
```

6. **Add Configuration** (Database):
```sql
INSERT INTO PaymentProviderConfigs (Provider, Username, Password, MerchantNumber, MerchantPin, BaseUrl, IsActive)
VALUES (4, 'mynetwork-user', 'password', '12345678', '1234', 'https://api.mynetwork.com', 1);
```

That's it! The new provider is now available via the API using `"MYNETWORK"` as the provider string.
