using bse_payments.Data;
using bse_payments.Models.DTOs.Responses;
using bse_payments.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace bse_payments.Services;

public class ClientService
{
    private readonly BoDbContext _boContext;
    private readonly PaymentDbContext _paymentContext;
    private readonly ILogger<ClientService> _logger;

    public ClientService(
        BoDbContext boContext, 
        PaymentDbContext paymentContext,
        ILogger<ClientService> logger)
    {
        _boContext = boContext;
        _paymentContext = paymentContext;
        _logger = logger;
    }

    public async Task<PaginatedResponse<CashTransResponse>> GetClientTransactionsAsync(
        string cdsNumber, 
        int page = 1, 
        int pageSize = 20,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? transType = null)
    {
        try
        {
            var query = _boContext.CashTrans
                .Where(ct => ct.CDS_Number == cdsNumber);

            // Apply filters
            if (startDate.HasValue)
                query = query.Where(ct => ct.DateCreated >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(ct => ct.DateCreated <= endDate.Value);

            if (!string.IsNullOrEmpty(transType))
                query = query.Where(ct => ct.TransType == transType);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var transactions = await query
                .OrderByDescending(ct => ct.DateCreated)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ct => new CashTransResponse
                {
                    Id = ct.ID,
                    Description = ct.Description,
                    TransType = ct.TransType,
                    Amount = ct.Amount,
                    DateCreated = ct.DateCreated,
                    CdsNumber = ct.CDS_Number,
                    Reference = ct.Reference,
                    Ref2 = ct.Ref2,
                    PostedBy = ct.PostedBy,
                    Currency = ct.Currency,
                    CaptureDate = ct.CaptureDate
                })
                .ToListAsync();

            return new PaginatedResponse<CashTransResponse>
            {
                Data = transactions,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching transactions for CDS {CdsNumber}", cdsNumber);
            throw;
        }
    }

    public async Task<ClientBalanceResponse> GetClientBalanceAsync(string cdsNumber)
    {
        try
        {
            var now = DateTime.Now;
            var weekAgo = now.AddDays(-7);

            // Get all transactions for this client
            var allTransactions = await _boContext.CashTrans
                .Where(ct => ct.CDS_Number == cdsNumber)
                .ToListAsync();

            if (!allTransactions.Any())
            {
                return new ClientBalanceResponse
                {
                    CdsNumber = cdsNumber,
                    CurrentBalance = 0,
                    Currency = "BWP",
                    WeeklyChange = 0,
                    WeeklyChangePercentage = 0,
                    TotalTransactions = 0,
                    LastTransactionDate = null
                };
            }

            // Calculate current balance (sum of all transactions)
            var currentBalance = allTransactions.Sum(ct => ct.Amount);

            // Calculate balance from a week ago
            var balanceWeekAgo = allTransactions
                .Where(ct => ct.DateCreated < weekAgo)
                .Sum(ct => ct.Amount);

            var weeklyChange = currentBalance - balanceWeekAgo;
            var weeklyChangePercentage = balanceWeekAgo != 0 
                ? (weeklyChange / balanceWeekAgo) * 100 
                : 0;

            return new ClientBalanceResponse
            {
                CdsNumber = cdsNumber,
                CurrentBalance = currentBalance,
                Currency = "BWP",
                WeeklyChange = weeklyChange,
                WeeklyChangePercentage = Math.Round(weeklyChangePercentage, 2),
                TotalTransactions = allTransactions.Count,
                LastTransactionDate = allTransactions.Max(ct => ct.DateCreated)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating balance for CDS {CdsNumber}", cdsNumber);
            throw;
        }
    }

    public async Task<PaginatedResponse<PaymentTransactionResponse>> GetPaymentTransactionsAsync(
        int page = 1,
        int pageSize = 20,
        string? cdsNumber = null,
        string? provider = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var query = _paymentContext.PaymentTransactions.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(cdsNumber))
                query = query.Where(pt => pt.CdsNumber == cdsNumber);

            if (!string.IsNullOrEmpty(provider))
                query = query.Where(pt => pt.Provider.ToString() == provider);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(pt => pt.Status.ToString() == status);

            if (startDate.HasValue)
                query = query.Where(pt => pt.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(pt => pt.CreatedAt <= endDate.Value);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var transactions = await query
                .OrderByDescending(pt => pt.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(pt => new PaymentTransactionResponse
                {
                    Id = pt.Id,
                    Provider = pt.Provider.ToString(),
                    Type = pt.Type.ToString(),
                    Status = pt.Status.ToString(),
                    CdsNumber = pt.CdsNumber,
                    OriginalTransactionReference = pt.OriginalTransactionReference,
                    ProviderTransactionReference = pt.ProviderTransactionReference,
                    Amount = pt.Amount,
                    Currency = pt.Currency,
                    DebitPartyMsisdn = pt.DebitPartyMsisdn,
                    CreditPartyMsisdn = pt.CreditPartyMsisdn,
                    Description = pt.Description,
                    PostedToCashTrans = pt.PostedToCashTrans,
                    CreatedAt = pt.CreatedAt,
                    UpdatedAt = pt.UpdatedAt
                })
                .ToListAsync();

            return new PaginatedResponse<PaymentTransactionResponse>
            {
                Data = transactions,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching payment transactions");
            throw;
        }
    }
}
