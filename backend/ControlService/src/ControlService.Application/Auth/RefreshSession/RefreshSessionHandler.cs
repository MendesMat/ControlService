using ControlService.Application.Common;
using ControlService.Application.Users;
using ControlService.Domain.Common;
using ControlService.Domain.Users;

namespace ControlService.Application.Auth.RefreshSession;

public sealed class RefreshSessionHandler(
    IUserRepository users,
    ICredentialStore credentials,
    ISessionStore sessions,
    IAccessTokenIssuer accessTokens) : ICommandHandler<RefreshSessionCommand, SessionGrant>
{
    public async Task<Result<SessionGrant>> Handle(RefreshSessionCommand command, CancellationToken cancellationToken)
    {
        var rotated = await sessions.RotateAsync(command.RefreshToken, cancellationToken);
        if (rotated is null)
        {
            return Result<SessionGrant>.Failure(AuthErrors.SessionExpired);
        }

        // Refresh is anonymous (cookie only), so the per-request account check does not cover it (D10).
        var user = await users.GetByIdAsync(rotated.UserId, cancellationToken);
        if (user is null)
        {
            await sessions.EndAsync(rotated.Tokens.SessionId, cancellationToken);
            return Result<SessionGrant>.Failure(AuthErrors.SessionExpired);
        }

        if (user.Status != UserStatus.Active)
        {
            await sessions.EndAsync(rotated.Tokens.SessionId, cancellationToken);
            return Result<SessionGrant>.Failure(AuthErrors.AccountInactive);
        }

        var mustChangePassword = await credentials.MustChangePasswordAsync(user.Id, cancellationToken);
        var accessToken = accessTokens.Issue(user.Id, rotated.Tokens.SessionId, mustChangePassword);

        return Result<SessionGrant>.Success(SessionGrant.From(accessToken, rotated.Tokens, mustChangePassword));
    }
}
