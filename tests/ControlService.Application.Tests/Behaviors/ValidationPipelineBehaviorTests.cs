using ControlService.Application.Behaviors;
using FluentValidation;
using MediatR;

namespace ControlService.Application.Tests.Behaviors;

public class ValidationPipelineBehaviorTests
{
    public record SampleRequest : IRequest<string>;

    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<SampleRequest>>();
        var behavior = new ValidationPipelineBehavior<SampleRequest, string>(validators);
        var request = new SampleRequest();
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("Success"); };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        var validator = Substitute.For<IValidator<SampleRequest>>();
        var failure = new FluentValidation.Results.ValidationFailure("Prop", "Error");
        validator.Validate(Arg.Any<IValidationContext>()!).Returns(new FluentValidation.Results.ValidationResult(new[] { failure }));

        var validators = new[] { validator };
        var behavior = new ValidationPipelineBehavior<SampleRequest, string>(validators);
        var request = new SampleRequest();
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

        // Act
        var act = async () => await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_ValidationSucceeds_CallsNext()
    {
        // Arrange
        var validator = Substitute.For<IValidator<SampleRequest>>();
        validator.Validate(Arg.Any<IValidationContext>()!).Returns(new FluentValidation.Results.ValidationResult());

        var validators = new[] { validator };
        var behavior = new ValidationPipelineBehavior<SampleRequest, string>(validators);
        var request = new SampleRequest();
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("Success"); };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MultipleValidators_MergesAllFailures()
    {
        // Arrange
        var validatorSum = Substitute.For<IValidator<SampleRequest>>();
        var failure1 = new FluentValidation.Results.ValidationFailure("Prop1", "Error1");
        validatorSum.Validate(Arg.Any<IValidationContext>()!).Returns(new FluentValidation.Results.ValidationResult(new[] { failure1 }));

        var validatorAccount = Substitute.For<IValidator<SampleRequest>>();
        var failure2 = new FluentValidation.Results.ValidationFailure("Prop2", "Error2");
        validatorAccount.Validate(Arg.Any<IValidationContext>()!).Returns(new FluentValidation.Results.ValidationResult(new[] { failure2 }));

        var validators = new[] { validatorSum, validatorAccount };
        var behavior = new ValidationPipelineBehavior<SampleRequest, string>(validators);
        var request = new SampleRequest();
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

        // Act
        var act = async () => await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCount(2);
        exception.Which.Errors.Should().Contain(failure1);
        exception.Which.Errors.Should().Contain(failure2);
    }

    [Fact]
    public async Task Handle_ValidatorReturnsNullFailure_FiltersItOut()
    {
        // Arrange
        var validator = Substitute.For<IValidator<SampleRequest>>();
        var resultWithNull = new FluentValidation.Results.ValidationResult(new[] { (FluentValidation.Results.ValidationFailure)null! });
        validator.Validate(Arg.Any<IValidationContext>()!).Returns(resultWithNull);

        var validators = new[] { validator };
        var behavior = new ValidationPipelineBehavior<SampleRequest, string>(validators);
        var request = new SampleRequest();
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("Success"); };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public void Constructor_NullValidators_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<IValidator<SampleRequest>> validators = null!;

        // Act
        var act = () => new ValidationPipelineBehavior<SampleRequest, string>(validators);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("validators");
    }
}
