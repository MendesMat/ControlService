using ControlService.Application.Common;
using ControlService.Application.Users;
using ControlService.Domain.Common;
using ControlService.Domain.Users;

namespace ControlService.Application.Auth.SignIn;

public sealed class SignInHandler(
    IUserRepository users,
    ICredentialStore credentials,
    ISessionStore sessions,
    IAccessTokenIssuer accessTokens) : ICommandHandler<SignInCommand, SessionGrant>
{
    public async Task<Result<SessionGrant>> Handle(SignInCommand command, CancellationToken cancellationToken)
    {
        // A login that could never exist is answered like one that does not (AUTH-08).
        var login = Login.Create(command.Login);
        if (login.IsFailure)
        {
            return Result<SessionGrant>.Failure(AuthErrors.InvalidCredentials);
        }

        var user = await users.GetByLoginAsync(login.Value, cancellationToken);
        if (user is null)
        {
            return Result<SessionGrant>.Failure(AuthErrors.InvalidCredentials);
        }

        var check = await credentials.CheckPasswordAsync(user.Id, command.Password, cancellationToken);
        if (check.Outcome != CredentialCheckOutcome.Succeeded)
        {
            return Result<SessionGrant>.Failure(AuthErrors.InvalidCredentials);
        }

        var session = await sessions.StartAsync(user.Id, cancellationToken);
        var accessToken = accessTokens.Issue(user.Id, session.SessionId, mustChangePassword: false);

        return Result<SessionGrant>.Success(new SessionGrant(
            accessToken.Value, accessToken.ExpiresInSeconds, MustChangePassword: false, session.RefreshToken, session.ExpiresAt));
    }
}
