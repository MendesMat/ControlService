using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ControlService.Infrastructure.Persistence;

/// <summary>Lets `dotnet ef migrations add` build the model without Program.cs or Aspire (T8).
/// The connection string is never used to connect: migrations add only inspects the model.</summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=controlservice;Username=controlservice;Password=controlservice")
            .UseSnakeCaseNamingConvention();

        return new AppDbContext(optionsBuilder.Options);
    }
}
