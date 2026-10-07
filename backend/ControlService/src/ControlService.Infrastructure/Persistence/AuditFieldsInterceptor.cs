using ControlService.Application.Common;
using ControlService.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ControlService.Infrastructure.Persistence;

/// <summary>Fills the audit fields on every save (CNV-10, CNV-19). A save during a request
/// without a signed-in user throws, so nothing is written without an author.</summary>
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

        var tracked = context.ChangeTracker.Entries().ToList();
        var entries = tracked
            .Where(entry => entry.Entity is AuditedAggregate)
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified
                || (entry.State == EntityState.Unchanged && HasChangedOwnedEntries(entry, tracked)))
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

            SetUpdated(entry.Property(nameof(AuditedAggregate.UpdatedAt)), now);
            SetUpdated(entry.Property(nameof(AuditedAggregate.UpdatedBy)), userId);
        }
    }

    // Assigning a value equal to the stored one leaves the property unmodified, and then an owner
    // whose only change is in its owned rows would get no UPDATE and no xmin check (CNV-13).
    private static void SetUpdated(PropertyEntry property, object value)
    {
        property.CurrentValue = value;
        if (property.EntityEntry.State != EntityState.Added)
        {
            property.IsModified = true;
        }
    }

    // Owned collections (profile levels, profile assignments) live in their own tables, so changing
    // only them leaves the owner Unchanged. Touching the owner's audit fields forces an UPDATE of its
    // row, which bumps xmin and checks it (CNV-10, CNV-12, CNV-13).
    private static bool HasChangedOwnedEntries(EntityEntry owner, IEnumerable<EntityEntry> tracked) =>
        tracked.Any(entry =>
            entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
            && entry.Metadata.FindOwnership() is { } ownership
            && ownership.PrincipalEntityType == owner.Metadata
            && ownership.Properties.Select(property => entry.Property(property.Name).CurrentValue)
                .SequenceEqual(ownership.PrincipalKey.Properties.Select(property => owner.Property(property.Name).CurrentValue)));
}
