using ControlService.Domain.Common;

namespace ControlService.Application.Common;

/// <summary>Persists the changes tracked by the current unit of work, translating a
/// concurrency conflict into the concurrency_conflict error.</summary>
public interface IUnitOfWork
{
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken);
}
