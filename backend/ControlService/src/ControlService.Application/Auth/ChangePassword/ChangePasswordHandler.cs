using ControlService.Application.Common;
using ControlService.Application.Users;
using ControlService.Domain.Common;

namespace ControlService.Application.Auth.ChangePassword;

public sealed class ChangePasswordHandler(
    IUserRepository users,
    ICredentialStore credentials,
    ISessionStore sessions,
    IAccessTokenIssuer accessTokens) : ICommandHandler<ChangePasswordCommand, SessionGrant>
{
    public async Task<Result<SessionGrant>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return Result<SessionGrant>.Failure(AuthErrors.SessionExpired);
        }

        if (!await credentials.MustChangePasswordAsync(command.UserId, cancellationToken))
        {
            return Result<SessionGrant>.Failure(AuthErrors.PasswordAlreadyCreated);
        }

        if (await credentials.IsCurrentPasswordAsync(command.UserId, command.Password, cancellationToken))
        {
            return Result<SessionGrant>.Failure(AuthErrors.NewPasswordEqualsInitial);
        }

        await credentials.ReplacePasswordAsync(command.UserId, command.Password, cancellationToken);

        // A new password ends every session, including the one that asked for the change.
        await sessions.EndAllAsync(command.UserId, cancellationToken);
        var session = await sessions.StartAsync(command.UserId, cancellationToken);
        var accessToken = accessTokens.Issue(command.UserId, session.SessionId, mustChangePassword: false);

        return Result<SessionGrant>.Success(SessionGrant.From(accessToken, session, mustChangePassword: false));
    }
}
