using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ControlService.Reporting.Infrastructure.Data;

namespace ControlService.Reporting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");

        services.AddDbContext<ReportingDbContext>(options =>
        {
            if (useInMemory)
            {
                options.UseInMemoryDatabase("ControlServiceReportingDb");
            }
            else
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory_Reporting", "public")
                );
            }
        });

        return services;
    }
}
