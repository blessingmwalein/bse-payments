using bse_payments.Models.Enums;

namespace bse_payments.Models.Entities;

public class ProviderToken
{
    public int Id { get; set; }
    public PaymentProvider Provider { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
