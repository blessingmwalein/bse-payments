namespace bse_payments.Models.DTOs.Requests;

public class TransactionStatusRequest
{
    public string Provider { get; set; } = string.Empty;
    public string TransactionReference { get; set; } = string.Empty;
    public bool UseOriginalReference { get; set; } = false;
}
