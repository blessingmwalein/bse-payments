using bse_payments.Models.Enums;

namespace bse_payments.Helpers;

public static class ProviderMapper
{
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

    public static string MapToString(PaymentProvider provider)
    {
        return provider switch
        {
            PaymentProvider.BTC => "BTC",
            PaymentProvider.Orange => "ORANGE",
            PaymentProvider.Mascom => "MASCOM",
            _ => "UNKNOWN"
        };
    }

    public static bool IsValidProvider(string providerName)
    {
        return MapToEnum(providerName) != null;
    }
}
