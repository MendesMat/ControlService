using ControlService.Domain.Access;

namespace ControlService.Application.Auth.CheckScreenAccess;

/// <summary>Asks whether the person reaches at least <paramref name="Minimum"/> on <paramref name="Screen"/>.</summary>
public sealed record CheckScreenAccessQuery(Guid UserId, ScreenKey Screen, AccessLevel Minimum);
