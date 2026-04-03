using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.Commercial.Customers.ValueObjects;
using ControlService.Domain.SeedWork;

namespace ControlService.Domain.Commercial.Customers;

public class Customer : Entity, IAggregateRoot
{
    public CustomerType Type { get; private set; }
    public string LegalName { get; private set; }
    public string? TradeName { get; private set; }
    public Document? Document { get; private set; }
    public TaxInscription? MunicipalInscription { get; private set; }
    public TaxInscription? StateInscription { get; private set; }
    public Address Address { get; private set; }
    public string? Activity { get; private set; }
    public string? OperationalNote { get; private set; }
    public string? FinancialNote { get; private set; }
    public Guid? PayingOfficeId { get; private set; }
    public CustomerStatus Status { get; private set; }

    private readonly List<Phone> _phones = new();
    public IReadOnlyCollection<Phone> Phones => _phones.AsReadOnly();

    private readonly List<Email> _emails = new();
    public IReadOnlyCollection<Email> Emails => _emails.AsReadOnly();

    private readonly List<string> _contactPersons = new();
    public IReadOnlyCollection<string> ContactPersons => _contactPersons.AsReadOnly();

    private Customer() { }

    public Customer(CustomerType type, string legalName, string? tradeName, Document? document, Address address)
        : base()
    {
        if (string.IsNullOrWhiteSpace(legalName))
            throw new DomainException("Nome é obrigatório.");

        Type = type;
        LegalName = legalName;
        TradeName = tradeName;
        Document = document;
        Address = address;
        Status = CustomerStatus.Active;
    }

    public void AddGeneralPhone(Phone phone)
    {
        if (!_phones.Any(p => p == phone))
            _phones.Add(phone);
    }

    public void RemoveGeneralPhone(Phone phone) => _phones.RemoveAll(p => p == phone);

    public void AddGeneralEmail(Email email)
    {
        if (!_emails.Any(e => e == email))
            _emails.Add(email);
    }

    public void RemoveGeneralEmail(Email email) => _emails.RemoveAll(e => e == email);

    public void AddContactPerson(string person)
    {
        if (string.IsNullOrWhiteSpace(person)) return;

        var trimmed = person.Trim();
        if (!_contactPersons.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            _contactPersons.Add(trimmed);
    }

    public void RemoveContactPerson(string person)
    {
        if (string.IsNullOrWhiteSpace(person)) return;
        _contactPersons.RemoveAll(p => p.Equals(person.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public void SetMunicipalInscription(TaxInscription inscription)
    {
        EnsureIsBusinessCustomer();
        MunicipalInscription = inscription;
    }

    public void RemoveMunicipalInscription() => MunicipalInscription = null;

    public void SetStateInscription(TaxInscription inscription)
    {
        EnsureIsBusinessCustomer();
        StateInscription = inscription;
    }

    public void RemoveStateInscription() => StateInscription = null;

    private void EnsureIsBusinessCustomer()
    {
        if (Type != CustomerType.Business)
            throw new DomainException("Inscrições fiscais são permitidas apenas para clientes do tipo Pessoa Jurídica.");
    }

    public void UpdateAddress(Address address)
    {
        Address = address ?? throw new DomainException("Endereço é obrigatório.");
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateActivity(string? activity)
    {
        Activity = activity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string? operational, string? financial)
    {
        OperationalNote = operational;
        FinancialNote = financial;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPayingOffice(Guid? payingOfficeId)
    {
        if (payingOfficeId.HasValue && payingOfficeId.Value == Id)
            throw new DomainException("Um cliente não pode ser sua própria matriz.");

        PayingOfficeId = payingOfficeId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(CustomerStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
