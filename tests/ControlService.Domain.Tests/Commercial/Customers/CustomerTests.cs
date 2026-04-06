using FluentAssertions;
using ControlService.Domain.Commercial.Customers;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.Commercial.Customers.ValueObjects;
using ControlService.Domain.SeedWork;

namespace ControlService.Domain.Tests.Commercial.Customers;

public class CustomerTests
{
    // -------------------------------------------------------------------------
    // Builders
    // -------------------------------------------------------------------------

    private static Address CreateValidAddress() =>
        Address.Create("12345678", "Rua das Flores", "100", null, "Centro", "São Paulo", "SP");

    private static Customer CreateValidIndividual() =>
        new(CustomerType.Individual, "João da Silva", null, null, CreateValidAddress());

    private static Customer CreateValidBusiness() =>
        new(CustomerType.Business, "Empresa Ltda", "Nome Fantasia", null, CreateValidAddress());

    private static Phone CreateMobilePhone(bool isMain = false) =>
        Phone.Create("11999999999", PhoneType.Mobile, isMain);

    private static Email CreateWorkEmail() =>
        Email.Create("contato@empresa.com.br", EmailType.Work);

    // =========================================================================
    // Construction
    // =========================================================================

    [Fact]
    public void Constructor_WithValidIndividual_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var address = CreateValidAddress();

        // Act
        var customer = new Customer(CustomerType.Individual, "João da Silva", null, null, address);

