using ControlService.Application.Common;
using ControlService.Domain.Common;

namespace ControlService.Application.Tests.Fakes;

/// <summary>Counts the saves: the in-memory repositories hold the aggregates themselves, so what a
/// test checks is that the handler asked to persist them.</summary>
internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Error? FailWith { get; set; }

    public Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.FromResult(FailWith is null ? Result.Success() : Result.Failure(FailWith));
    }
}
