namespace ControlService.Application.PermissionProfiles.ListProfiles;

/// <summary>One line of the profile list. `UserCount` includes deactivated users, the same number that blocks
/// the deletion (PERM-20); `GrantedCount` is the screens above `negado` out of `ScreenCount`.</summary>
public sealed record ProfileListItemResponse(
    Guid Id, string Name, string Description, bool IsSystem, int UserCount, int GrantedCount, int ScreenCount);
