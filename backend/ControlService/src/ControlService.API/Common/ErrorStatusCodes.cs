namespace ControlService.API.Common;

/// <summary>The HTTP status for each business error code (ADR-0009). A code missing here is a
/// programming mistake, not a business outcome, and falls back to 500 in <see cref="ErrorResults"/>.</summary>
internal static class ErrorStatusCodes
{
    private static readonly Dictionary<string, int> StatusesByCode = new()
    {
        ["validation_failed"] = StatusCodes.Status400BadRequest,
        ["not_found"] = StatusCodes.Status404NotFound,
        ["concurrency_conflict"] = StatusCodes.Status409Conflict,
    };

    public static int? For(string code) => StatusesByCode.TryGetValue(code, out var status) ? status : null;
}
