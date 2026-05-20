using ControlService.SharedKernel.SeedWork;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ControlService.Management.Infrastructure.Data;

public class ManagementDbContext : DbContext, IUnitOfWork
{
    public ManagementDbContext(DbContextOptions<ManagementDbContext> options) : base(options)
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
