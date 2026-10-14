namespace ControlService.Application.PermissionProfiles.UpdateProfile;

/// <summary>`Version` is what the screen read, from `If-Match`. `Levels` replaces every stored level: a screen
/// left out becomes `negado`.</summary>
public sealed record UpdateProfileCommand(Guid Id, uint Version, string Name, string Description, IReadOnlyList<LevelInput> Levels);
