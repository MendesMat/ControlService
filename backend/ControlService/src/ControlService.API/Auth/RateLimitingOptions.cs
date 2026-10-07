namespace ControlService.API.Auth;

/// <summary>Requests per client address in a 60-second window (AUTH-22). Sign-in and refresh
/// each have their own limit; the global limit and the e-mail routes come with their own issues.</summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public RateLimitPolicyOptions SignIn { get; init; } = new();

    public RateLimitPolicyOptions Refresh { get; init; } = new();
}

public sealed class RateLimitPolicyOptions
{
    public int PermitLimit { get; init; }
}
