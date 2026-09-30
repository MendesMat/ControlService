namespace ControlService.Application.Tests.Fakes;

/// <summary>A controllable clock so no test depends on the real time (testing guide).</summary>
internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    private DateTimeOffset _utcNow = utcNow;

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void Advance(TimeSpan span) => _utcNow += span;
}
