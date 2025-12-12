using Microsoft.EntityFrameworkCore;
using bse_payments.Models.Entities;

namespace bse_payments.Data;

public class BoDbContext : DbContext
{
    public BoDbContext(DbContextOptions<BoDbContext> options) : base(options) { }

    public DbSet<CashTrans> CashTrans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CashTrans>(entity =>
        {
            entity.ToTable("CashTrans", tb => tb.HasTrigger("YourTriggerName"));
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).ValueGeneratedOnAdd();
            entity.Property(e => e.Amount).HasColumnType("money");
            
            // Tell EF Core not to use OUTPUT clause because table has triggers
            entity.ToTable(tb => tb.UseSqlOutputClause(false));
        });
    }
}
