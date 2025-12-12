namespace bse_payments.Models.DTOs.Responses;

public class CashTransResponse
{
    public long Id { get; set; }
    public string? Description { get; set; }
    public string TransType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DateCreated { get; set; }
    public string CdsNumber { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Ref2 { get; set; }
    public string? PostedBy { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CaptureDate { get; set; }
}
