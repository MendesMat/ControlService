using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ControlService.Operational.Infrastructure.Data;

namespace ControlService.Operational.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOperationalInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");

        services.AddDbContext<OperationalDbContext>(options =>
        {
            if (useInMemory)
            {
                options.UseInMemoryDatabase("ControlServiceOperationalDb");
            }
            else
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory_Operational", "public")
                );
            }
        });

        return services;
    }
}
