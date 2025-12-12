namespace bse_payments.Models.Entities;

public class CashTrans
{
    public long ID { get; set; }
    public string? Description { get; set; }
    public string? TransType { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateCreated { get; set; }
    public string? TransStatus { get; set; }
    public string CDS_Number { get; set; } = string.Empty;
    public bool Paid { get; set; } = true;
    public string? Reference { get; set; }
    public string? ChargeCode { get; set; }
    public string? AssetManager { get; set; }
    public string? BankAccount { get; set; }
    public string? Ref2 { get; set; }
    public string? PostedBy { get; set; }
    public string? Currency { get; set; }
    public DateTime CaptureDate { get; set; }
    public string? SBZBankName { get; set; }
    public string? SBZBankAccountNo { get; set; }
    public bool? PostToPastel { get; set; }
}
