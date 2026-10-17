using System.Net;
using ControlService.Api.IntegrationTests.Auth;
using ControlService.Api.IntegrationTests.PermissionProfiles;
using ControlService.Domain.Access;

namespace ControlService.Api.IntegrationTests.Users;

public sealed class CreateUserApiTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    // #11-T05
    [Fact]
    public async Task An_editor_registers_a_user_with_every_field() // API-05, AUTH-02, CNV-07, CNV-19
    {
        var marker = ProfileTestSupport.UniqueSuffix();
        using var client = factory.CreateHttpsClient();
        var (editor, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);

        using var response = await client.SendWithTokenAsync(HttpMethod.Post, UserTestSupport.UsersPath, token, UserTestSupport.NewUserBody(marker));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var user = await response.ReadJsonAsync();
        var id = user.GetProperty("id").GetGuid();
        response.Headers.Location.ShouldNotBeNull().OriginalString.ShouldBe($"{UserTestSupport.UsersPath}/{id}");
        var version = user.GetProperty("version").GetString();
        response.Headers.ETag.ShouldNotBeNull().Tag.ShouldBe($"\"{version}\"");
        user.GetProperty("status").GetString().ShouldBe("active");
        user.GetProperty("isSystem").GetBoolean().ShouldBeFalse();
        user.GetProperty("login").GetString().ShouldBe($"ana.{marker}");
        user.GetProperty("email").GetString().ShouldBe($"ana.{marker}@example.com");
        user.GetProperty("fullName").GetString().ShouldBe($"Ana Paula Souza {marker}");
        user.GetProperty("displayName").GetString().ShouldBe($"Ana {marker}");
        user.GetProperty("cpf").GetString().ShouldBe("52998224725");
        user.GetProperty("rg").GetString().ShouldBe("12.345.678-9");
        user.GetProperty("birthDate").GetString().ShouldBe("1990-04-17");
        user.GetProperty("bloodType").GetString().ShouldBe("O+");
        user.GetProperty("phone").GetString().ShouldBe("21987654321");
        var address = user.GetProperty("address");
        address.GetProperty("cep").GetString().ShouldBe("20040020");
        address.GetProperty("street").GetString().ShouldBe("Rua da Assembleia");
        address.GetProperty("number").GetString().ShouldBe("10");
        address.GetProperty("complement").GetString().ShouldBe("sala 501");
        address.GetProperty("district").GetString().ShouldBe("Centro");
        address.GetProperty("city").GetString().ShouldBe("Rio de Janeiro");
        var emergencyContact = user.GetProperty("emergencyContact");
        emergencyContact.GetProperty("name").GetString().ShouldBe("Marcos Souza");
        emergencyContact.GetProperty("relationship").GetString().ShouldBe("irmão");
        emergencyContact.GetProperty("phone").GetString().ShouldBe("21998765432");
        user.GetProperty("profileIds").GetArrayLength().ShouldBe(0);
        user.GetProperty("createdBy").GetGuid().ShouldBe(editor.Id);
        user.GetProperty("createdByName").GetString().ShouldBe(editor.DisplayName);
        user.GetProperty("updatedBy").GetGuid().ShouldBe(editor.Id);
        user.GetProperty("updatedByName").GetString().ShouldBe(editor.DisplayName);
        user.GetProperty("deactivatedAt").GetString().ShouldBe(string.Empty);
        user.GetProperty("deactivatedBy").GetString().ShouldBe(string.Empty);
        user.GetProperty("deactivatedByName").GetString().ShouldBe(string.Empty);
        user.TryGetProperty("temporaryPassword", out _).ShouldBeFalse();

        var stored = await UserTestSupport.FindUserAsync(factory.Services, id);
        stored.PersonalData.Cpf.ShouldNotBeNull().Value.ShouldBe("52998224725");
        stored.PersonalData.Rg.ShouldBe("12.345.678-9");
        stored.PersonalData.BirthDate.ShouldBe(new DateOnly(1990, 4, 17));
        stored.PersonalData.BloodType.ShouldNotBeNull().Value.ShouldBe("O+");
        stored.PersonalData.Phone.ShouldNotBeNull().Value.ShouldBe("21987654321");
        stored.PersonalData.Address.Cep.ShouldNotBeNull().Value.ShouldBe("20040020");
        stored.PersonalData.Address.City.ShouldBe("Rio de Janeiro");
        stored.PersonalData.EmergencyContact.Phone.ShouldNotBeNull().Value.ShouldBe("21998765432");
    }

    // #11-T06
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Optional_fields_left_out_or_null_are_stored_as_empty_text(bool sendNulls) // CNV-03, USR-21
    {
        var marker = ProfileTestSupport.UniqueSuffix();
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);
        var body = new Dictionary<string, object?>
        {
            ["login"] = $"bia.{marker}",
            ["fullName"] = $"Bia Lima {marker}",
            ["displayName"] = $"Bia {marker}",
            ["temporaryPassword"] = UserTestSupport.TemporaryPassword,
        };
        if (sendNulls)
        {
            foreach (var optional in new[] { "email", "cpf", "rg", "birthDate", "bloodType", "phone", "address", "emergencyContact", "profileIds" })
            {
                body[optional] = null;
            }
        }

        using var response = await client.SendWithTokenAsync(HttpMethod.Post, UserTestSupport.UsersPath, token, body);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var user = await response.ReadJsonAsync();
        foreach (var optional in new[] { "email", "cpf", "rg", "birthDate", "bloodType", "phone" })
        {
            user.GetProperty(optional).GetString().ShouldBe(string.Empty, optional);
        }

        foreach (var part in new[] { "cep", "street", "number", "complement", "district", "city" })
        {
            user.GetProperty("address").GetProperty(part).GetString().ShouldBe(string.Empty, $"address.{part}");
        }

        foreach (var part in new[] { "name", "relationship", "phone" })
        {
            user.GetProperty("emergencyContact").GetProperty(part).GetString().ShouldBe(string.Empty, $"emergencyContact.{part}");
        }

        user.GetProperty("profileIds").GetArrayLength().ShouldBe(0);
    }

    // #11-T07
    [Fact]
    public async Task The_new_user_signs_in_with_the_temporary_password_and_must_replace_it() // AUTH-02, AUTH-14, AUTH-16
    {
        var marker = ProfileTestSupport.UniqueSuffix();
        const string password = " temporaria 1 ";
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);
        var body = UserTestSupport.NewUserBody(marker);
        body["temporaryPassword"] = password;
        using var created = await client.SendWithTokenAsync(HttpMethod.Post, UserTestSupport.UsersPath, token, body);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);

        using var signIn = await client.SignInAsync($"ana.{marker}", password);

        signIn.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await signIn.ReadJsonAsync()).GetProperty("mustChangePassword").GetBoolean().ShouldBeTrue();
    }
}
