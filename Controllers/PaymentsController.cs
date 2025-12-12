using Microsoft.AspNetCore.Mvc;
using bse_payments.Models.DTOs.Requests;
using bse_payments.Services;
using bse_payments.Helpers;

namespace bse_payments.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(PaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Initiate a deposit transaction (subscriber pays merchant)
    /// </summary>
    /// <remarks>
    /// Provider options: BTC, ORANGE, MASCOM
    /// </remarks>
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] PaymentRequest request)
    {
        if (!ProviderMapper.IsValidProvider(request.Provider))
        {
            return BadRequest(new { success = false, message = $"Invalid provider: {request.Provider}. Valid options: BTC, ORANGE, MASCOM" });
        }

        _logger.LogInformation("Deposit request for {Provider}: Amount={Amount}, Subscriber={Subscriber}", 
            request.Provider, request.Amount, request.SubscriberMsisdn);
        var result = await _paymentService.DepositAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Initiate a withdrawal transaction (merchant pays subscriber)
    /// </summary>
    /// <remarks>
    /// Provider options: BTC, ORANGE, MASCOM
    /// </remarks>
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] PaymentRequest request)
    {
        if (!ProviderMapper.IsValidProvider(request.Provider))
        {
            return BadRequest(new { success = false, message = $"Invalid provider: {request.Provider}. Valid options: BTC, ORANGE, MASCOM" });
        }

        _logger.LogInformation("Withdraw request for {Provider}: Amount={Amount}, Subscriber={Subscriber}", 
            request.Provider, request.Amount, request.SubscriberMsisdn);
        var result = await _paymentService.WithdrawAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Check transaction status
    /// </summary>
    /// <remarks>
    /// Provider options: BTC, ORANGE, MASCOM
    /// </remarks>
    [HttpPost("status")]
    public async Task<IActionResult> GetStatus([FromBody] TransactionStatusRequest request)
    {
        if (!ProviderMapper.IsValidProvider(request.Provider))
        {
            return BadRequest(new { success = false, message = $"Invalid provider: {request.Provider}. Valid options: BTC, ORANGE, MASCOM" });
        }

        _logger.LogInformation("Status check for {Provider}: {Reference}", request.Provider, request.TransactionReference);
        var result = await _paymentService.GetTransactionStatusAsync(request);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
