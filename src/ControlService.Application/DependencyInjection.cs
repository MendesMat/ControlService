using Microsoft.Extensions.DependencyInjection;
using MediatR;
using FluentValidation;
using ControlService.Application.Behaviors;

namespace ControlService.Application;

/// <summary>
/// Provedor central de registro para serviços da camada de Aplicação.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        RegisterMediatR(services, assembly);
        RegisterValidators(services, assembly);

        return services;
    }

    private static void RegisterMediatR(IServiceCollection services, System.Reflection.Assembly assembly)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
        });
    }

    private static void RegisterValidators(IServiceCollection services, System.Reflection.Assembly assembly)
    {
        services.AddValidatorsFromAssembly(assembly);
    }
}
