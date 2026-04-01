namespace ControlService.Domain.Commercial.Customers.Enums;

public enum CustomerType
{
    Individual = 1,
    Business = 2
}

public enum DocumentType
{
    CPF = 1,
    CNPJ = 2
}

public enum CustomerStatus
{
    Active = 1,
    Inactive = 2,
    Delinquent = 3,
    Suspended = 4
}

public enum TaxInscriptionType
{
    Municipal = 1,
    State = 2
}

public enum PhoneType
{
    Mobile = 1,
    Landline = 2,
    Fax = 3,
    WhatsApp = 4
}

public enum EmailType
{
    Personal = 1,
    Work = 2,
    Billing = 3,
    Support = 4
}
