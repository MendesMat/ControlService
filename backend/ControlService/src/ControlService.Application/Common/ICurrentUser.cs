namespace ControlService.Application.Common;

/// <summary>The signed-in person, backed by the authenticated principal (ADR-0015, ADR-0019).
/// Outside a request, the system acts as the Admin.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
}
