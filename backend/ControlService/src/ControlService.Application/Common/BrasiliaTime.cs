namespace ControlService.Application.Common;

/// <summary>The business runs on Brasília time (CNV-11): "today" is the date there, not the server's or UTC's.</summary>
public static class BrasiliaTime
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateOnly Today(TimeProvider timeProvider) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), Zone).DateTime);
}
