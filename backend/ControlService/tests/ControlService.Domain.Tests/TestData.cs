using ControlService.Domain.PermissionProfiles;
using ControlService.Domain.Users;

namespace ControlService.Domain.Tests;

/// <summary>Centralizes aggregate construction so a constructor signature change touches one file (plan of #7).</summary>
internal static class TestData
{
    public static User NewUser() => User.Create();

    public static User NewAdmin() => User.CreateAdmin();

    public static PermissionProfile NewProfile() => PermissionProfile.Create();
}
