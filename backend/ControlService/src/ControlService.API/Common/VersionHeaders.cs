using System.Globalization;

namespace ControlService.API.Common;

/// <summary>The `ETag` a record is read with and the `If-Match` it is changed with (API-06). Both carry the
/// record's `version` between quotes.</summary>
public static class VersionHeaders
{
    public static void WriteETag(this HttpResponse response, string version) =>
        response.Headers.ETag = $"\"{version}\"";

    /// <summary>The version the person read, with or without quotes. A missing or unreadable header gives 0, which
    /// no row can have, so the save is refused as a conflict (CNV-13) instead of overwriting blindly.</summary>
    public static uint ReadIfMatch(this HttpRequest request)
    {
        var header = request.Headers.IfMatch.ToString().Trim().Trim('"');
        return uint.TryParse(header, NumberStyles.None, CultureInfo.InvariantCulture, out var version) ? version : 0;
    }
}
