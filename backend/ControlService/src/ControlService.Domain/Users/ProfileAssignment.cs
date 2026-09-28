namespace ControlService.Domain.Users;

/// <summary>One of a user's permission profiles (PERM-01). Mapped to the join table
/// user_permission_profiles, with a foreign key to the profile (PERM-20).</summary>
public sealed record ProfileAssignment(Guid ProfileId);
