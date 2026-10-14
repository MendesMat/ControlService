namespace ControlService.Application.PermissionProfiles.CreateProfile;

/// <summary>A screen missing from `Levels` is left at `negado` (PERM-18); one sent twice keeps the last.</summary>
public sealed record CreateProfileCommand(string Name, string Description, IReadOnlyList<LevelInput> Levels);
