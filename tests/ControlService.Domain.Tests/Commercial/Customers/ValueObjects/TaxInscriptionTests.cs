using FluentAssertions;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.Commercial.Customers.ValueObjects;

namespace ControlService.Domain.Tests.Commercial.Customers.ValueObjects;

public class TaxInscriptionTests
{
    public class Create
    {
        [Theory]
        [InlineData("12.345/6", "123456")]
        [InlineData("12.345-6", "123456")]
        [InlineData("12 345 6", "123456")]
        [InlineData("123456", "123456")]
        [InlineData("", "")]
        public void GivenMunicipalValue_WhenCreated_ShouldStripFormatting(string raw, string expected)
        {
            var inscription = TaxInscription.Create(raw, TaxInscriptionType.Municipal);

            inscription.Value.Should().Be(expected);
        }

        [Theory]
        [InlineData("12.345.678/9", "123456789")]
        [InlineData("12.345.678-9", "123456789")]
        [InlineData("123456789", "123456789")]
        public void GivenStateValue_WhenCreated_ShouldStripFormatting(string raw, string expected)
        {
            var inscription = TaxInscription.Create(raw, TaxInscriptionType.State);

            inscription.Value.Should().Be(expected);
        }

        [Fact]
        public void GivenNullValue_WhenCreated_ShouldProduceEmptyValue()
        {
            var inscription = TaxInscription.Create(null!, TaxInscriptionType.Municipal);

            inscription.Value.Should().BeEmpty();
        }

        [Fact]
        public void GivenMunicipalType_WhenCreated_ShouldPreserveType()
        {
            var inscription = TaxInscription.Create("123", TaxInscriptionType.Municipal);

            inscription.Type.Should().Be(TaxInscriptionType.Municipal);
        }

        [Fact]
        public void GivenStateType_WhenCreated_ShouldPreserveType()
        {
            var inscription = TaxInscription.Create("123", TaxInscriptionType.State);

            inscription.Type.Should().Be(TaxInscriptionType.State);
        }
    }

    public class GetFormattedValue
    {
        [Fact]
        public void GivenCreatedInscription_WhenFormattingRequested_ShouldReturnStrippedValue()
        {
            var inscription = TaxInscription.Create("12.345/6", TaxInscriptionType.Municipal);

            inscription.GetFormattedValue().Should().Be("123456");
        }
    }

    public class Equality
    {
        [Fact]
        public void GivenSameValueAndType_WhenCompared_ShouldBeEqual()
        {
            var first = TaxInscription.Create("123456", TaxInscriptionType.Municipal);
            var second = TaxInscription.Create("123456", TaxInscriptionType.Municipal);

            first.Should().Be(second);
        }

        [Fact]
        public void GivenSameValueAndType_WhenComparedWithOperator_ShouldBeEqual()
        {
            var first = TaxInscription.Create("123456", TaxInscriptionType.Municipal);
            var second = TaxInscription.Create("123456", TaxInscriptionType.Municipal);

            (first == second).Should().BeTrue();
        }

        [Fact]
        public void GivenDifferentValues_WhenCompared_ShouldNotBeEqual()
        {
            var first = TaxInscription.Create("111111", TaxInscriptionType.Municipal);
            var second = TaxInscription.Create("999999", TaxInscriptionType.Municipal);

            first.Should().NotBe(second);
            (first != second).Should().BeTrue();
        }

        [Fact]
        public void GivenDifferentTypes_WhenCompared_ShouldNotBeEqual()
        {
            var municipal = TaxInscription.Create("123456", TaxInscriptionType.Municipal);
            var state = TaxInscription.Create("123456", TaxInscriptionType.State);

            municipal.Should().NotBe(state);
            (municipal != state).Should().BeTrue();
        }

        [Fact]
        public void GivenSameValueAndType_WhenHashCodeComputed_ShouldProduceSameHash()
        {
            var first = TaxInscription.Create("123456", TaxInscriptionType.Municipal);
            var second = TaxInscription.Create("123456", TaxInscriptionType.Municipal);

            first.GetHashCode().Should().Be(second.GetHashCode());
        }

        [Fact]
        public void GivenFormattedAndRawEquivalents_WhenCompared_ShouldBeEqual()
        {
            var formatted = TaxInscription.Create("12.345/6", TaxInscriptionType.Municipal);
            var raw = TaxInscription.Create("123456", TaxInscriptionType.Municipal);

            formatted.Should().Be(raw);
        }
    }
}
