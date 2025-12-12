using bse_payments.Models.Enums;

namespace bse_payments.Models.Entities;

public class PaymentProviderConfig
{
    public int Id { get; set; }
    public PaymentProvider Provider { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string MerchantNumber { get; set; } = string.Empty;
    public string MerchantPin { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
