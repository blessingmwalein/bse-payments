using bse_payments.Models.DTOs.Responses;
using bse_payments.Services;
using Microsoft.AspNetCore.Mvc;

namespace bse_payments.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ClientService _clientService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ClientService clientService, ILogger<TransactionsController> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    /// <summary>
    /// Get payment transactions with filters and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<PaymentTransactionResponse>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cdsNumber = null,
        [FromQuery] string? provider = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var result = await _clientService.GetPaymentTransactionsAsync(
                page, pageSize, cdsNumber, provider, status, startDate, endDate);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment transactions");
            return StatusCode(500, new { error = "Failed to retrieve payment transactions" });
        }
    }
}
