namespace ControlService.Application.Common;

/// <summary>The signed-in person, backed by the authenticated principal.
/// Outside a request, the system acts as the Admin.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
}
