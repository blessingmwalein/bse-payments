using bse_payments.Models.DTOs.Requests;
using bse_payments.Models.DTOs.Responses;

namespace bse_payments.Services.Adapters;

public interface IPaymentAdapter
{
    Task<PaymentResponse> DepositAsync(PaymentRequest request);
    Task<PaymentResponse> WithdrawAsync(PaymentRequest request);
    Task<PaymentResponse> FinalizeWithdrawalAsync(string originalTransactionReference);
    Task<PaymentResponse> GetTransactionStatusAsync(TransactionStatusRequest request);
}
