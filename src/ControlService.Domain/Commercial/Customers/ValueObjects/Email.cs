using System.Text.RegularExpressions;
using ControlService.Domain.Commercial.Customers.Enums;
using ControlService.Domain.SeedWork;

namespace ControlService.Domain.Commercial.Customers.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; }
    public EmailType Type { get; }
    public bool IsMainEmail { get; }

    private Email(string value, EmailType type, bool isMain)
    {
        Value = value;
        Type = type;
        IsMainEmail = isMain;
    }

    public static Email Create(string value, EmailType type, bool isMain = false)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty.");

        var normalized = value.Trim().ToLowerInvariant();

        if (!IsValid(normalized))
            throw new DomainException("Invalid email format.");

        return new Email(normalized, type, isMain);
    }

    private static bool IsValid(string email)
    {
        // Simple regex for basic validation complying with TDD scope
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Type;
        yield return IsMainEmail;
    }
}
