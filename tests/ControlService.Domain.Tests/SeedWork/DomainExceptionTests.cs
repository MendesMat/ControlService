using FluentAssertions;
using System;
using ControlService.Domain.SeedWork;
using Xunit;

namespace ControlService.Domain.Tests.SeedWork;

public class DomainExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessageProperty()
    {
        // Arrange
        string message = "Test exception message";

        // Act
        var exception = new DomainException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetBothProperties()
    {
        // Arrange
        string message = "Test exception message";
        var innerException = new Exception("Inner exception");

        // Act
        var exception = new DomainException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void Type_ShouldBeDerivedFromException()
    {
        // Arrange & Act
        var exception = new DomainException("Test");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }
}
