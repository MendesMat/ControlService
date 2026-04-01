using FluentAssertions;
using ControlService.Domain.Commercial.Customers;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.Commercial.Customers.ValueObjects;
using ControlService.Domain.SeedWork;
using Xunit;

namespace ControlService.Domain.Tests.Commercial.Customers;

public class CustomerTests
{
    // -------------------------------------------------------------------------
    // Builders
    // -------------------------------------------------------------------------

    private static Address ValidAddress() =>
        Address.Create("12345678", "Rua das Flores", "100", null, "Centro", "São Paulo", "SP");

    private static Customer ValidIndividual() =>
        new(CustomerType.Individual, "João da Silva", null, null, ValidAddress());

    private static Customer ValidBusiness() =>
        new(CustomerType.Business, "Empresa Ltda", "Nome Fantasia", null, ValidAddress());

    private static Phone MobilePhone(bool isMain = false) =>
        Phone.Create("11999999999", PhoneType.Mobile, isMain);

    private static Email WorkEmail() =>
        Email.Create("contato@empresa.com.br", EmailType.Work);

    // =========================================================================
    // Construction
    // =========================================================================

    [Fact]
    public void Constructor_WithValidIndividual_ShouldSetPropertiesCorrectly()
    {
        var address = ValidAddress();

        var customer = new Customer(CustomerType.Individual, "João da Silva", "Joãozinho", null, address);

        customer.LegalName.Should().Be("João da Silva");
        customer.TradeName.Should().Be("Joãozinho");
        customer.Type.Should().Be(CustomerType.Individual);
        customer.Status.Should().Be(CustomerStatus.Active);
        customer.Address.Should().Be(address);
    }

