using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ControlService.Financial.Infrastructure.Data;

namespace ControlService.Financial.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinancialInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");

        services.AddDbContext<FinancialDbContext>(options =>
        {
            if (useInMemory)
            {
                options.UseInMemoryDatabase("ControlServiceFinancialDb");
            }
            else
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory_Financial", "public")
                );
            }
        });

        return services;
    }
}
