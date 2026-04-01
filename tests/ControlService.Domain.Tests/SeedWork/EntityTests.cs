using FluentAssertions;
using System;
using ControlService.Domain.SeedWork;
using Xunit;

namespace ControlService.Domain.Tests.SeedWork;

// Fake subclass to test the abstract Entity class
public class FakeEntity : Entity
{
    public FakeEntity()
    {
    }

    public FakeEntity(Guid id)
    {
        Id = id;
    }
}

public class EntityTests
{
    [Fact]
    public void Constructor_WithoutId_ShouldGenerateNewIdAndSetTimestamps()
    {
        // Arrange & Act
        var entity = new FakeEntity();

        // Assert
        entity.Id.Should().NotBeEmpty();
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TwoEntities_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new FakeEntity(id);
        var entity2 = new FakeEntity(id);

        // Act & Assert
        entity1.Equals(entity2).Should().BeTrue();
        (entity1 == entity2).Should().BeTrue();
        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    [Fact]
    public void TwoEntities_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var entity1 = new FakeEntity(Guid.NewGuid());
        var entity2 = new FakeEntity(Guid.NewGuid());

        // Act & Assert
        entity1.Equals(entity2).Should().BeFalse();
        (entity1 == entity2).Should().BeFalse();
        (entity1 != entity2).Should().BeTrue();
    }

    [Fact]
    public void Entity_ComparedToNull_ShouldNotBeEqual()
    {
        // Arrange
        var entity1 = new FakeEntity();

        // Act & Assert
        entity1.Equals(null).Should().BeFalse();
        (entity1 == null).Should().BeFalse();
        (null == entity1).Should().BeFalse();
    }
}
