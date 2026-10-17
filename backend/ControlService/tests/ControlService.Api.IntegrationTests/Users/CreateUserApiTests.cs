using System.Net;
using ControlService.Api.IntegrationTests.Auth;
using ControlService.Api.IntegrationTests.PermissionProfiles;
using ControlService.Domain.Access;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

    // #11-T08
    [Fact]
    public async Task Required_fields_left_blank_are_refused_each_under_its_own_field() // USR-01, USR-02, USR-04, USR-35
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);
        var body = new Dictionary<string, object?>
        {
            ["fullName"] = "   ",
            ["displayName"] = string.Empty,
            ["login"] = string.Empty,
            ["temporaryPassword"] = string.Empty,
        };

        using var response = await client.SendWithTokenAsync(HttpMethod.Post, UserTestSupport.UsersPath, token, body);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed", ProfileTestSupport.FieldsToFix);
        problem.FieldErrors("fullName").ShouldBe(["Informe o nome completo."]);
        problem.FieldErrors("displayName").ShouldBe(["Informe como o nome vai aparecer no sistema."]);
        problem.FieldErrors("login").ShouldBe(["Informe o login."]);
        problem.FieldErrors("temporaryPassword").ShouldBe(["Informe a senha temporária."]);
    }

    // #11-T09
    [Fact]
    public async Task Fields_in_an_invalid_format_are_refused_each_under_its_own_path() // USR-05, USR-07, USR-08, USR-10, USR-11, USR-12, USR-26
    {
        const string invalidPhone = "Digite o telefone com DDD. Ex.: (21) 98765-4321.";
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);
        var body = UserTestSupport.NewUserBody(ProfileTestSupport.UniqueSuffix());
        body["login"] = "ab";
        body["email"] = "ana";
        body["cpf"] = "111.111.111-11";
        body["phone"] = "123";
        body["bloodType"] = "Z+";
        ((Dictionary<string, object?>)body["address"]!)["cep"] = "123";
        ((Dictionary<string, object?>)body["emergencyContact"]!)["phone"] = "99";

        using var response = await client.SendWithTokenAsync(HttpMethod.Post, UserTestSupport.UsersPath, token, body);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed", ProfileTestSupport.FieldsToFix);
        problem.FieldErrors("login").ShouldBe(
            ["O login deve ter de 3 a 30 caracteres, usando só letras sem acento, números, ponto, hífen ou sublinhado."]);
        problem.FieldErrors("email").ShouldBe(
            ["Este e-mail não parece válido. Confira se ele tem @ e o domínio, por exemplo ana@empresa.com.br."]);
        problem.FieldErrors("cpf").ShouldBe(["Este CPF não é válido. Confira os números ou deixe o campo em branco."]);
        problem.FieldErrors("phone").ShouldBe([invalidPhone]);
        problem.FieldErrors("address.cep").ShouldBe(["O CEP precisa ter 8 números."]);
        problem.FieldErrors("emergencyContact.phone").ShouldBe([invalidPhone]);
        problem.FieldErrors("bloodType").ShouldBe(["Este tipo sanguíneo não é válido."]);
    }

    // #11-T10
    [Fact]
    public async Task A_temporary_password_shorter_than_the_minimum_is_refused() // USR-35, AUTH-16
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);
        var body = UserTestSupport.NewUserBody(ProfileTestSupport.UniqueSuffix());
        body["temporaryPassword"] = "1234567";

        using var response = await client.SendWithTokenAsync(HttpMethod.Post, UserTestSupport.UsersPath, token, body);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed", ProfileTestSupport.FieldsToFix);
        problem.FieldErrors("temporaryPassword").ShouldBe(["A senha precisa ter pelo menos 8 caracteres."]);
    }

    // #11-T11
    [Fact]
    public async Task A_birth_date_after_today_in_brasilia_is_refused() // USR-09, CNV-11
    {
        var originalNow = factory.Clock.GetUtcNow();
        // 01:00 UTC on the 17th is still 22:00 on the 16th in Brasília.
        factory.Clock.Set(new DateTimeOffset(2026, 10, 17, 1, 0, 0, TimeSpan.Zero));
        try
        {
            using var client = factory.CreateHttpsClient();
            var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);
            var body = UserTestSupport.NewUserBody(ProfileTestSupport.UniqueSuffix());
            body["birthDate"] = "2026-10-17";

            using var response = await client.SendWithTokenAsync(HttpMethod.Post, UserTestSupport.UsersPath, token, body);

            var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed", ProfileTestSupport.FieldsToFix);
            problem.FieldErrors("birthDate").ShouldBe(["A data de nascimento não pode ser no futuro."]);
        }
        finally
        {
            factory.Clock.Set(originalNow);
        }
    }

    // #11-T12
    [Fact]
    public async Task A_birth_date_that_does_not_exist_is_refused() // USR-37
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);
        var body = UserTestSupport.NewUserBody(ProfileTestSupport.UniqueSuffix());
        body["birthDate"] = "2026-02-30";

        using var response = await client.SendWithTokenAsync(HttpMethod.Post, UserTestSupport.UsersPath, token, body);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed", ProfileTestSupport.FieldsToFix);
        problem.FieldErrors("birthDate").ShouldBe(["Esta data de nascimento não é válida."]);
    }

    // #11-T13
    [Fact]
    public async Task A_login_already_used_is_refused_ignoring_case() // USR-06, API-13
    {
        var marker = ProfileTestSupport.UniqueSuffix();
        await AuthTestSupport.CreateUserAsync(factory.Services, login: $"ana.{marker}");
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);
        var body = UserTestSupport.NewUserBody(marker);
        body["login"] = $"ANA.{marker}".ToUpperInvariant();

        using var response = await client.SendWithTokenAsync(HttpMethod.Post, UserTestSupport.UsersPath, token, body);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed", ProfileTestSupport.FieldsToFix);
        problem.FieldErrors("login").ShouldBe([$"Já existe um usuário com o login “ana.{marker}”. Escolha outro."]);
    }

    // #11-T14
    [Fact]
    public async Task A_display_name_already_used_is_refused_ignoring_accents_and_case() // USR-03, CNV-09
    {
        var marker = ProfileTestSupport.UniqueSuffix();
        await AuthTestSupport.CreateUserAsync(factory.Services, displayName: $"Ána {marker}");
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);
        var body = UserTestSupport.NewUserBody(marker);
        body["displayName"] = $"ana {marker}";

        using var response = await client.SendWithTokenAsync(HttpMethod.Post, UserTestSupport.UsersPath, token, body);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed", ProfileTestSupport.FieldsToFix);
        problem.FieldErrors("displayName").ShouldBe(
            [$"Já existe um usuário chamado “Ána {marker}”. Use um nome diferente, por exemplo acrescentando o sobrenome ou o setor."]);
    }

    // #11-T15
    [Fact]
    public async Task The_login_and_display_name_of_a_deactivated_user_stay_reserved() // USR-17
    {
        var marker = ProfileTestSupport.UniqueSuffix();
        var former = await AuthTestSupport.CreateUserAsync(factory.Services, login: $"ana.{marker}", displayName: $"Ana {marker}");
        await AuthTestSupport.DeactivateUserAsync(factory.Services, former.Id);
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);

        using var response = await client.SendWithTokenAsync(
            HttpMethod.Post, UserTestSupport.UsersPath, token, UserTestSupport.NewUserBody(marker));

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "validation_failed", ProfileTestSupport.FieldsToFix);
        problem.FieldErrors("login").ShouldBe([$"Já existe um usuário com o login “ana.{marker}”. Escolha outro."]);
        problem.FieldErrors("displayName").ShouldBe(
            [$"Já existe um usuário chamado “Ana {marker}”. Use um nome diferente, por exemplo acrescentando o sobrenome ou o setor."]);
    }

    // #11-T16
    [Fact]
    public async Task The_cpf_and_email_of_another_user_are_accepted() // USR-07, USR-08
    {
        var first = ProfileTestSupport.UniqueSuffix();
        var second = ProfileTestSupport.UniqueSuffix();
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);
        var firstBody = UserTestSupport.NewUserBody(first);
        var secondBody = UserTestSupport.NewUserBody(second);
        secondBody["cpf"] = firstBody["cpf"];
        secondBody["email"] = firstBody["email"];

        using var created = await client.SendWithTokenAsync(HttpMethod.Post, UserTestSupport.UsersPath, token, firstBody);
        using var duplicated = await client.SendWithTokenAsync(HttpMethod.Post, UserTestSupport.UsersPath, token, secondBody);

        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        duplicated.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    // #11-T17
    [Fact]
    public async Task Profiles_that_do_not_exist_are_discarded() // USR-13
    {
        var marker = ProfileTestSupport.UniqueSuffix();
        var existing = await ProfileTestSupport.CreateProfileAsync(factory.Services, $"Vendas {marker}");
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);
        var body = UserTestSupport.NewUserBody(marker);
        body["profileIds"] = new[] { existing, Guid.NewGuid() };

        using var response = await client.SendWithTokenAsync(HttpMethod.Post, UserTestSupport.UsersPath, token, body);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var user = await response.ReadJsonAsync();
        user.GetProperty("profileIds").EnumerateArray().Select(id => id.GetGuid()).ShouldBe([existing]);
        (await UserTestSupport.FindUserAsync(factory.Services, user.GetProperty("id").GetGuid())).ProfileIds.ShouldBe([existing]);
    }

    // #11-T18
    [Fact]
    public async Task A_reader_on_users_cannot_register_a_user() // PERM-03
    {
        using var client = factory.CreateHttpsClient();
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Reader);

        using var response = await client.SendWithTokenAsync(
            HttpMethod.Post, UserTestSupport.UsersPath, token, UserTestSupport.NewUserBody(ProfileTestSupport.UniqueSuffix()));

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "forbidden", ProfileTestSupport.Denied);
    }

    // #11-T19
    [Fact]
    public async Task Registering_a_user_writes_no_password_or_personal_data_to_the_logs() // AUTH-20, LGPD
    {
        var logs = new CapturingLoggerProvider();
        using var logged = factory.WithWebHostBuilder(builder => builder.ConfigureLogging(logging =>
        {
            // A rule of this provider wins over the appsettings rules (Default: Information).
            logging.AddFilter<CapturingLoggerProvider>(category: null, LogLevel.Trace);
            logging.Services.AddSingleton<ILoggerProvider>(logs);
        }));
        using var client = logged.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = false,
        });
        var (_, token) = await ProfileTestSupport.SignInWithLevelAsync(factory, client, ScreenKeys.Users, AccessLevel.Editor);
        var body = UserTestSupport.NewUserBody(ProfileTestSupport.UniqueSuffix());
        body["bloodType"] = "AB-";
        body["temporaryPassword"] = "senha-secreta-9";

        using var response = await client.SendWithTokenAsync(HttpMethod.Post, UserTestSupport.UsersPath, token, body);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        // Below Information is where frameworks write their diagnostics: the capture must really reach it.
        logs.Lines.ShouldContain(line => line.StartsWith($"{LogLevel.Debug} ", StringComparison.Ordinal));
        string[] secrets = ["senha-secreta-9", "52998224725", "529.982.247-25", "987654321", "12.345.678-9", "AB-"];
        foreach (var secret in secrets)
        {
            logs.Lines.ShouldNotContain(line => line.Contains(secret, StringComparison.Ordinal), secret);
        }
    }
}
