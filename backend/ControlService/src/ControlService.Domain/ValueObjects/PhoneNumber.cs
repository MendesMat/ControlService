using ControlService.Domain.Common;

namespace ControlService.Domain.ValueObjects;

public sealed class PhoneNumber
{
    private PhoneNumber(string value) => Value = value;

    public string Value { get; }

    public static Result<PhoneNumber> Create(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        if (digits.Length is < 10 or > 11)
        {
            return Result<PhoneNumber>.Failure(new Error(
                "validation_failed",
                "Digite o telefone com DDD. Ex.: (21) 98765-4321."));
        }

        return Result<PhoneNumber>.Success(new PhoneNumber(digits));
    }
}
