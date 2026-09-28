using ControlService.Application.Common;
using ControlService.Domain.Common;
using FluentValidation;

namespace ControlService.Application.Tests.Common;

public class ValidatingCommandHandlerTests
{
    [Fact]
    public async Task Valid_command_reaches_the_handler() // ADR-0008
    {
        var innerHandler = new FakeCommandHandler();
        var sut = new ValidatingCommandHandler<TestCommand, TestResponse>(innerHandler, new TestCommandValidator());

        var command = new TestCommand("ok", new EmergencyContact("21987654321"));

        var result = await sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("ok");
        innerHandler.WasCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Invalid_command_returns_validation_failed_and_never_reaches_the_handler() // ADR-0008
    {
        var innerHandler = new FakeCommandHandler();
        var sut = new ValidatingCommandHandler<TestCommand, TestResponse>(innerHandler, new TestCommandValidator());

        var command = new TestCommand("", new EmergencyContact("21987654321"));

        var result = await sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("validation_failed");
        innerHandler.WasCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Validation_errors_use_camelCase_field_paths() // ADR-0008
    {
        var innerHandler = new FakeCommandHandler();
        var sut = new ValidatingCommandHandler<TestCommand, TestResponse>(innerHandler, new TestCommandValidator());
        var command = new TestCommand("ok", new EmergencyContact(""));

        var result = await sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.Fields.ShouldNotBeNull();
        result.Error.Fields.ShouldContainKey("emergencyContact.phone");
        result.Error.Fields["emergencyContact.phone"].ShouldContain("Digite o telefone com DDD. Ex.: (21) 98765-4321.");
    }

    [Fact]
    public async Task Command_with_no_validation_rules_passes_through() // ADR-0008
    {
        var innerHandler = new FakeCommandHandler();
        var sut = new ValidatingCommandHandler<TestCommand, TestResponse>(innerHandler, new EmptyValidator());
        var command = new TestCommand("", new EmergencyContact(""));

        var result = await sut.Handle(command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        innerHandler.WasCalled.ShouldBeTrue();
    }

    private sealed record TestCommand(string Value, EmergencyContact EmergencyContact);

    private sealed record EmergencyContact(string Phone);

    private sealed record TestResponse(string Value);

    private sealed class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(command => command.Value).NotEmpty();
            RuleFor(command => command.EmergencyContact.Phone)
                .NotEmpty()
                .WithMessage("Digite o telefone com DDD. Ex.: (21) 98765-4321.");
        }
    }

    private sealed class EmptyValidator : AbstractValidator<TestCommand>;

    private sealed class FakeCommandHandler : ICommandHandler<TestCommand, TestResponse>
    {
        public bool WasCalled { get; private set; }

        public Task<Result<TestResponse>> Handle(TestCommand command, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult(Result<TestResponse>.Success(new TestResponse(command.Value)));
        }
    }
}
