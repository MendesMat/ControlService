using ControlService.Application.Common;
using ControlService.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ControlService.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException exception)
        {
            return await ConcurrencyConflict(exception, cancellationToken);
        }
    }

    // A conflict on a row deleted meanwhile is out of scope here: GetDatabaseValuesAsync
    // returns null and this throws, propagating as an unexpected failure.
    private async Task<Result> ConcurrencyConflict(DbUpdateConcurrencyException exception, CancellationToken cancellationToken)
    {
        var entry = exception.Entries.Single();
        var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);
        var updatedBy = (Guid)databaseValues![nameof(AuditedAggregate.UpdatedBy)]!;
        var updatedByUser = await dbContext.Users.AsNoTracking().SingleAsync(user => user.Id == updatedBy, cancellationToken);

        return Result.Failure(new Error(
            "concurrency_conflict",
            $"Este cadastro foi alterado por {updatedByUser.DisplayName} enquanto você editava. Recarregue para ver a versão atual.",
            Details: new Dictionary<string, object?> { ["updatedByName"] = updatedByUser.DisplayName }));
    }
}
