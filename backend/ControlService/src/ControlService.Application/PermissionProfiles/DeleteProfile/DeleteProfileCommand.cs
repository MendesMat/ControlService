namespace ControlService.Application.PermissionProfiles.DeleteProfile;

/// <summary>`Version` is what the screen read, from `If-Match`.</summary>
public sealed record DeleteProfileCommand(Guid Id, uint Version);
