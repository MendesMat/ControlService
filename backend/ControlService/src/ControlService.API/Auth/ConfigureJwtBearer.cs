using ControlService.API.Common;
using ControlService.Application.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ControlService.API.Auth;

/// <summary>How access tokens are checked. Claim names stay as issued (`sub`, `sid`), because
/// ICurrentUser reads `sub`; the lifetime is judged by the application's clock, with no skew, so 15
/// minutes means 15 minutes; and every failed authentication looks like any other error (API-12).</summary>
public sealed class ConfigureJwtBearer(IOptions<JwtOptions> jwtOptions, TimeProvider timeProvider)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(JwtBearerOptions options) => Configure(Options.DefaultName, options);

    public void Configure(string? name, JwtBearerOptions options)
    {
        var jwt = jwtOptions.Value;

        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwt.SigningKeyBytes()),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.Zero,

            // IdentityModel has no clock seam, so the lifetime check is ours: it reads TimeProvider (which
            // tests control), and `exp` is exclusive, as RFC 7519 says.
            LifetimeValidator = (notBefore, expires, _, _) =>
            {
                var now = timeProvider.GetUtcNow().UtcDateTime;
                return expires is not null && now < expires && (notBefore is null || notBefore <= now);
            },
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                await AuthErrors.SessionExpired.ToProblem().ExecuteAsync(context.HttpContext);
            },
        };
    }
}
