using ControlService.Application.Auth;
using ControlService.Application.Auth.SignIn;
using ControlService.Application.Common;

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

        // Handlers are registered by hand (ADR-0008); the ones with input to validate are wrapped in the decorator.
        services.AddScoped<ICommandHandler<SignInCommand, SessionGrant>, SignInHandler>();

        return services;
    }

    // Never echoes the key: the message names it, and that is all.
    private static bool IsValidSigningKey(JwtOptions options)
    {
        Span<byte> buffer = new byte[64];
        return Convert.TryFromBase64String(options.SigningKey, buffer, out var written) && written >= 32;
    }
}
