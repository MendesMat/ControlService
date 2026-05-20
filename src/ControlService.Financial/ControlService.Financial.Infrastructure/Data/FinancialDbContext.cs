using ControlService.SharedKernel.SeedWork;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ControlService.Financial.Infrastructure.Data;

public class FinancialDbContext : DbContext, IUnitOfWork
{
    public FinancialDbContext(DbContextOptions<FinancialDbContext> options) : base(options)
    {
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await base.SaveChangesAsync(cancellationToken);
        return true;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
