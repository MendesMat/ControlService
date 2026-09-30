using ControlService.Application.Common;
using ControlService.Domain.Common;

namespace ControlService.Application.Auth.ChangePassword;

public sealed class ChangePasswordHandler(
    ISessionStore sessions,
    IAccessTokenIssuer accessTokens) : ICommandHandler<ChangePasswordCommand, SessionGrant>
{
    public async Task<Result<SessionGrant>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var session = await sessions.StartAsync(command.UserId, cancellationToken);
        var accessToken = accessTokens.Issue(command.UserId, session.SessionId, mustChangePassword: false);

        return Result<SessionGrant>.Success(SessionGrant.From(accessToken, session, mustChangePassword: false));
    }
}
