using ControlService.Domain.PermissionProfiles;
using ControlService.Domain.Users;

namespace ControlService.Domain.Tests;

/// <summary>Centralizes aggregate construction so a constructor signature change touches one file (plan of #7).</summary>
internal static class TestData
{
    public static User NewUser() => User.Create(
        Login.Create("test.user").Value,
        EmailAddress.Create("test.user@example.com").Value,
        "Test User",
        "Test User Full Name");

    public static User NewAdmin() => User.CreateAdmin();

    public static PermissionProfile NewProfile() => PermissionProfile.Create();
}
