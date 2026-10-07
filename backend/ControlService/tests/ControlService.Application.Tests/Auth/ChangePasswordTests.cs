using ControlService.Application.Auth;
using ControlService.Application.Auth.ChangePassword;
using ControlService.Application.Auth.SignIn;
using ControlService.Application.Tests.Fakes;
using ControlService.Domain.Common;

namespace ControlService.Application.Tests.Auth;

public class ChangePasswordTests
{
    [Fact]
    public async Task Short_password_is_refused() // AUTH-16, API-15
    {
        var bed = new AuthTestBed();
        var admin = bed.AddAdminWithInitialPassword("senha-inicial");

        var result = await bed.CreateChangePasswordHandler()
            .Handle(new ChangePasswordCommand(admin.Id, "1234567", "1234567"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("validation_failed");
        result.Error.Message.ShouldBe("Alguns campos precisam ser corrigidos.");
        result.Error.Fields.ShouldNotBeNull();
        result.Error.Fields["password"].ShouldBe(["A senha precisa ter pelo menos 8 caracteres."]);
    }

    [Fact]
    public async Task Password_of_exactly_the_minimum_length_is_accepted() // AUTH-16
    {
        var bed = new AuthTestBed();
        var admin = bed.AddAdminWithInitialPassword("senha-inicial");

        var result = await bed.CreateChangePasswordHandler()
            .Handle(new ChangePasswordCommand(admin.Id, "12345678", "12345678"), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task Different_confirmation_is_refused() // AUTH-16
    {
        var bed = new AuthTestBed();
        var admin = bed.AddAdminWithInitialPassword("senha-inicial");

        var result = await bed.CreateChangePasswordHandler()
            .Handle(new ChangePasswordCommand(admin.Id, "12345678", "87654321"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("validation_failed");
        result.Error.Fields.ShouldNotBeNull();
        result.Error.Fields["passwordConfirmation"].ShouldBe(["As duas senhas não são iguais. Digite de novo."]);
    }

    [Fact]
    public async Task Minimum_length_message_uses_the_configured_length() // AUTH-28
    {
        var bed = new AuthTestBed(new AuthSettings(PasswordMinLength: 10, LockoutMinutes: 15));
        var admin = bed.AddAdminWithInitialPassword("senha-inicial");

        var result = await bed.CreateChangePasswordHandler()
            .Handle(new ChangePasswordCommand(admin.Id, "123456789", "123456789"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Fields.ShouldNotBeNull();
        result.Error.Fields["password"].ShouldBe(["A senha precisa ter pelo menos 10 caracteres."]);
    }

    [Fact]
    public async Task Password_equal_to_the_initial_one_is_refused() // AUTH-25
    {
        var bed = new AuthTestBed();
        var admin = bed.AddAdminWithInitialPassword("senha-inicial");

        var result = await bed.CreateChangePasswordHandler()
            .Handle(new ChangePasswordCommand(admin.Id, "senha-inicial", "senha-inicial"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("validation_failed");
        result.Error.Message.ShouldBe("Alguns campos precisam ser corrigidos.");
        result.Error.Fields.ShouldNotBeNull();
        result.Error.Fields["password"].ShouldBe(["A nova senha precisa ser diferente da senha inicial."]);
        bed.Sessions.SessionsOf(admin.Id).ShouldBeEmpty();
    }

    [Fact]
    public async Task Account_without_a_mandatory_change_cannot_use_change_password() // AUTH-24
    {
        var bed = new AuthTestBed();
        var user = bed.AddActiveUser("ana.souza", "senha-da-ana");

        var result = await bed.CreateChangePasswordHandler()
            .Handle(new ChangePasswordCommand(user.Id, "nova-senha-1", "nova-senha-1"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("forbidden");
        result.Error.Message.ShouldBe("Sua senha já foi criada. Para trocá-la, use “Esqueci minha senha”.");
        bed.Sessions.SessionsOf(user.Id).ShouldBeEmpty();
    }

    [Fact]
    public async Task Changing_the_password_ends_every_session_and_starts_a_new_one()
    {
        var bed = new AuthTestBed();
        var admin = bed.AddAdminWithInitialPassword("senha-inicial");
        var other = bed.AddActiveUser("ana.souza", "senha-da-ana");
        var signIn = bed.CreateSignInHandler();
        await signIn.Handle(new SignInCommand("admin", "senha-inicial"), TestContext.Current.CancellationToken);
        await signIn.Handle(new SignInCommand("admin", "senha-inicial"), TestContext.Current.CancellationToken);
        await signIn.Handle(new SignInCommand("ana.souza", "senha-da-ana"), TestContext.Current.CancellationToken);

        var result = await bed.CreateChangePasswordHandler()
            .Handle(new ChangePasswordCommand(admin.Id, "nova-senha-1", "nova-senha-1"), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        var session = bed.Sessions.SessionsOf(admin.Id).ShouldHaveSingleItem();
        result.Value.RefreshToken.ShouldBe(session.RefreshToken);
        result.Value.AccessToken.ShouldBe(FakeAccessTokenIssuer.TokenFor(admin.Id, session.Id, mustChangePassword: false));
        result.Value.MustChangePassword.ShouldBeFalse();
        (await bed.Credentials.IsCurrentPasswordAsync(admin.Id, "nova-senha-1", TestContext.Current.CancellationToken)).ShouldBeTrue();
        (await bed.Credentials.MustChangePasswordAsync(admin.Id, TestContext.Current.CancellationToken)).ShouldBeFalse();
        bed.Sessions.SessionsOf(other.Id).ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Changing_the_initial_password_fills_the_admin_activation_time() // previous plan, see decision 21 (was USR-34)
    {
        var bed = new AuthTestBed();
        var admin = bed.AddAdminWithInitialPassword("senha-inicial");

        var result = await bed.CreateChangePasswordHandler()
            .Handle(new ChangePasswordCommand(admin.Id, "nova-senha-1", "nova-senha-1"), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        admin.ActivatedAt.ShouldBe(bed.Clock.GetUtcNow());
        bed.UnitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task Change_password_of_a_user_that_no_longer_exists_is_refused_as_session_expired() // AUTH-17
    {
        var bed = new AuthTestBed();

        var result = await bed.CreateChangePasswordHandler()
            .Handle(new ChangePasswordCommand(Guid.CreateVersion7(), "nova-senha-1", "nova-senha-1"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AuthErrors.SessionExpired);
    }

    [Fact]
    public async Task Change_password_stops_when_the_user_cannot_be_saved()
    {
        var bed = new AuthTestBed();
        var admin = bed.AddAdminWithInitialPassword("senha-inicial");
        var conflict = new Error("concurrency_conflict", "Este cadastro foi alterado.");
        bed.UnitOfWork.FailWith = conflict;

        var result = await bed.CreateChangePasswordHandler()
            .Handle(new ChangePasswordCommand(admin.Id, "nova-senha-1", "nova-senha-1"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(conflict);
        (await bed.Credentials.IsCurrentPasswordAsync(admin.Id, "senha-inicial", TestContext.Current.CancellationToken)).ShouldBeTrue();
    }
}
