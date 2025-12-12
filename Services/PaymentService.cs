using bse_payments.Models.DTOs.Requests;
using bse_payments.Models.DTOs.Responses;
using bse_payments.Models.Enums;
using bse_payments.Services.Adapters;
using bse_payments.Helpers;
using bse_payments.Data.Repositories;

namespace bse_payments.Services;

public class PaymentService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPaymentRepository _repository;
    private readonly CashTransService _cashTransService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IServiceProvider serviceProvider, 
        IPaymentRepository repository,
        CashTransService cashTransService,
        ILogger<PaymentService> logger)
    {
        _serviceProvider = serviceProvider;
        _repository = repository;
        _cashTransService = cashTransService;
        _logger = logger;
    }

    public async Task<PaymentResponse> DepositAsync(PaymentRequest request)
    {
        var providerEnum = ProviderMapper.MapToEnum(request.Provider);
        if (providerEnum == null)
            return new PaymentResponse { Success = false, Message = $"Invalid provider: {request.Provider}" };

        var adapter = GetAdapter(providerEnum.Value);
        if (adapter == null)
            return new PaymentResponse { Success = false, Message = $"Provider {request.Provider} not supported yet" };

        return await adapter.DepositAsync(request);
    }

    public async Task<PaymentResponse> WithdrawAsync(PaymentRequest request)
    {
        var providerEnum = ProviderMapper.MapToEnum(request.Provider);
        if (providerEnum == null)
            return new PaymentResponse { Success = false, Message = $"Invalid provider: {request.Provider}" };

        var adapter = GetAdapter(providerEnum.Value);
        if (adapter == null)
            return new PaymentResponse { Success = false, Message = $"Provider {request.Provider} not supported yet" };

        return await adapter.WithdrawAsync(request);
    }

    public async Task<PaymentResponse> GetTransactionStatusAsync(TransactionStatusRequest request)
    {
        var providerEnum = ProviderMapper.MapToEnum(request.Provider);
        if (providerEnum == null)
            return new PaymentResponse { Success = false, Message = $"Invalid provider: {request.Provider}" };

        var adapter = GetAdapter(providerEnum.Value);
        if (adapter == null)
            return new PaymentResponse { Success = false, Message = $"Provider {request.Provider} not supported yet" };

        var response = await adapter.GetTransactionStatusAsync(request);

        // If transaction is successful, check if we need to post to CashTrans
        if (response.Success && response.Status == TransactionStatus.Success)
        {
            _logger.LogInformation("Transaction {Ref} is successful, checking if needs posting to CashTrans", 
                request.TransactionReference);

            // Try to find transaction by provider reference first, then by original reference
            var transaction = await _repository.GetTransactionByReferenceAsync(
                request.TransactionReference, 
                request.UseOriginalReference);

            if (transaction == null)
            {
                _logger.LogWarning("Transaction {Ref} not found in database, cannot post to CashTrans", 
                    request.TransactionReference);
            }
            else if (transaction.PostedToCashTrans)
            {
                _logger.LogInformation("Transaction {Ref} already posted to CashTrans", 
                    request.TransactionReference);
            }
            else
            {
                _logger.LogInformation("Posting transaction {Ref} to CashTrans for CDS {CdsNumber}", 
                    request.TransactionReference, transaction.CdsNumber);

                var posted = await _cashTransService.PostTransactionAsync(transaction);
                if (posted)
                {
                    transaction.PostedToCashTrans = true;
                    await _repository.UpdateTransactionAsync(transaction);
                    _logger.LogInformation("Successfully posted transaction {Ref} to CashTrans", 
                        request.TransactionReference);
                }
                else
                {
                    _logger.LogError("Failed to post transaction {Ref} to CashTrans", 
                        request.TransactionReference);
                }
            }
        }

        return response;
    }

    private IPaymentAdapter? GetAdapter(PaymentProvider provider)
    {
        return provider switch
        {
            PaymentProvider.BTC => _serviceProvider.GetService<BtcAdapter>(),
            // PaymentProvider.Orange => _serviceProvider.GetService<OrangeAdapter>(),
            // PaymentProvider.Mascom => _serviceProvider.GetService<MascomAdapter>(),
            _ => null
        };
    }
}
