using bse_payments.Models.Entities;
using bse_payments.Models.Enums;

namespace bse_payments.Data;

public static class SeedData
{
    public static async Task Initialize(PaymentDbContext context)
    {
        if (context.PaymentProviderConfigs.Any())
            return;

        var btcConfig = new PaymentProviderConfig
        {
            Provider = PaymentProvider.BTC,
            Username = "btc-dealer-1000212",
            Password = "pass1234", // In production, use secure storage
            MerchantNumber = "70383747",
            MerchantPin = "4827", // In production, encrypt this
            BaseUrl = "https://btcapps.btc.bw",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.PaymentProviderConfigs.Add(btcConfig);
        await context.SaveChangesAsync();
    }
}
