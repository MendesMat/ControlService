using ControlService.Application.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ControlService.API.Auth;

/// <summary>Issues the HS256 access token. It carries the user id in `sub` (ICurrentUser reads it),
/// the session in `sid`, and `must_change_password` only while the change is mandatory. No personal data.</summary>
public sealed class JwtAccessTokenIssuer(IOptions<JwtOptions> options, TimeProvider timeProvider) : IAccessTokenIssuer
{
    public const string MustChangePasswordClaim = "must_change_password";
    public const string SessionIdClaim = "sid";

    public AccessToken Issue(Guid userId, Guid sessionId, bool mustChangePassword)
    {
        var jwt = options.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var lifetime = TimeSpan.FromMinutes(jwt.AccessTokenMinutes);

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = userId.ToString(),
            [SessionIdClaim] = sessionId.ToString(),
        };
        if (mustChangePassword)
        {
            claims[MustChangePasswordClaim] = true;
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now + lifetime,
            Claims = claims,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(jwt.SigningKeyBytes()), SecurityAlgorithms.HmacSha256),
        };

        return new AccessToken(new JsonWebTokenHandler().CreateToken(descriptor), (int)lifetime.TotalSeconds);
    }
}
