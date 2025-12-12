namespace bse_payments.Models.DTOs.Responses;

public class PaymentTransactionResponse
{
    public int Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CdsNumber { get; set; } = string.Empty;
    public string OriginalTransactionReference { get; set; } = string.Empty;
    public string? ProviderTransactionReference { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string DebitPartyMsisdn { get; set; } = string.Empty;
    public string CreditPartyMsisdn { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool PostedToCashTrans { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
