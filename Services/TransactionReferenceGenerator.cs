namespace bse_payments.Services;

public static class TransactionReferenceGenerator
{
    public static string Generate(string prefix = "BSE")
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var random = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"{prefix}-{timestamp}-{random}";
    }
}
