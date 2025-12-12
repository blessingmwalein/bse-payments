using bse_payments.Models.Enums;

namespace bse_payments.Models.Entities;

public class PaymentTransaction
{
    public int Id { get; set; }
    public PaymentProvider Provider { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public string CdsNumber { get; set; } = string.Empty;
    public string OriginalTransactionReference { get; set; } = string.Empty;
    public string? ProviderTransactionReference { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BWP";
    public string DebitPartyMsisdn { get; set; } = string.Empty;
    public string CreditPartyMsisdn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RawRequest { get; set; }
    public string? RawResponse { get; set; }
    public bool PostedToCashTrans { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
