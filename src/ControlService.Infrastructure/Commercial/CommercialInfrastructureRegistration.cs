using ControlService.Domain.Commercial.Customers;
using ControlService.Infrastructure.Data.Repositories.Commercial;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Infrastructure.Commercial;

/// <summary>
/// Registra os serviços de infraestrutura específicos do módulo comercial (Commercial).
/// </summary>
public static class CommercialInfrastructureRegistration
{
    public static IServiceCollection AddCommercialInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        return services;
    }
}
