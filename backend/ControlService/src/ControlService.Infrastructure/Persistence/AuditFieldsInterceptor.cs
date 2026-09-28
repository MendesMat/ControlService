using ControlService.Application.Common;
using ControlService.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ControlService.Infrastructure.Persistence;

/// <summary>Fills the audit fields on every save (ADR-0015). Never runs for a request without a
/// signed-in user (CNV-20): the client stays consistent instead of writing an orphaned record.</summary>
public sealed class AuditFieldsInterceptor(ICurrentUser currentUser, TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entries = context.ChangeTracker.Entries<AuditedAggregate>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .ToList();
        if (entries.Count == 0)
        {
            return;
        }

        var userId = currentUser.UserId ?? throw new InvalidOperationException(
            "A record cannot be saved without a signed-in user.");
        var now = timeProvider.GetUtcNow();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(AuditedAggregate.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(AuditedAggregate.CreatedBy)).CurrentValue = userId;
            }

            entry.Property(nameof(AuditedAggregate.UpdatedAt)).CurrentValue = now;
            entry.Property(nameof(AuditedAggregate.UpdatedBy)).CurrentValue = userId;
        }
    }
}
