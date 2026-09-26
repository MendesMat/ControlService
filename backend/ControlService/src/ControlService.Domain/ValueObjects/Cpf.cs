using ControlService.Domain.Common;

namespace ControlService.Domain.ValueObjects;

public sealed class Cpf
{
    private Cpf(string value) => Value = value;

    public string Value { get; }

    public static Result<Cpf> Create(string cpf)
    {
        var digits = TextNormalization.ExtractDigits(cpf);

        var isInvalid = digits.Distinct().Count() == 1
            || digits[9] - '0' != CheckDigit(digits[..9], firstWeight: 10)
            || digits[10] - '0' != CheckDigit(digits[..10], firstWeight: 11);

        if (isInvalid)
        {
            return Result<Cpf>.Failure(new Error(
                "validation_failed",
                "Este CPF não é válido. Confira os números ou deixe o campo em branco."));
        }

        return Result<Cpf>.Success(new Cpf(digits));
    }

    private static int CheckDigit(string digits, int firstWeight)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            sum += (digits[i] - '0') * (firstWeight - i);
        }

        var remainder = sum * 10 % 11;
        return remainder == 10 ? 0 : remainder;
    }
}
