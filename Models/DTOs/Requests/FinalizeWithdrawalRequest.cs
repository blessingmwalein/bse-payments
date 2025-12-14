using System.ComponentModel.DataAnnotations;

namespace bse_payments.Models.DTOs.Requests;

public class FinalizeWithdrawalRequest
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string OriginalTransactionReference { get; set; } = string.Empty;
}
