using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ControlService.Commercial.Domain.Customers;
using ControlService.Commercial.Infrastructure.Data;
using ControlService.Commercial.Infrastructure.Data.Repositories;

namespace ControlService.Commercial.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCommercialInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase");

        services.AddDbContext<CommercialDbContext>(options =>
        {
            if (useInMemory)
            {
                options.UseInMemoryDatabase("ControlServiceCommercialDb");
            }
            else
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory_Commercial", "public")
                );
            }
        });

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        return services;
    }
}
