using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ControlService.Management.Infrastructure.Data;

namespace ControlService.Management.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddManagementInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");

        services.AddDbContext<ManagementDbContext>(options =>
        {
            if (useInMemory)
            {
                options.UseInMemoryDatabase("ControlServiceManagementDb");
            }
            else
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory_Management", "public")
                );
            }
        });

        return services;
    }
}
