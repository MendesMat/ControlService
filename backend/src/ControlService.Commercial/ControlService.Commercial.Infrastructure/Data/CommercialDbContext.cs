using ControlService.SharedKernel.SeedWork;
using ControlService.Commercial.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ControlService.Commercial.Infrastructure.Data;

public class CommercialDbContext : DbContext, IUnitOfWork
{
    public CommercialDbContext(DbContextOptions<CommercialDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

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
