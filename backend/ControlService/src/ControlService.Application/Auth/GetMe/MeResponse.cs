namespace ControlService.Application.Auth.GetMe;

/// <summary>The signed-in person and their effective level on every screen, in catalog order,
/// including `negado` (D2). `Level` is the wire value.</summary>
public sealed record MeResponse(Guid Id, string DisplayName, string Login, IReadOnlyList<ScreenLevelResponse> Levels);

public sealed record ScreenLevelResponse(string Screen, string Level);
