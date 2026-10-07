using ControlService.Domain.Common;

namespace ControlService.Application.Common;

/// <summary>Handles one query and returns its result. Queries may skip the
/// domain and project straight to response models.</summary>
public interface IQueryHandler<in TQuery, TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
