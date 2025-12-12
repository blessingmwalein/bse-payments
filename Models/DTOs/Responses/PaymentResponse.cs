using System.Text.Json.Serialization;
using bse_payments.Models.Enums;

namespace bse_payments.Models.DTOs.Responses;

public class PaymentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public string? OriginalTransactionReference { get; set; }
    
    [JsonIgnore]
    public TransactionStatus Status { get; set; }
    
    [JsonPropertyName("status")]
    public string StatusText => Status switch
    {
        TransactionStatus.Pending => "PENDING",
        TransactionStatus.Paused => "PAUSED",
        TransactionStatus.Success => "SUCCESS",
        TransactionStatus.Failed => "FAILED",
        TransactionStatus.Cancelled => "CANCELLED",
        _ => "UNKNOWN"
    };
    
    public decimal? Amount { get; set; }
    public string? ErrorCode { get; set; }
}
