namespace ControlService.API.Common;

/// <summary>The `ETag` a record is read with and the `If-Match` it is changed with (API-06). Both carry the
/// record's `version` between quotes.</summary>
public static class VersionHeaders
{
    public static void WriteETag(this HttpResponse response, string version) =>
        response.Headers.ETag = $"\"{version}\"";
}
