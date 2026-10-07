using System.Globalization;
using System.Threading.RateLimiting;
using ControlService.API.Common;
using ControlService.Application.Auth;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace ControlService.API.Auth;

/// <summary>Rate limits of the authentication routes: a sliding window of 60 seconds
/// in 6 segments per client address, with no queue. A refused request answers like a lockout does:
/// 429 `locked_out` with `Retry-After` (AUTH-26).</summary>
internal static class AuthRateLimiting
{
    public const string SignInPolicy = "sign-in";
    public const string RefreshPolicy = "refresh";

    private static readonly TimeSpan Window = TimeSpan.FromSeconds(60);

    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .Validate(
                options => options.SignIn.PermitLimit > 0 && options.Refresh.PermitLimit > 0,
                "RateLimiting:SignIn:PermitLimit and RateLimiting:Refresh:PermitLimit must be greater than zero.")
            .ValidateOnStart();

        services.AddRateLimiter();
        services.AddOptions<RateLimiterOptions>().Configure<IOptions<RateLimitingOptions>>((limiter, limits) =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.AddPolicy(SignInPolicy, context => PartitionByAddress(context, limits.Value.SignIn.PermitLimit));
            limiter.AddPolicy(RefreshPolicy, context => PartitionByAddress(context, limits.Value.Refresh.PermitLimit));
            limiter.OnRejected = RejectAsync;
        });

        return services;
    }

    // `unknown` covers a request with no remote address, as in the test server.
    private static RateLimitPartition<string> PartitionByAddress(HttpContext context, int permitLimit) =>
        RateLimitPartition.GetSlidingWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = Window,
                SegmentsPerWindow = 6,
                QueueLimit = 0,
            });

    private static async ValueTask RejectAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        // The limiter reports when the next segment frees up; the whole window is the safe fallback.
        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var wait) ? wait : Window;
        var seconds = (int)Math.Ceiling(retryAfter.TotalSeconds);

        context.HttpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
        await AuthErrors.TooManyRequests(seconds).ToProblem().ExecuteAsync(context.HttpContext);
    }
}
