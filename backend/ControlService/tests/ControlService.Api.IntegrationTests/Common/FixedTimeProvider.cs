namespace ControlService.Api.IntegrationTests.Common;

/// <summary>A controllable clock so tests never depend on the real time (T12, testing guide).</summary>
public sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    private DateTimeOffset _utcNow = utcNow;

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void Set(DateTimeOffset utcNow) => _utcNow = utcNow;

    public void Advance(TimeSpan span) => _utcNow += span;
}
