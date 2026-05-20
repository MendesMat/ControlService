using ControlService.SharedKernel.SeedWork;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ControlService.Reporting.Infrastructure.Data;

public class ReportingDbContext : DbContext, IUnitOfWork
{
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options) : base(options)
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
