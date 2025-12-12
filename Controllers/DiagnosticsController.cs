using Microsoft.AspNetCore.Mvc;
using bse_payments.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using bse_payments.Data;

namespace bse_payments.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly IPaymentRepository _repository;
    private readonly PaymentDbContext _paymentContext;
    private readonly BoDbContext _boContext;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(
        IPaymentRepository repository,
        PaymentDbContext paymentContext,
        BoDbContext boContext,
        ILogger<DiagnosticsController> logger)
    {
        _repository = repository;
        _paymentContext = paymentContext;
        _boContext = boContext;
        _logger = logger;
    }

    /// <summary>
    /// Get transaction details by reference
    /// </summary>
    [HttpGet("transaction/{reference}")]
    public async Task<IActionResult> GetTransaction(string reference)
    {
        var transaction = await _paymentContext.PaymentTransactions
            .FirstOrDefaultAsync(t => 
                t.ProviderTransactionReference == reference || 
                t.OriginalTransactionReference == reference);

        if (transaction == null)
            return NotFound(new { message = "Transaction not found", reference });

        return Ok(new
        {
            id = transaction.Id,
            provider = transaction.Provider.ToString(),
            type = transaction.Type.ToString(),
            status = transaction.Status.ToString(),
            cdsNumber = transaction.CdsNumber,
            originalTransactionReference = transaction.OriginalTransactionReference,
            providerTransactionReference = transaction.ProviderTransactionReference,
            amount = transaction.Amount,
            currency = transaction.Currency,
            debitPartyMsisdn = transaction.DebitPartyMsisdn,
            creditPartyMsisdn = transaction.CreditPartyMsisdn,
            description = transaction.Description,
            postedToCashTrans = transaction.PostedToCashTrans,
            createdAt = transaction.CreatedAt,
            updatedAt = transaction.UpdatedAt
        });
    }

    /// <summary>
    /// Check if transaction was posted to CashTrans
    /// </summary>
    [HttpGet("cashtrans/check/{cdsNumber}")]
    public async Task<IActionResult> CheckCashTrans(string cdsNumber)
    {
        var records = await _boContext.CashTrans
            .Where(c => c.CDS_Number == cdsNumber)
            .OrderByDescending(c => c.DateCreated)
            .Take(10)
            .Select(c => new
            {
                id = c.ID,
                description = c.Description,
                amount = c.Amount,
                cdsNumber = c.CDS_Number,
                reference = c.Reference,
                ref2 = c.Ref2,
                postedBy = c.PostedBy,
                dateCreated = c.DateCreated
            })
            .ToListAsync();

        return Ok(new
        {
            cdsNumber,
            recordCount = records.Count,
            records
        });
    }

    /// <summary>
    /// Get recent transactions
    /// </summary>
    [HttpGet("transactions/recent")]
    public async Task<IActionResult> GetRecentTransactions([FromQuery] int count = 10)
    {
        var transactions = await _paymentContext.PaymentTransactions
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Select(t => new
            {
                id = t.Id,
                provider = t.Provider.ToString(),
                type = t.Type.ToString(),
                status = t.Status.ToString(),
                cdsNumber = t.CdsNumber,
                originalRef = t.OriginalTransactionReference,
                providerRef = t.ProviderTransactionReference,
                amount = t.Amount,
                postedToCashTrans = t.PostedToCashTrans,
                createdAt = t.CreatedAt
            })
            .ToListAsync();

        return Ok(transactions);
    }
}
