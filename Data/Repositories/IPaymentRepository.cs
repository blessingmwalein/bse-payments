using bse_payments.Models.Entities;
using bse_payments.Models.Enums;

namespace bse_payments.Data.Repositories;

public interface IPaymentRepository
{
    Task<PaymentProviderConfig?> GetProviderConfigAsync(PaymentProvider provider);
    Task<PaymentTransaction> CreateTransactionAsync(PaymentTransaction transaction);
    Task<PaymentTransaction?> GetTransactionByReferenceAsync(string reference, bool isOriginalReference = false);
    Task UpdateTransactionAsync(PaymentTransaction transaction);
    Task<ProviderToken?> GetValidTokenAsync(PaymentProvider provider);
    Task SaveTokenAsync(ProviderToken token);
}
