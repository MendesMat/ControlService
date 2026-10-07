using ControlService.Application.Common;
using ControlService.Domain.Common;

namespace ControlService.Api.IntegrationTests.Persistence;

/// <summary>ICurrentUser controlled by the test, starting as the Admin so a save doesn't
/// need an explicit sign-in to satisfy the authorship foreign keys.</summary>
public sealed class FakeCurrentUser : ICurrentUser
{
    public Guid? UserId { get; set; } = SystemIds.AdminUser;
}
