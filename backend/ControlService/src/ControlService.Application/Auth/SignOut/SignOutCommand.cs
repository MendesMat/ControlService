namespace ControlService.Application.Auth.SignOut;

/// <summary>`SessionId` comes from the `sid` claim of the access token: the refresh cookie is
/// scoped to the refresh route, so sign-out never receives it.</summary>
public sealed record SignOutCommand(Guid SessionId);
