namespace bse_payments.Models.DTOs.Responses;

public class ClientBalanceResponse
{
    public string CdsNumber { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = "BWP";
    public decimal WeeklyChange { get; set; }
    public decimal WeeklyChangePercentage { get; set; }
    public int TotalTransactions { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}
