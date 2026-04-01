using FluentAssertions;
using ControlService.Domain.Commercial.Customers.ValueObjects;
using Xunit;

namespace ControlService.Domain.Tests.Commercial.Customers.ValueObjects;

public class ValidationResultTests
{
    [Fact]
    public void Success_ShouldProduceValidResult()
    {
        var result = ValidationResult.Success();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithSingleError_ShouldProduceInvalidResult()
    {
        var result = ValidationResult.Failure("Error 1");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle("Error 1");
    }

    [Fact]
    public void Failure_WithMultipleErrors_ShouldPreserveOrderAndCount()
    {
        var result = ValidationResult.Failure("Error 1", "Error 2");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2).And.ContainInOrder("Error 1", "Error 2");
    }

    [Fact]
    public void Failure_WithNoArgs_ShouldProduceInvalidResultWithNoErrors()
    {
        var result = ValidationResult.Failure();

        result.IsValid.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithErrorCollection_ShouldProduceInvalidResultWithAllErrors()
    {
        IEnumerable<string> errors = ["Error A", "Error B", "Error C"];

        var result = ValidationResult.Failure(errors);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3).And.ContainInOrder("Error A", "Error B", "Error C");
    }

    [Fact]
    public void Combine_TwoSuccesses_ShouldReturnSuccess()
    {
        var left = ValidationResult.Success();
        var right = ValidationResult.Success();

        var combined = left.Combine(right);

        combined.IsValid.Should().BeTrue();
        combined.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Combine_SuccessWithFailure_ShouldReturnFailureWithRightErrors()
    {
        var success = ValidationResult.Success();
        var failure = ValidationResult.Failure("Right Error");

        var combined = success.Combine(failure);

        combined.IsValid.Should().BeFalse();
        combined.Errors.Should().ContainSingle("Right Error");
    }

    [Fact]
    public void Combine_FailureWithSuccess_ShouldReturnFailureWithLeftErrors()
    {
        var failure = ValidationResult.Failure("Left Error");
        var success = ValidationResult.Success();

        var combined = failure.Combine(success);

        combined.IsValid.Should().BeFalse();
        combined.Errors.Should().ContainSingle("Left Error");
    }

    [Fact]
    public void Combine_TwoFailures_ShouldMergeErrorsFromLeftThenRight()
    {
        var left = ValidationResult.Failure("Left Error");
        var right = ValidationResult.Failure("Right Error");

        var combined = left.Combine(right);

        combined.IsValid.Should().BeFalse();
        combined.Errors.Should().HaveCount(2).And.ContainInOrder("Left Error", "Right Error");
    }

    [Fact]
    public void Equals_TwoSuccesses_ShouldBeEqualByValue()
    {
        var first = ValidationResult.Success();
        var second = ValidationResult.Success();

        first.Should().Be(second);
    }

    [Fact]
    public void Equals_TwoFailuresWithSameErrors_ShouldBeEqualByValue()
    {
        var first = ValidationResult.Failure("Error 1", "Error 2");
        var second = ValidationResult.Failure("Error 1", "Error 2");

        first.Should().Be(second);
    }

    [Fact]
    public void Equals_FailureAndSuccess_ShouldNotBeEqual()
    {
        var success = ValidationResult.Success();
        var failure = ValidationResult.Failure("Error");

        success.Should().NotBe(failure);
    }

    [Fact]
    public void Equals_FailuresWithDifferentErrors_ShouldNotBeEqual()
    {
        var first = ValidationResult.Failure("Error 1");
        var second = ValidationResult.Failure("Error 2");

        first.Should().NotBe(second);
    }
}
