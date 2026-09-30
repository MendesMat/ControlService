namespace ControlService.API.Common;

/// <summary>The `/api/v1` group every feature registers its endpoints under (ADR-0003, API-01).
/// `Program.cs` and the test factories both map it through here.</summary>
public static class ApiV1Group
{
    public static RouteGroupBuilder MapApiV1(this IEndpointRouteBuilder app) => app.MapGroup("/api/v1");
}
