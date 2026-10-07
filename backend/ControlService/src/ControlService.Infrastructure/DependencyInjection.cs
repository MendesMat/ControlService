using ControlService.Application.Auth;
using ControlService.Application.Common;
using ControlService.Application.PermissionProfiles;
using ControlService.Application.Users;
using ControlService.Domain.Users;
using ControlService.Infrastructure.Auth;
using ControlService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ControlService.Infrastructure;

public static class DependencyInjection
{
    // ICurrentUser and TimeProvider are registered by the caller (API/Program.cs):
    // AuditFieldsInterceptor depends on both, and the Admin's own creation depends on ICurrentUser
    // answering the Admin's id outside a request.
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<AuditFieldsInterceptor>();

        builder.Services.AddOptions<AuthOptions>()
            .Bind(builder.Configuration.GetSection(AuthOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Message texts state these two values (AUTH-28), and Application cannot read options.
        builder.Services.AddSingleton(serviceProvider =>
        {
            var auth = serviceProvider.GetRequiredService<IOptions<AuthOptions>>().Value;
            return new AuthSettings(auth.PasswordMinLength, auth.LockoutMinutes);
        });

        builder.Services.AddOptions<AdminOptions>()
            .Bind(builder.Configuration.GetSection(AdminOptions.SectionName))
            .Validate(options => EmailAddress.Create(options.Email).IsSuccess, "Admin:Email must be a valid e-mail address.")
            .Validate<IOptions<AuthOptions>>(
                // Binding leaves the value null when the user secret was never set.
                (options, auth) => options.InitialPassword?.Length >= auth.Value.PasswordMinLength,
                "Admin:InitialPassword must be set, with at least Auth:PasswordMinLength characters.")
            .ValidateOnStart();

        // No SignInManager: it pulls in cookie authentication. The credential store uses UserManager
        // for hashing and lockout only.
        builder.Services.AddIdentityCore<UserCredential>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddUserManager<CredentialUserManager>();
        builder.Services.AddOptions<IdentityOptions>().Configure<IOptions<AuthOptions>>((identity, auth) =>
        {
            // Identity must never refuse a password the validator accepted (AUTH-16): length only.
            identity.Password.RequireDigit = false;
            identity.Password.RequireLowercase = false;
            identity.Password.RequireUppercase = false;
            identity.Password.RequireNonAlphanumeric = false;
            identity.Password.RequiredUniqueChars = 1;
            identity.Password.RequiredLength = auth.Value.PasswordMinLength;
            identity.Lockout.AllowedForNewUsers = true;
            identity.Lockout.MaxFailedAccessAttempts = auth.Value.LockoutMaxFailedAttempts;
            identity.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(auth.Value.LockoutMinutes);
        });

        builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var admin = serviceProvider.GetRequiredService<IOptions<AdminOptions>>().Value;
            options
                .UseNpgsql(builder.Configuration.GetConnectionString("controlservice"))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(serviceProvider.GetRequiredService<AuditFieldsInterceptor>())
                .UseSeeding((context, _) => SystemRecordsSeeder.Seed(context, admin.Email, admin.InitialPassword))
                .UseAsyncSeeding((context, _, cancellationToken) =>
                    SystemRecordsSeeder.SeedAsync(context, admin.Email, admin.InitialPassword, cancellationToken));
        });

        // AddNpgsqlDbContext pools contexts, which does not fit a scoped interceptor: the
        // context above is registered by hand and only enriched with Aspire's retries, health
        // checks, logging and telemetry.
        builder.EnrichNpgsqlDbContext<AppDbContext>();

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IPermissionProfileRepository, PermissionProfileRepository>();
        builder.Services.AddScoped<ICredentialStore, CredentialStore>();
        builder.Services.AddScoped<ISessionStore, SessionStore>();

        return builder;
    }
}
