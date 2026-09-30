using ControlService.Application.Auth;
using ControlService.Application.Auth.GetMe;
using ControlService.Application.Auth.RefreshSession;
using ControlService.Application.Auth.SignIn;
using ControlService.Application.Auth.SignOut;
using ControlService.Application.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace ControlService.API.Auth;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(IsValidSigningKey, "Auth:SigningKey must be the base64 of at least 32 bytes.")
            .ValidateOnStart();

        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearer>();
        services.AddAuthorization();

        // Handlers are registered by hand (ADR-0008); the ones with input to validate are wrapped in the decorator.
        services.AddScoped<ICommandHandler<SignInCommand, SessionGrant>, SignInHandler>();
        services.AddScoped<ICommandHandler<RefreshSessionCommand, SessionGrant>, RefreshSessionHandler>();
        services.AddScoped<ICommandHandler<SignOutCommand, Unit>, SignOutHandler>();
        services.AddScoped<IQueryHandler<GetMeQuery, MeResponse>, GetMeHandler>();

        return services;
    }

    // Never echoes the key: the message names it, and that is all.
    private static bool IsValidSigningKey(JwtOptions options)
    {
        Span<byte> buffer = new byte[64];
        return Convert.TryFromBase64String(options.SigningKey, buffer, out var written) && written >= 32;
    }
}
