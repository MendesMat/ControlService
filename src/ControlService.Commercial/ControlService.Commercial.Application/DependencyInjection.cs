using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using ControlService.SharedKernel.Behaviors;

namespace ControlService.Commercial.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCommercialApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            // Registrar o ValidationPipelineBehavior do SharedKernel no pipeline do MediatR
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
