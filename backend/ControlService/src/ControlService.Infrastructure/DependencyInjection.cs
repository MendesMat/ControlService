using ControlService.Application.Common;
using ControlService.Domain.Users;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ControlService.Infrastructure;

public static class DependencyInjection
{
    // ICurrentUser and TimeProvider are registered by the caller (API/Program.cs, T12):
    // AuditFieldsInterceptor depends on both, and the Admin's own creation depends on ICurrentUser
    // answering the Admin's id outside a request (D3).
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<AuditFieldsInterceptor>();

        builder.Services.AddOptions<AdminOptions>()
            .Bind(builder.Configuration.GetSection(AdminOptions.SectionName))
            .Validate(options => EmailAddress.Create(options.Email).IsSuccess, "Admin:Email must be a valid e-mail address.")
            .ValidateOnStart();

        builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var adminEmail = serviceProvider.GetRequiredService<IOptions<AdminOptions>>().Value.Email;
            options
                .UseNpgsql(builder.Configuration.GetConnectionString("controlservice"))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(serviceProvider.GetRequiredService<AuditFieldsInterceptor>())
                .UseSeeding((context, _) => SystemRecordsSeeder.Seed(context, adminEmail))
                .UseAsyncSeeding((context, _, cancellationToken) =>
                    SystemRecordsSeeder.SeedAsync(context, adminEmail, cancellationToken));
        });

        // AddNpgsqlDbContext pools contexts, which does not fit a scoped interceptor (T7): the
        // context above is registered by hand and only enriched with Aspire's retries, health
        // checks, logging and telemetry.
        builder.EnrichNpgsqlDbContext<AppDbContext>();

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        return builder;
    }
}
