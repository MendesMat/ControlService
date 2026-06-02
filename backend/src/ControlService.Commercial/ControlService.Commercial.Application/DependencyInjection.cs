using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using ControlService.SharedKernel.Behaviors;
using ControlService.Commercial.Domain.Customers.Services;

namespace ControlService.Commercial.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCommercialApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<CustomerDocumentUniquenessService>();

        return services;
    }
}
