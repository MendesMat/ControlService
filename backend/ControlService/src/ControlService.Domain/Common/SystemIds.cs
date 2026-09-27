namespace ControlService.Domain.Common;

/// <summary>Fixed, well-known ids of the system records that always exist (ADR-0022).</summary>
public static class SystemIds
{
    public static readonly Guid AdminUser = new("00000000-0000-7000-8000-000000000001");
    public static readonly Guid ManagerProfile = new("00000000-0000-7000-8000-000000000002");
}
