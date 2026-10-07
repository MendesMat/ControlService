using ControlService.Domain.Access;

namespace ControlService.Domain.PermissionProfiles;

/// <summary>A profile's access level for one screen. A screen missing from the
/// collection means Denied, so Denied is never stored explicitly.</summary>
public sealed record ScreenLevel(ScreenKey Screen, AccessLevel Level);
