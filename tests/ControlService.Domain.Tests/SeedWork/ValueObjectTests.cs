using FluentAssertions;
using System.Collections.Generic;
using ControlService.Domain.SeedWork;
using Xunit;

namespace ControlService.Domain.Tests.SeedWork;

public class AddressValueObject : ValueObject
{
    public string Street { get; }
    public string City { get; }

    public AddressValueObject(string street, string city)
    {
        Street = street;
        City = city;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
    }
}

public class ValueObjectTests
{
    [Fact]
    public void TwoValueObjects_WithSameProperties_ShouldBeEqual()
    {
        var address1 = new AddressValueObject("Main St", "NY");
        var address2 = new AddressValueObject("Main St", "NY");

        address1.Equals(address2).Should().BeTrue();
        (address1 == address2).Should().BeTrue();
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }

    [Fact]
    public void TwoValueObjects_WithDifferentProperties_ShouldNotBeEqual()
    {
        var address1 = new AddressValueObject("Main St", "NY");
        var address2 = new AddressValueObject("2nd St", "NY");

        address1.Equals(address2).Should().BeFalse();
        (address1 == address2).Should().BeFalse();
        (address1 != address2).Should().BeTrue();
    }
}
