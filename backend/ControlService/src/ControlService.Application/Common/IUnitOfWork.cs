using ControlService.Domain.Common;

namespace ControlService.Application.Common;

/// <summary>Persists the changes tracked by the current unit of work (ADR-0009), translating a
/// concurrency conflict into the concurrency_conflict error (ADR-0014).</summary>
public interface IUnitOfWork
{
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken);
}
