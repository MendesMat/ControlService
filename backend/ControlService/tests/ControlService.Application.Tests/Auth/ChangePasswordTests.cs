using ControlService.Application.Auth;
using ControlService.Application.Auth.ChangePassword;

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
}
