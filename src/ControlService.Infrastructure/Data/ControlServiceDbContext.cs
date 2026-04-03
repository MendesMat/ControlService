using ControlService.Domain.SeedWork;
using ControlService.Domain.Commercial.Customers;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ControlService.Infrastructure.Data;

public class ControlServiceDbContext : DbContext, IUnitOfWork
{
    public ControlServiceDbContext(DbContextOptions<ControlServiceDbContext> options) : base(options)
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
