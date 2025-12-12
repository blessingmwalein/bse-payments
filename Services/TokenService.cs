using bse_payments.Data.Repositories;
using bse_payments.Models.Entities;
using bse_payments.Models.Enums;

namespace bse_payments.Services;

public class TokenService
{
    private readonly IPaymentRepository _repository;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IPaymentRepository repository, ILogger<TokenService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<string?> GetValidTokenAsync(PaymentProvider provider)
    {
        var token = await _repository.GetValidTokenAsync(provider);
        if (token != null)
        {
            _logger.LogInformation("Using cached token for {Provider}", provider);
            return token.AccessToken;
        }
        return null;
    }

    public async Task SaveTokenAsync(PaymentProvider provider, string accessToken, int expiresInSeconds = 300)
    {
        var token = new ProviderToken
        {
            Provider = provider,
            AccessToken = accessToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds - 30) // 30s buffer
        };
        await _repository.SaveTokenAsync(token);
        _logger.LogInformation("Saved new token for {Provider}, expires at {ExpiresAt}", provider, token.ExpiresAt);
    }
}
