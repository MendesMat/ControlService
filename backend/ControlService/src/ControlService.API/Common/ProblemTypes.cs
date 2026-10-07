namespace ControlService.API.Common;

/// <summary>The `type` and `title` pair for each status code this API returns: RFC 9110,
/// and RFC 6585 for 429, which RFC 9110 does not define.</summary>
internal static class ProblemTypes
{
    private static readonly Dictionary<int, (string Type, string Title)> ByStatus = new()
    {
        [StatusCodes.Status400BadRequest] = ("https://tools.ietf.org/html/rfc9110#section-15.5.1", "Bad Request"),
        [StatusCodes.Status401Unauthorized] = ("https://tools.ietf.org/html/rfc9110#section-15.5.2", "Unauthorized"),
        [StatusCodes.Status403Forbidden] = ("https://tools.ietf.org/html/rfc9110#section-15.5.4", "Forbidden"),
        [StatusCodes.Status404NotFound] = ("https://tools.ietf.org/html/rfc9110#section-15.5.5", "Not Found"),
        [StatusCodes.Status409Conflict] = ("https://tools.ietf.org/html/rfc9110#section-15.5.10", "Conflict"),
        [StatusCodes.Status410Gone] = ("https://tools.ietf.org/html/rfc9110#section-15.5.11", "Gone"),
        [StatusCodes.Status429TooManyRequests] = ("https://tools.ietf.org/html/rfc6585#section-4", "Too Many Requests"),
        [StatusCodes.Status500InternalServerError] =
            ("https://tools.ietf.org/html/rfc9110#section-15.6.1", "Internal Server Error"),
    };

    public static (string Type, string Title) For(int status) => ByStatus[status];
}
