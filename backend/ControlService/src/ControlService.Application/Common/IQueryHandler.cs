using ControlService.Domain.Common;

namespace ControlService.Application.Common;

/// <summary>Handles one query and returns its result (ADR-0007, ADR-0009). Queries may skip the
/// domain and project straight to response models.</summary>
public interface IQueryHandler<in TQuery, TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