        // Assert
        customer.Should().BeEquivalentTo(new
        {
            Type = CustomerType.Individual,
            LegalName = "João da Silva",
            Status = CustomerStatus.Active,
            Address = address
        });
    }

    [Fact]
    public void Constructor_WithValidBusiness_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var address = CreateValidAddress();
        var document = Document.Create("12345678000195");

        // Act
        var customer = new Customer(CustomerType.Business, "Empresa Ltda", "Nome Fantasia", document, address);

        // Assert
        customer.Should().BeEquivalentTo(new
        {
            LegalName = "Empresa Ltda",
            TradeName = "Nome Fantasia",
            Type = CustomerType.Business,
            Document = document,
            Status = CustomerStatus.Active
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyLegalName_ShouldThrowDomainException(string? legalName)
    {
        // Arrange
        var address = CreateValidAddress();

        // Act
        var action = () => new Customer(CustomerType.Individual, legalName!, null, null, address);

        // Assert
        action.Should().Throw<DomainException>().WithMessage("Nome é obrigatório.");
    }

    [Fact]
    public void Constructor_WithNullDocument_ShouldAllowCreation()
    {
        // Arrange & Act
        var customer = new Customer(CustomerType.Individual, "João da Silva", null, null, CreateValidAddress());

        // Assert
        customer.Document.Should().BeNull();
    }

    // =========================================================================
    // Phone management
    // =========================================================================

    [Fact]
    public void AddGeneralPhone_WithNewPhone_ShouldAddToCollection()
    {
        // Arrange
        var customer = CreateValidIndividual();
        var phone = CreateMobilePhone();

        // Act
        customer.AddGeneralPhone(phone);

        // Assert
        customer.Phones.Should().ContainSingle().Which.Should().Be(phone);
    }

    [Fact]
    public void AddGeneralPhone_WithDuplicatePhone_ShouldNotAddAgain()
    {
        // Arrange
        var customer = CreateValidIndividual();
        var phone = CreateMobilePhone();

        // Act
        customer.AddGeneralPhone(phone);
        customer.AddGeneralPhone(phone);

        // Assert
        customer.Phones.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveGeneralPhone_WithExistingPhone_ShouldRemoveFromCollection()
    {
        // Arrange
        var customer = CreateValidIndividual();
        var phone = CreateMobilePhone();
        customer.AddGeneralPhone(phone);

        // Act
        customer.RemoveGeneralPhone(phone);

        // Assert
        customer.Phones.Should().BeEmpty();
    }

    [Fact]
    public void RemoveGeneralPhone_WithNonExistingPhone_ShouldNotThrow()
    {
        // Arrange
        var customer = CreateValidIndividual();
        var phone = CreateMobilePhone();

        // Act
        var action = () => customer.RemoveGeneralPhone(phone);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Phones_Collection_ShouldBeImmutableFromOutside()
    {
        // Arrange
        var customer = CreateValidIndividual();
        customer.AddGeneralPhone(CreateMobilePhone());

        // Act
        var action = () =>
        {
            var mutableList = (IList<Phone>)customer.Phones;
            mutableList.Add(CreateMobilePhone());
        };

        // Assert
        action.Should().Throw<NotSupportedException>();
    }

    // =========================================================================
    // Email management
    // =========================================================================

    [Fact]
    public void AddGeneralEmail_WithNewEmail_ShouldAddToCollection()
    {
        // Arrange
        var customer = CreateValidBusiness();
        var email = CreateWorkEmail();

        // Act
        customer.AddGeneralEmail(email);

        // Assert
        customer.Emails.Should().ContainSingle().Which.Should().Be(email);
    }

    [Fact]
    public void AddGeneralEmail_WithDuplicateEmail_ShouldNotAddAgain()
    {
        // Arrange
        var customer = CreateValidBusiness();
        var email = CreateWorkEmail();

        // Act
        customer.AddGeneralEmail(email);
        customer.AddGeneralEmail(email);

        // Assert
        customer.Emails.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveGeneralEmail_WithExistingEmail_ShouldRemoveFromCollection()
    {
        // Arrange
        var customer = CreateValidBusiness();
        var email = CreateWorkEmail();
        customer.AddGeneralEmail(email);

        // Act
        customer.RemoveGeneralEmail(email);

        // Assert
        customer.Emails.Should().BeEmpty();
    }

    [Fact]
    public void Emails_Collection_ShouldBeImmutableFromOutside()
    {
        // Arrange
        var customer = CreateValidBusiness();
        customer.AddGeneralEmail(CreateWorkEmail());

        // Act
        var action = () =>
        {
            var mutableList = (IList<Email>)customer.Emails;
            mutableList.Add(CreateWorkEmail());
        };

        // Assert
        action.Should().Throw<NotSupportedException>();
    }

    // =========================================================================
    // Contact persons management
    // =========================================================================

    [Fact]
    public void AddContactPerson_WithValidName_ShouldAddToCollection()
    {
        // Arrange
        var customer = CreateValidBusiness();

        // Act
        customer.AddContactPerson("Maria Oliveira");

        // Assert
        customer.ContactPersons.Should().ContainSingle("Maria Oliveira");
    }

    [Fact]
    public void AddContactPerson_IsCaseInsensitiveDuplicate_ShouldNotAddAgain()
    {
        // Arrange
        var customer = CreateValidBusiness();

        // Act
        customer.AddContactPerson("Maria Oliveira");
        customer.AddContactPerson("MARIA OLIVEIRA");

        // Assert
        customer.ContactPersons.Should().HaveCount(1);
    }

    [Fact]
    public void AddContactPerson_ShouldTrimWhitespace()
    {
        // Arrange
        var customer = CreateValidBusiness();

        // Act
        customer.AddContactPerson("  Ana Paula  ");

        // Assert
        customer.ContactPersons.Should().ContainSingle("Ana Paula");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddContactPerson_WithBlankName_ShouldNotAddToCollection(string? name)
    {
        // Arrange
        var customer = CreateValidBusiness();

        // Act
        customer.AddContactPerson(name!);

        // Assert
        customer.ContactPersons.Should().BeEmpty();
    }

    [Fact]
    public void RemoveContactPerson_WithExistingName_ShouldRemoveFromCollection()
    {
        // Arrange
        var customer = CreateValidBusiness();
        customer.AddContactPerson("Carlos");

        // Act
        customer.RemoveContactPerson("Carlos");

        // Assert
        customer.ContactPersons.Should().BeEmpty();
    }

    [Fact]
    public void RemoveContactPerson_IsCaseInsensitive()
    {
        // Arrange
        var customer = CreateValidBusiness();
        customer.AddContactPerson("Carlos");

        // Act
        customer.RemoveContactPerson("CARLOS");

        // Assert
        customer.ContactPersons.Should().BeEmpty();
    }

    [Fact]
    public void RemoveContactPerson_WithWhitespace_ShouldTrimBeforeRemoving()
    {
        // Arrange
        var customer = CreateValidBusiness();
        customer.AddContactPerson("Carlos");

        // Act
        customer.RemoveContactPerson("  Carlos  ");

        // Assert
        customer.ContactPersons.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RemoveContactPerson_WithBlankName_ShouldNotThrow(string? name)
    {
        // Arrange
        var customer = CreateValidBusiness();
        customer.AddContactPerson("Carlos");

        // Act
        var action = () => customer.RemoveContactPerson(name!);

        // Assert
        action.Should().NotThrow();
        customer.ContactPersons.Should().HaveCount(1);
    }

    [Fact]
    public void ContactPersons_Collection_ShouldBeImmutableFromOutside()
    {
        // Arrange
        var customer = CreateValidBusiness();
        customer.AddContactPerson("Carlos");

        // Act
        var action = () =>
        {
            var mutableList = (IList<string>)customer.ContactPersons;
            mutableList.Add("Pedro");
        };

        // Assert
        action.Should().Throw<NotSupportedException>();
    }

    // =========================================================================
    // Tax inscriptions
    // =========================================================================

    [Fact]
    public void SetMunicipalInscription_ToBusiness_ShouldSetMunicipalInscription()
    {
        // Arrange
        var customer = CreateValidBusiness();
        var inscription = TaxInscription.Create("12345", TaxInscriptionType.Municipal);

        // Act
        customer.SetMunicipalInscription(inscription);

        // Assert
        customer.MunicipalInscription.Should().Be(inscription);
        customer.StateInscription.Should().BeNull();
    }

    [Fact]
    public void SetStateInscription_ToBusiness_ShouldSetStateInscription()
    {
        // Arrange
        var customer = CreateValidBusiness();
        var inscription = TaxInscription.Create("654321", TaxInscriptionType.State);

        // Act
        customer.SetStateInscription(inscription);

        // Assert
        customer.StateInscription.Should().Be(inscription);
        customer.MunicipalInscription.Should().BeNull();
    }

    [Fact]
    public void SetStateInscription_WhenAlreadyExists_ShouldOverwrite()
    {
        // Arrange
        var customer = CreateValidBusiness();
        var initialInscription = TaxInscription.Create("1111", TaxInscriptionType.State);
        var newInscription = TaxInscription.Create("2222", TaxInscriptionType.State);
        customer.SetStateInscription(initialInscription);

        // Act
        customer.SetStateInscription(newInscription);

        // Assert
        customer.StateInscription.Should().Be(newInscription);
    }

    [Fact]
    public void SetMunicipalInscription_ToIndividual_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateValidIndividual();
        var inscription = TaxInscription.Create("12345", TaxInscriptionType.Municipal);

        // Act
        var action = () => customer.SetMunicipalInscription(inscription);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Inscrições fiscais são permitidas apenas para clientes do tipo Pessoa Jurídica.");
    }

    [Fact]
    public void RemoveMunicipalInscription_ShouldClearMunicipalInscription()
    {
        // Arrange
        var customer = CreateValidBusiness();
        customer.SetMunicipalInscription(TaxInscription.Create("12345", TaxInscriptionType.Municipal));

        // Act
        customer.RemoveMunicipalInscription();

        // Assert
        customer.MunicipalInscription.Should().BeNull();
    }

    [Fact]
    public void RemoveStateInscription_ShouldClearStateInscription()
    {
        // Arrange
        var customer = CreateValidBusiness();
        customer.SetStateInscription(TaxInscription.Create("654321", TaxInscriptionType.State));

        // Act
        customer.RemoveStateInscription();

        // Assert
        customer.StateInscription.Should().BeNull();
    }

    // =========================================================================
    // Address
    // =========================================================================

    [Fact]
    public void UpdateAddress_WithValidAddress_ShouldReplaceCurrentAddress()
    {
        // Arrange
        var customer = CreateValidIndividual();
        var newAddress = Address.Create("87654321", "Av. Paulista", "1000", "Apto 5", "Bela Vista", "São Paulo", "SP");

        // Act
        customer.UpdateAddress(newAddress);

        // Assert
        customer.Address.Should().Be(newAddress);
    }

    [Fact]
    public void UpdateAddress_WithNullAddress_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateValidIndividual();

        // Act
        var action = () => customer.UpdateAddress(null!);

        // Assert
        action.Should().Throw<DomainException>().WithMessage("Endereço é obrigatório.");
    }

    // =========================================================================
    // Activity
    // =========================================================================

    [Fact]
    public void UpdateActivity_WithAnyValue_ShouldReplaceCurrentActivity()
    {
        // Arrange
        var customer = CreateValidBusiness();

        // Act
        customer.UpdateActivity("Comércio Atacadista");

        // Assert
        customer.Activity.Should().Be("Comércio Atacadista");
    }

    [Fact]
    public void UpdateActivity_WithNullValue_ShouldClearActivity()
    {
        // Arrange
        var customer = CreateValidBusiness();
        customer.UpdateActivity("Comércio Atacadista");

        // Act
        customer.UpdateActivity(null);

        // Assert
        customer.Activity.Should().BeNull();
    }

    // =========================================================================
    // Notes
    // =========================================================================

    [Fact]
    public void UpdateNotes_WithBothNotes_ShouldPersistBothValues()
    {
        // Arrange
        var customer = CreateValidBusiness();

        // Act
        customer.UpdateNotes("Nota operacional", "Nota financeira");

        // Assert
        customer.Should().BeEquivalentTo(new
        {
            OperationalNote = "Nota operacional",
            FinancialNote = "Nota financeira"
        });
    }

    [Fact]
    public void UpdateNotes_WithNullValues_ShouldClearNotes()
    {
        // Arrange
        var customer = CreateValidBusiness();
        customer.UpdateNotes("Antiga nota", "Antigo financeiro");

        // Act
        customer.UpdateNotes(null, null);

        // Assert
        customer.OperationalNote.Should().BeNull();
        customer.FinancialNote.Should().BeNull();
    }

    // =========================================================================
    // Paying office
    // =========================================================================

    [Fact]
    public void SetPayingOffice_WithDifferentCustomerId_ShouldAssignPayingOffice()
    {
        // Arrange
        var customer = CreateValidBusiness();
        var headOfficeId = Guid.NewGuid();

        // Act
        customer.SetPayingOffice(headOfficeId);

        // Assert
        customer.PayingOfficeId.Should().Be(headOfficeId);
    }

    [Fact]
    public void SetPayingOffice_WithNull_ShouldClearPayingOffice()
    {
        // Arrange
        var customer = CreateValidBusiness();
        customer.SetPayingOffice(Guid.NewGuid());

        // Act
        customer.SetPayingOffice(null);

        // Assert
        customer.PayingOfficeId.Should().BeNull();
    }

    [Fact]
    public void SetPayingOffice_WithOwnId_ShouldThrowDomainException()
    {
        // Arrange
        var customer = CreateValidBusiness();

        // Act
        var action = () => customer.SetPayingOffice(customer.Id);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("Um cliente não pode ser sua própria matriz.");
    }

    // =========================================================================
    // Status
    // =========================================================================

    [Theory]
    [InlineData(CustomerStatus.Inactive)]
    [InlineData(CustomerStatus.Delinquent)]
    [InlineData(CustomerStatus.Suspended)]
    [InlineData(CustomerStatus.Active)]
    public void SetStatus_WithAnyValidStatus_ShouldUpdateStatus(CustomerStatus newStatus)
    {
        // Arrange
        var customer = CreateValidBusiness();

        // Act
        customer.SetStatus(newStatus);

        // Assert
        customer.Status.Should().Be(newStatus);
    }

    // =========================================================================
    // Auditing / Side-Effects
    // =========================================================================

    [Fact]
    public void ModificationMethods_ShouldUpdateUpdatedAtProperty()
    {
        // Arrange
        var customer = CreateValidIndividual();
        var previousUpdatedAt = customer.UpdatedAt;

        // Wait briefly to ensure clock ticks
        System.Threading.Thread.Sleep(15);

        var newAddress = Address.Create("11111111", "Outra Rua", "20", null, "Bairro", "Cidade", "ST");

        // Act
        customer.UpdateAddress(newAddress);

        // Assert
        customer.UpdatedAt.Should().BeAfter(previousUpdatedAt);
    }
}
