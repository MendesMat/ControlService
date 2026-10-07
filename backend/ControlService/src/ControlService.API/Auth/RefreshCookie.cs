namespace ControlService.API.Auth;

/// <summary>The refresh token cookie (T6). Its `Path` is the refresh route only, so the
/// browser sends it nowhere else; expiring it needs the same name and `Path`.</summary>
internal static class RefreshCookie
{
    public const string Name = "__Secure-cs-refresh";
    public const string Path = "/api/v1/auth/refresh";

    public static void Append(HttpResponse response, string refreshToken, DateTimeOffset expiresAt) =>
        response.Cookies.Append(Name, refreshToken, Options(expiresAt));

    public static void Expire(HttpResponse response) => response.Cookies.Delete(Name, Options(expiresAt: null));

    private static CookieOptions Options(DateTimeOffset? expiresAt) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = Path,
        Expires = expiresAt,
        IsEssential = true,
    };
}
