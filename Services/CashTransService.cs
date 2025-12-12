using bse_payments.Data;
using bse_payments.Models.Entities;
using bse_payments.Models.Enums;

namespace bse_payments.Services;

public class CashTransService
{
    private readonly BoDbContext _boContext;
    private readonly ILogger<CashTransService> _logger;

    public CashTransService(BoDbContext boContext, ILogger<CashTransService> logger)
    {
        _boContext = boContext;
        _logger = logger;
    }

    public async Task<bool> PostTransactionAsync(PaymentTransaction transaction, string postedBy = "bse-api")
    {
        try
        {
            // Check if already posted
            if (transaction.PostedToCashTrans)
            {
                _logger.LogInformation("Transaction {TransactionRef} already posted to CashTrans", 
                    transaction.OriginalTransactionReference);
                return false;
            }

            // Only post successful transactions
            if (transaction.Status != TransactionStatus.Success)
            {
                _logger.LogWarning("Transaction {TransactionRef} status is {Status}, not posting to CashTrans", 
                    transaction.OriginalTransactionReference, transaction.Status);
                return false;
            }

            // Check CDS Number
            if (string.IsNullOrEmpty(transaction.CdsNumber))
            {
                _logger.LogError("Transaction {TransactionRef} has no CDS Number, cannot post to CashTrans", 
                    transaction.OriginalTransactionReference);
                return false;
            }

            // For withdrawals, amount should be negative to deduct from balance
            var amount = transaction.Type == TransactionType.Withdraw 
                ? -Math.Abs(transaction.Amount) 
                : transaction.Amount;

            var cashTrans = new CashTrans
            {
                Description = transaction.Type == TransactionType.Deposit 
                    ? $"Mobile Money Deposit - {transaction.Provider}" 
                    : $"Mobile Money Withdrawal - {transaction.Provider}",
                TransType = transaction.Type == TransactionType.Deposit 
                    ? "Account Deposit" 
                    : "Account Withdrawal",
                Amount = amount,
                DateCreated = DateTime.Now,
                TransStatus = "1",
                CDS_Number = transaction.CdsNumber,
                Paid = true,
                Reference = transaction.OriginalTransactionReference,
                PostedBy = postedBy,
                Currency = transaction.Currency,
                CaptureDate = DateTime.Now,
                Ref2 = transaction.ProviderTransactionReference
            };

            _logger.LogInformation("Posting to CashTrans: CDS={CdsNumber}, Amount={Amount}, Ref={Ref}", 
                cashTrans.CDS_Number, cashTrans.Amount, cashTrans.Reference);

            _boContext.CashTrans.Add(cashTrans);
            await _boContext.SaveChangesAsync();

            _logger.LogInformation("Successfully posted transaction {TransactionRef} to CashTrans for CDS {CdsNumber}", 
                transaction.OriginalTransactionReference, transaction.CdsNumber);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting transaction {TransactionRef} to CashTrans: {Error}", 
                transaction.OriginalTransactionReference, ex.Message);
            return false;
        }
    }
}
