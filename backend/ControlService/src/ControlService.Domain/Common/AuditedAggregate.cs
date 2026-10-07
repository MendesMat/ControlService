namespace ControlService.Domain.Common;

/// <summary>Audit and concurrency fields shared by every aggregate.
/// Filled by infrastructure (the interceptor, the database), never by domain logic.</summary>
public abstract class AuditedAggregate
{
    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public Guid UpdatedBy { get; private set; }

    public uint Version { get; private set; }
}
