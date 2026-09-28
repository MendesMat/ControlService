namespace ControlService.API.Common;

/// <summary>The HTTP status of every error code in the ADR-0009 table. A code missing here is a
/// programming mistake, not a business outcome: <see cref="ErrorResults"/> treats it as an
/// unexpected failure (API-12, API-14).</summary>
internal static class ErrorStatusCodes
{
    private static readonly Dictionary<string, int> StatusesByCode = new()
    {
        ["validation_failed"] = StatusCodes.Status400BadRequest,
        ["session_expired"] = StatusCodes.Status401Unauthorized,
        ["invalid_credentials"] = StatusCodes.Status401Unauthorized,
        // During a session. At sign-in the same code is a 403, returned explicitly by that endpoint.
        ["account_inactive"] = StatusCodes.Status401Unauthorized,
        ["password_change_required"] = StatusCodes.Status401Unauthorized,
        ["forbidden"] = StatusCodes.Status403Forbidden,
        ["not_found"] = StatusCodes.Status404NotFound,
        ["concurrency_conflict"] = StatusCodes.Status409Conflict,
        ["profile_in_use"] = StatusCodes.Status409Conflict,
        ["system_record"] = StatusCodes.Status409Conflict,
        ["self_deactivation"] = StatusCodes.Status409Conflict,
        ["not_pending"] = StatusCodes.Status409Conflict,
        ["not_inactive"] = StatusCodes.Status409Conflict,
        ["email_missing"] = StatusCodes.Status409Conflict,
        ["link_invalid"] = StatusCodes.Status410Gone,
        ["locked_out"] = StatusCodes.Status429TooManyRequests,
        ["unexpected_error"] = StatusCodes.Status500InternalServerError,
    };

    public static int? For(string code) => StatusesByCode.TryGetValue(code, out var status) ? status : null;
}
