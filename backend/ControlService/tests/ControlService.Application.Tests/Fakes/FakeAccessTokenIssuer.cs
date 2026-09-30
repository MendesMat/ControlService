using ControlService.Application.Auth;

namespace ControlService.Application.Tests.Fakes;

internal sealed class FakeAccessTokenIssuer : IAccessTokenIssuer
{
    public AccessToken Issue(Guid userId, Guid sessionId, bool mustChangePassword) =>
        new(TokenFor(userId, sessionId, mustChangePassword), ExpiresInSeconds: 900);

    public static string TokenFor(Guid userId, Guid sessionId, bool mustChangePassword) =>
        $"access-token:{userId}:{sessionId}:{(mustChangePassword ? "must-change" : "regular")}";
}
