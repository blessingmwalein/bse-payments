using bse_payments.Models.DTOs.Responses;
using bse_payments.Services;
using Microsoft.AspNetCore.Mvc;

namespace bse_payments.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ClientService _clientService;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(ClientService clientService, ILogger<ClientsController> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    /// <summary>
    /// Get client's transaction history from CashTrans
    /// </summary>
    [HttpGet("{cdsNumber}/transactions")]
    public async Task<ActionResult<PaginatedResponse<CashTransResponse>>> GetClientTransactions(
        string cdsNumber,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? transType = null)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var result = await _clientService.GetClientTransactionsAsync(
                cdsNumber, page, pageSize, startDate, endDate, transType);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for client {CdsNumber}", cdsNumber);
            return StatusCode(500, new { error = "Failed to retrieve client transactions" });
        }
    }

    /// <summary>
    /// Get client's current balance and portfolio statistics
    /// </summary>
    [HttpGet("{cdsNumber}/balance")]
    public async Task<ActionResult<ClientBalanceResponse>> GetClientBalance(string cdsNumber)
    {
        try
        {
            var result = await _clientService.GetClientBalanceAsync(cdsNumber);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balance for client {CdsNumber}", cdsNumber);
            return StatusCode(500, new { error = "Failed to retrieve client balance" });
        }
    }
}
