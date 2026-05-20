using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using ControlService.SharedKernel.Behaviors;

namespace ControlService.Reporting.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
