using System.Text;
using System.Text.Json;
using bse_payments.Data.Repositories;
using bse_payments.Models.DTOs.Requests;
using bse_payments.Models.DTOs.Responses;
using bse_payments.Models.Entities;
using bse_payments.Models.Enums;
using bse_payments.Services;
using bse_payments.Helpers;

namespace bse_payments.Services.Adapters;

public class BtcAdapter : IPaymentAdapter
{
    private readonly IPaymentRepository _repository;
    private readonly TokenService _tokenService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BtcAdapter> _logger;

    public BtcAdapter(
        IPaymentRepository repository,
        TokenService tokenService,
        IHttpClientFactory httpClientFactory,
        ILogger<BtcAdapter> logger)
    {
        _repository = repository;
        _tokenService = tokenService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<PaymentResponse> DepositAsync(PaymentRequest request)
    {
        var config = await _repository.GetProviderConfigAsync(PaymentProvider.BTC);
        if (config == null)
            return new PaymentResponse { Success = false, Message = "BTC provider not configured" };

        var token = await GetOrRefreshTokenAsync(config);
        if (string.IsNullOrEmpty(token))
            return new PaymentResponse { Success = false, Message = "Failed to obtain authentication token" };

        // Auto-generate transaction reference
        var originalRef = TransactionReferenceGenerator.Generate("DEP");

        var transaction = new PaymentTransaction
        {
            Provider = PaymentProvider.BTC,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Pending,
            CdsNumber = request.CdsNumber,
            OriginalTransactionReference = originalRef,
            Amount = request.Amount,
            DebitPartyMsisdn = request.SubscriberMsisdn, // Subscriber pays
            CreditPartyMsisdn = config.MerchantNumber // Merchant receives
        };

        var payload = new
        {
            originalTransactionReference = originalRef,
            amount = request.Amount.ToString("F2"),
            creditParty = new[] { new { key = "msisdn", value = config.MerchantNumber } },
            debitParty = new[] { new { key = "msisdn", value = request.SubscriberMsisdn } },
            customData = new[] { new { key = "mpin", value = config.MerchantPin } }
        };

        transaction.RawRequest = JsonSerializer.Serialize(payload);

        try
        {
            var client = _httpClientFactory.CreateClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{config.BaseUrl}/mfs-ocpdev/gsma/transactions/type/merchantpay");
            httpRequest.Headers.Add("Authorization", $"Bearer {token}");
            httpRequest.Headers.Add("X-Account-Holding-Institution-Identifier", "smega");
            httpRequest.Headers.Add("X-Account-Holding-Institution-Identifier-Type", "organisationid");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();
            transaction.RawResponse = responseBody;

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
                transaction.ProviderTransactionReference = result.GetProperty("transactionReference").GetString();
                transaction.Description = result.GetProperty("descriptionText").GetString();
                transaction.Status = TransactionStatus.Paused;

                await _repository.CreateTransactionAsync(transaction);

                return new PaymentResponse
                {
                    Success = true,
                    Message = transaction.Description ?? "Transaction initiated",
                    TransactionReference = transaction.ProviderTransactionReference,
                    OriginalTransactionReference = originalRef,
                    Status = TransactionStatus.Paused,
                    Amount = request.Amount
                };
            }
            else
            {
                transaction.Status = TransactionStatus.Failed;
                transaction.ErrorMessage = responseBody;
                await _repository.CreateTransactionAsync(transaction);

                return new PaymentResponse
                {
                    Success = false,
                    Message = "Transaction failed",
                    Status = TransactionStatus.Failed,
                    ErrorCode = response.StatusCode.ToString()
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing deposit");
            transaction.Status = TransactionStatus.Failed;
            transaction.ErrorMessage = ex.Message;
            await _repository.CreateTransactionAsync(transaction);

            return new PaymentResponse { Success = false, Message = ex.Message, Status = TransactionStatus.Failed };
        }
    }

    public async Task<PaymentResponse> WithdrawAsync(PaymentRequest request)
    {
        var config = await _repository.GetProviderConfigAsync(PaymentProvider.BTC);
        if (config == null)
            return new PaymentResponse { Success = false, Message = "BTC provider not configured" };

        var token = await GetOrRefreshTokenAsync(config);
        if (string.IsNullOrEmpty(token))
            return new PaymentResponse { Success = false, Message = "Failed to obtain authentication token" };

        // Auto-generate transaction reference
        var originalRef = TransactionReferenceGenerator.Generate("WD");

        var transaction = new PaymentTransaction
        {
            Provider = PaymentProvider.BTC,
            Type = TransactionType.Withdraw,
            Status = TransactionStatus.Pending,
            CdsNumber = request.CdsNumber,
            OriginalTransactionReference = originalRef,
            Amount = request.Amount,
            DebitPartyMsisdn = config.MerchantNumber, // Merchant pays
            CreditPartyMsisdn = request.SubscriberMsisdn // Subscriber receives
        };

        var payload = new
        {
            originalTransactionReference = originalRef,
            subType = "cash-in",
            amount = request.Amount.ToString("F2"),
            creditParty = new[] { new { key = "msisdn", value = request.SubscriberMsisdn } },
            debitParty = new[] { new { key = "msisdn", value = config.MerchantNumber } },
            customData = new[] { new { key = "pin", value = config.MerchantPin } }
        };

        transaction.RawRequest = JsonSerializer.Serialize(payload);

        try
        {
            var client = _httpClientFactory.CreateClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{config.BaseUrl}/mfs-ocpdev/gsma/transactions/type/disbursement");
            httpRequest.Headers.Add("Authorization", $"Bearer {token}");
            httpRequest.Headers.Add("X-Account-Holding-Institution-Identifier", "smega");
            httpRequest.Headers.Add("X-Account-Holding-Institution-Identifier-Type", "organisationid");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();
            transaction.RawResponse = responseBody;

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
                transaction.ProviderTransactionReference = result.GetProperty("transactionReference").GetString();
                transaction.Description = result.GetProperty("descriptionText").GetString();
                transaction.Status = TransactionStatus.Success;

                await _repository.CreateTransactionAsync(transaction);

                return new PaymentResponse
                {
                    Success = true,
                    Message = transaction.Description ?? "Withdrawal successful",
                    TransactionReference = transaction.ProviderTransactionReference,
                    OriginalTransactionReference = originalRef,
                    Status = TransactionStatus.Success,
                    Amount = request.Amount
                };
            }
            else
            {
                transaction.Status = TransactionStatus.Failed;
                transaction.ErrorMessage = responseBody;
                await _repository.CreateTransactionAsync(transaction);

                return new PaymentResponse
                {
                    Success = false,
                    Message = "Withdrawal failed",
                    Status = TransactionStatus.Failed,
                    ErrorCode = response.StatusCode.ToString()
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing withdrawal");
            transaction.Status = TransactionStatus.Failed;
            transaction.ErrorMessage = ex.Message;
            await _repository.CreateTransactionAsync(transaction);

            return new PaymentResponse { Success = false, Message = ex.Message, Status = TransactionStatus.Failed };
        }
    }

    public async Task<PaymentResponse> GetTransactionStatusAsync(TransactionStatusRequest request)
    {
        var config = await _repository.GetProviderConfigAsync(PaymentProvider.BTC);
        if (config == null)
            return new PaymentResponse { Success = false, Message = "BTC provider not configured" };

        var token = await GetOrRefreshTokenAsync(config);
        if (string.IsNullOrEmpty(token))
            return new PaymentResponse { Success = false, Message = "Failed to obtain authentication token" };

        try
        {
            var endpoint = request.UseOriginalReference ? "external" : "internal";
            var client = _httpClientFactory.CreateClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, 
                $"{config.BaseUrl}/mfs-ocpdev/gsma/transactions/{endpoint}/{request.TransactionReference}");
            httpRequest.Headers.Add("Authorization", $"Bearer {token}");
            httpRequest.Headers.Add("X-Account-Holding-Institution-Identifier", "smega");
            httpRequest.Headers.Add("X-Account-Holding-Institution-Identifier-Type", "organisationid");

            var response = await client.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
                var amount = decimal.Parse(result.GetProperty("amount").GetString() ?? "0");
                var description = result.GetProperty("descriptionText").GetString();
                var transactionRef = result.GetProperty("transactionReference").GetString();

                var status = description?.ToLower().Contains("success") == true 
                    ? TransactionStatus.Success 
                    : TransactionStatus.Pending;

                // Update transaction status in database
                var transaction = await _repository.GetTransactionByReferenceAsync(
                    request.TransactionReference, 
                    request.UseOriginalReference);

                if (transaction != null && transaction.Status != status)
                {
                    _logger.LogInformation("Updating transaction {Ref} status from {OldStatus} to {NewStatus}", 
                        request.TransactionReference, transaction.Status, status);
                    
                    transaction.Status = status;
                    transaction.Description = description;
                    await _repository.UpdateTransactionAsync(transaction);
                }

                return new PaymentResponse
                {
                    Success = true,
                    Message = description ?? "Transaction found",
                    TransactionReference = transactionRef,
                    Status = status,
                    Amount = amount
                };
            }
            else
            {
                return new PaymentResponse
                {
                    Success = false,
                    Message = "Transaction not found",
                    ErrorCode = response.StatusCode.ToString()
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking transaction status");
            return new PaymentResponse { Success = false, Message = ex.Message };
        }
    }

    private async Task<string?> GetOrRefreshTokenAsync(PaymentProviderConfig config)
    {
        var cachedToken = await _tokenService.GetValidTokenAsync(PaymentProvider.BTC);
        if (!string.IsNullOrEmpty(cachedToken))
            return cachedToken;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var payload = new { username = config.Username, password = config.Password };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{config.BaseUrl}/security-services", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
                var accessToken = result.GetProperty("access_token").GetString();
                
                if (!string.IsNullOrEmpty(accessToken))
                {
                    await _tokenService.SaveTokenAsync(PaymentProvider.BTC, accessToken);
                    return accessToken;
                }
            }

            _logger.LogError("Failed to obtain token: {Response}", responseBody);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obtaining token");
            return null;
        }
    }
}
