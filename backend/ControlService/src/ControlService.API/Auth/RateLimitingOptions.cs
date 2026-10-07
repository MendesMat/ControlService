namespace ControlService.API.Auth;

/// <summary>Requests per client address in a 60-second window (AUTH-22). Sign-in and refresh
/// each have their own limit; no other route is limited.</summary>
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
