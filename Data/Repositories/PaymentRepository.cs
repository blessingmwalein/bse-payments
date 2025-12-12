using Microsoft.EntityFrameworkCore;
using bse_payments.Models.Entities;
using bse_payments.Models.Enums;

namespace bse_payments.Data.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentProviderConfig?> GetProviderConfigAsync(PaymentProvider provider)
    {
        return await _context.PaymentProviderConfigs
            .FirstOrDefaultAsync(p => p.Provider == provider && p.IsActive);
    }

    public async Task<PaymentTransaction> CreateTransactionAsync(PaymentTransaction transaction)
    {
        transaction.CreatedAt = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;
        _context.PaymentTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<PaymentTransaction?> GetTransactionByReferenceAsync(string reference, bool isOriginalReference = false)
    {
        return isOriginalReference
            ? await _context.PaymentTransactions.FirstOrDefaultAsync(t => t.OriginalTransactionReference == reference)
            : await _context.PaymentTransactions.FirstOrDefaultAsync(t => t.ProviderTransactionReference == reference);
    }

    public async Task UpdateTransactionAsync(PaymentTransaction transaction)
    {
        transaction.UpdatedAt = DateTime.UtcNow;
        _context.PaymentTransactions.Update(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<ProviderToken?> GetValidTokenAsync(PaymentProvider provider)
    {
        return await _context.ProviderTokens
            .Where(t => t.Provider == provider && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task SaveTokenAsync(ProviderToken token)
    {
        token.CreatedAt = DateTime.UtcNow;
        _context.ProviderTokens.Add(token);
        await _context.SaveChangesAsync();
    }
}
