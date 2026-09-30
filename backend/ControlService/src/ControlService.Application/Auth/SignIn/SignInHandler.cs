using ControlService.Application.Common;
using ControlService.Application.Users;
using ControlService.Domain.Common;
using ControlService.Domain.Users;

namespace ControlService.Application.Auth.SignIn;

public sealed class SignInHandler(
    IUserRepository users,
    ICredentialStore credentials,
    ISessionStore sessions,
    IAccessTokenIssuer accessTokens,
    AuthSettings settings,
    TimeProvider timeProvider) : ICommandHandler<SignInCommand, SessionGrant>
{
    public async Task<Result<SessionGrant>> Handle(SignInCommand command, CancellationToken cancellationToken)
    {
        // A login that could never exist is answered like one that does not (AUTH-08).
        var login = Login.Create(command.Login);
        var user = login.IsSuccess ? await users.GetByLoginAsync(login.Value, cancellationToken) : null;
        if (user is null)
        {
            return Result<SessionGrant>.Failure(AuthErrors.InvalidCredentials);
        }

        var check = await credentials.CheckPasswordAsync(user.Id, command.Password, cancellationToken);
        if (check.Outcome == CredentialCheckOutcome.LockedOut)
        {
            var retryAfter = Math.Ceiling((check.LockoutEnd!.Value - timeProvider.GetUtcNow()).TotalSeconds);
            return Result<SessionGrant>.Failure(AuthErrors.LockedOut(settings.LockoutMinutes, (int)retryAfter));
        }

        if (check.Outcome != CredentialCheckOutcome.Succeeded)
        {
            return Result<SessionGrant>.Failure(AuthErrors.InvalidCredentials);
        }

        // Only a correct password reveals that the account is deactivated (AUTH-09).
        if (user.Status == UserStatus.Inactive)
        {
            return Result<SessionGrant>.Failure(AuthErrors.AccountInactive);
        }

        var session = await sessions.StartAsync(user.Id, cancellationToken);
        var mustChangePassword = await credentials.MustChangePasswordAsync(user.Id, cancellationToken);
        var accessToken = accessTokens.Issue(user.Id, session.SessionId, mustChangePassword);

        return Result<SessionGrant>.Success(new SessionGrant(
            accessToken.Value, accessToken.ExpiresInSeconds, mustChangePassword, session.RefreshToken, session.ExpiresAt));
    }
}
