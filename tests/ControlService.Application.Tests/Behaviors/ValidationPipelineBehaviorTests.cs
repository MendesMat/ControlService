using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ControlService.Application.Behaviors;
using FluentValidation;
using MediatR;

namespace ControlService.Application.Tests.Behaviors;

public class ValidationPipelineBehaviorTests
{
    public class SampleRequest : IRequest<string> { }

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
        validator.Validate(Arg.Any<IValidationContext>()).Returns(new FluentValidation.Results.ValidationResult(new[] { failure }));
        
        var validators = new[] { validator };
        var behavior = new ValidationPipelineBehavior<SampleRequest, string>(validators);
        var request = new SampleRequest();
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

        // Act
        var act = async () => await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
}
