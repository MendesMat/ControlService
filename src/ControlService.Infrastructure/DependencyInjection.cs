using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ControlService.Infrastructure.Persistence;
using ControlService.Infrastructure.Commercial;

namespace ControlService.Infrastructure;

/// <summary>
/// Orquestra o registro de todos os serviços da camada de Infraestrutura.
/// Segue os princípios de DDD e Clean Architecture ao delegar registros para submódulos.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ── Registro de Persistência ───────────────────────────────────────────────
        services.AddPersistence(configuration);

        // ── Registro de Módulos (Bounded Contexts) ──────────────────────────────────
        services.AddCommercialInfrastructure();

        return services;
    }
}