    [Fact]
    public void Constructor_WithValidBusiness_ShouldSetPropertiesCorrectly()
    {
        var address = ValidAddress();
        var document = Document.Create("27865757000102", DocumentType.CNPJ);

        var customer = new Customer(CustomerType.Business, "Empresa Ltda", "Nome Fantasia", document, address);

        customer.LegalName.Should().Be("Empresa Ltda");
        customer.TradeName.Should().Be("Nome Fantasia");
        customer.Type.Should().Be(CustomerType.Business);
        customer.Document.Should().Be(document);
        customer.Status.Should().Be(CustomerStatus.Active);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyLegalName_ShouldThrowDomainException(string? legalName)
    {
        var action = () => new Customer(CustomerType.Individual, legalName!, null, null, ValidAddress());

        action.Should().Throw<DomainException>().WithMessage("Nome é obrigatório.");
    }

    [Fact]
    public void Constructor_WithNullDocument_ShouldAllowCreation()
    {
        var customer = new Customer(CustomerType.Individual, "João da Silva", null, null, ValidAddress());

        customer.Document.Should().BeNull();
    }

    // =========================================================================
    // Phone management
    // =========================================================================

    [Fact]
    public void AddGeneralPhone_WithNewPhone_ShouldAddToCollection()
    {
        var customer = ValidIndividual();
        var phone = MobilePhone();

        customer.AddGeneralPhone(phone);

        customer.Phones.Should().ContainSingle().Which.Should().Be(phone);
    }

    [Fact]
    public void AddGeneralPhone_WithDuplicatePhone_ShouldNotAddAgain()
    {
        var customer = ValidIndividual();
        var phone = MobilePhone();

        customer.AddGeneralPhone(phone);
        customer.AddGeneralPhone(phone);

        customer.Phones.Should().HaveCount(1);
    }



    [Fact]
    public void RemoveGeneralPhone_WithExistingPhone_ShouldRemoveFromCollection()
    {
        var customer = ValidIndividual();
        var phone = MobilePhone();
        customer.AddGeneralPhone(phone);

        customer.RemoveGeneralPhone(phone);

        customer.Phones.Should().BeEmpty();
    }

    [Fact]
    public void RemoveGeneralPhone_WithNonExistingPhone_ShouldNotThrow()
    {
        var customer = ValidIndividual();
        var phone = MobilePhone();

        var action = () => customer.RemoveGeneralPhone(phone);

        action.Should().NotThrow();
    }

    [Fact]
    public void Phones_Collection_ShouldBeImmutableFromOutside()
    {
        var customer = ValidIndividual();
        customer.AddGeneralPhone(MobilePhone());

        var action = () =>
        {
            var mutableList = (IList<Phone>)customer.Phones;
            mutableList.Add(MobilePhone());
        };

        action.Should().Throw<NotSupportedException>();
    }

    // =========================================================================
    // Email management
    // =========================================================================

    [Fact]
    public void AddGeneralEmail_WithNewEmail_ShouldAddToCollection()
    {
        var customer = ValidBusiness();
        var email = WorkEmail();

        customer.AddGeneralEmail(email);

        customer.Emails.Should().ContainSingle().Which.Should().Be(email);
    }

    [Fact]
    public void AddGeneralEmail_WithDuplicateEmail_ShouldNotAddAgain()
    {
        var customer = ValidBusiness();
        var email = WorkEmail();

        customer.AddGeneralEmail(email);
        customer.AddGeneralEmail(email);

        customer.Emails.Should().HaveCount(1);
    }



    [Fact]
    public void RemoveGeneralEmail_WithExistingEmail_ShouldRemoveFromCollection()
    {
        var customer = ValidBusiness();
        var email = WorkEmail();
        customer.AddGeneralEmail(email);

        customer.RemoveGeneralEmail(email);

        customer.Emails.Should().BeEmpty();
    }

    [Fact]
    public void Emails_Collection_ShouldBeImmutableFromOutside()
    {
        var customer = ValidBusiness();
        customer.AddGeneralEmail(WorkEmail());

        var action = () =>
        {
            var mutableList = (IList<Email>)customer.Emails;
            mutableList.Add(WorkEmail());
        };

        action.Should().Throw<NotSupportedException>();
    }

    // =========================================================================
    // Contact persons management
    // =========================================================================

    [Fact]
    public void AddContactPerson_WithValidName_ShouldAddToCollection()
    {
        var customer = ValidBusiness();

        customer.AddContactPerson("Maria Oliveira");

        customer.ContactPersons.Should().ContainSingle("Maria Oliveira");
    }

    [Fact]
    public void AddContactPerson_IsCaseInsensitiveDuplicate_ShouldNotAddAgain()
    {
        var customer = ValidBusiness();

        customer.AddContactPerson("Maria Oliveira");
        customer.AddContactPerson("MARIA OLIVEIRA");

        customer.ContactPersons.Should().HaveCount(1);
    }

    [Fact]
    public void AddContactPerson_ShouldTrimWhitespace()
    {
        var customer = ValidBusiness();

        customer.AddContactPerson("  Ana Paula  ");

        customer.ContactPersons.Should().ContainSingle("Ana Paula");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddContactPerson_WithBlankName_ShouldNotAddToCollection(string? name)
    {
        var customer = ValidBusiness();

        customer.AddContactPerson(name!);

        customer.ContactPersons.Should().BeEmpty();
    }

    [Fact]
    public void RemoveContactPerson_WithExistingName_ShouldRemoveFromCollection()
    {
        var customer = ValidBusiness();
        customer.AddContactPerson("Carlos");

        customer.RemoveContactPerson("Carlos");

        customer.ContactPersons.Should().BeEmpty();
    }

    [Fact]
    public void RemoveContactPerson_IsCaseInsensitive()
    {
        var customer = ValidBusiness();
        customer.AddContactPerson("Carlos");

        customer.RemoveContactPerson("CARLOS");

        customer.ContactPersons.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RemoveContactPerson_WithBlankName_ShouldNotThrow(string? name)
    {
        var customer = ValidBusiness();
        customer.AddContactPerson("Carlos");

        var action = () => customer.RemoveContactPerson(name!);

        action.Should().NotThrow();
        customer.ContactPersons.Should().HaveCount(1);
    }

    [Fact]
    public void ContactPersons_Collection_ShouldBeImmutableFromOutside()
    {
        var customer = ValidBusiness();
        customer.AddContactPerson("Carlos");

        var action = () =>
        {
            var mutableList = (IList<string>)customer.ContactPersons;
            mutableList.Add("Pedro");
        };

        action.Should().Throw<NotSupportedException>();
    }

    // =========================================================================
    // Tax inscriptions
    // =========================================================================

    [Fact]
    public void AddTaxInscription_MunicipalToBusiness_ShouldSetMunicipalInscription()
    {
        var customer = ValidBusiness();
        var inscription = TaxInscription.Create("12345", TaxInscriptionType.Municipal);

        customer.AddTaxInscription(inscription);

        customer.MunicipalInscription.Should().Be(inscription);
        customer.StateInscription.Should().BeNull();
    }

    [Fact]
    public void AddTaxInscription_StateToBusiness_ShouldSetStateInscription()
    {
        var customer = ValidBusiness();
        var inscription = TaxInscription.Create("654321", TaxInscriptionType.State);

        customer.AddTaxInscription(inscription);

        customer.StateInscription.Should().Be(inscription);
        customer.MunicipalInscription.Should().BeNull();
    }

    [Fact]
    public void AddTaxInscription_ToIndividual_ShouldThrowDomainException()
    {
        var customer = ValidIndividual();
        var inscription = TaxInscription.Create("12345", TaxInscriptionType.Municipal);

        var action = () => customer.AddTaxInscription(inscription);

        action.Should().Throw<DomainException>()
            .WithMessage("Inscrições fiscais são permitidas apenas para clientes do tipo Pessoa Jurídica.");
    }

    [Fact]
    public void RemoveTaxInscription_Municipal_ShouldClearMunicipalInscription()
    {
        var customer = ValidBusiness();
        customer.AddTaxInscription(TaxInscription.Create("12345", TaxInscriptionType.Municipal));

        customer.RemoveTaxInscription(TaxInscriptionType.Municipal);

        customer.MunicipalInscription.Should().BeNull();
    }

    [Fact]
    public void RemoveTaxInscription_State_ShouldClearStateInscription()
    {
        var customer = ValidBusiness();
        customer.AddTaxInscription(TaxInscription.Create("654321", TaxInscriptionType.State));

        customer.RemoveTaxInscription(TaxInscriptionType.State);

        customer.StateInscription.Should().BeNull();
    }

    // =========================================================================
    // Address
    // =========================================================================

    [Fact]
    public void UpdateAddress_WithValidAddress_ShouldReplaceCurrentAddress()
    {
        var customer = ValidIndividual();
        var newAddress = Address.Create("87654321", "Av. Paulista", "1000", "Apto 5", "Bela Vista", "São Paulo", "SP");

        customer.UpdateAddress(newAddress);

        customer.Address.Should().Be(newAddress);
    }

    [Fact]
    public void UpdateAddress_WithNullAddress_ShouldThrowDomainException()
    {
        var customer = ValidIndividual();

        var action = () => customer.UpdateAddress(null!);

        action.Should().Throw<DomainException>().WithMessage("Endereço é obrigatório.");
    }

    // =========================================================================
    // Notes
    // =========================================================================

    [Fact]
    public void UpdateNotes_WithBothNotes_ShouldPersistBothValues()
    {
        var customer = ValidBusiness();

        customer.UpdateNotes("Nota operacional", "Nota financeira");

        customer.OperationalNote.Should().Be("Nota operacional");
        customer.FinancialNote.Should().Be("Nota financeira");
    }

    [Fact]
    public void UpdateNotes_WithNullValues_ShouldClearNotes()
    {
        var customer = ValidBusiness();
        customer.UpdateNotes("Antiga nota", "Antigo financeiro");

        customer.UpdateNotes(null, null);

        customer.OperationalNote.Should().BeNull();
        customer.FinancialNote.Should().BeNull();
    }

    // =========================================================================
    // Paying office
    // =========================================================================

    [Fact]
    public void SetPayingOffice_WithDifferentCustomerId_ShouldAssignPayingOffice()
    {
        var customer = ValidBusiness();
        var headOfficeId = Guid.NewGuid();

        customer.SetPayingOffice(headOfficeId);

        customer.PayingOfficeId.Should().Be(headOfficeId);
    }

    [Fact]
    public void SetPayingOffice_WithNull_ShouldClearPayingOffice()
    {
        var customer = ValidBusiness();
        customer.SetPayingOffice(Guid.NewGuid());

        customer.SetPayingOffice(null);

        customer.PayingOfficeId.Should().BeNull();
    }

    [Fact]
    public void SetPayingOffice_WithOwnId_ShouldThrowDomainException()
    {
        var customer = ValidBusiness();

        var action = () => customer.SetPayingOffice(customer.Id);

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
        var customer = ValidBusiness();

        customer.SetStatus(newStatus);

        customer.Status.Should().Be(newStatus);
    }

    // =========================================================================
    // Validation
    // =========================================================================

    [Fact]
    public void Validate_WithValidCustomer_ShouldReturnSuccess()
    {
        var customer = ValidBusiness();

        var result = customer.Validate();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
