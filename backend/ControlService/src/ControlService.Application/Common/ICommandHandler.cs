using ControlService.Domain.Common;

namespace ControlService.Application.Common;

/// <summary>Handles one command and returns its result.</summary>
public interface ICommandHandler<in TCommand, TResponse>
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
