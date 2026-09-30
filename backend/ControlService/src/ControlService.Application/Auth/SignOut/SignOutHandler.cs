using ControlService.Application.Common;
using ControlService.Domain.Common;

namespace ControlService.Application.Auth.SignOut;

public sealed class SignOutHandler(ISessionStore sessions) : ICommandHandler<SignOutCommand, Unit>
{
    public async Task<Result<Unit>> Handle(SignOutCommand command, CancellationToken cancellationToken)
    {
        await sessions.EndAsync(command.SessionId, cancellationToken);
        return Result<Unit>.Success(default);
    }
}
