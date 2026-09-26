using ControlService.Domain.Common;

namespace ControlService.Domain.ValueObjects;

public sealed class Cep
{
    private Cep(string value) => Value = value;

    public string Value { get; }

    public static Result<Cep> Create(string cep)
    {
        var digits = TextNormalization.ExtractDigits(cep);

        if (digits.Length != 8)
        {
            return Result<Cep>.Failure(new Error("validation_failed", "O CEP precisa ter 8 números."));
        }

        return Result<Cep>.Success(new Cep(digits));
    }
}
