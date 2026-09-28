namespace ControlService.API.Common;

/// <summary>The `type` and `title` RFC 9110 pair for each status code this API returns (ADR-0009).</summary>
internal static class ProblemTypes
{
    private static readonly Dictionary<int, (string Type, string Title)> ByStatus = new()
    {
        [StatusCodes.Status400BadRequest] = ("https://tools.ietf.org/html/rfc9110#section-15.5.1", "Bad Request"),
        [StatusCodes.Status404NotFound] = ("https://tools.ietf.org/html/rfc9110#section-15.5.5", "Not Found"),
        [StatusCodes.Status409Conflict] = ("https://tools.ietf.org/html/rfc9110#section-15.5.10", "Conflict"),
        [StatusCodes.Status500InternalServerError] =
            ("https://tools.ietf.org/html/rfc9110#section-15.6.1", "Internal Server Error"),
    };

    public static (string Type, string Title) For(int status) => ByStatus[status];
}
