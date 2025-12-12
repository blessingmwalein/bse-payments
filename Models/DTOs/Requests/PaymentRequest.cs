namespace bse_payments.Models.DTOs.Requests;

public class PaymentRequest
{
    public string Provider { get; set; } = string.Empty;
    public string CdsNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string SubscriberMsisdn { get; set; } = string.Empty;
}
