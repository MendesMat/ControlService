using ControlService.Domain.Common;

namespace ControlService.Domain.ValueObjects;

public sealed class BloodType
{
    private static readonly string[] ClosedList = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];

    private BloodType(string value) => Value = value;

    public string Value { get; }

    public static Result<BloodType> Create(string bloodType)
    {
        if (!ClosedList.Contains(bloodType))
        {
            return Result<BloodType>.Failure(new Error("validation_failed", "Este tipo sanguíneo não é válido."));
        }

        return Result<BloodType>.Success(new BloodType(bloodType));
    }
}
