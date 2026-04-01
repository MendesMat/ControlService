using ControlService.Domain.SeedWork;

namespace ControlService.Domain.Commercial.Customers.ValueObjects;

public class ValidationResult : ValueObject
{
    public bool IsValid { get; }
    public IReadOnlyCollection<string> Errors { get; }

    private ValidationResult(bool isValid, IEnumerable<string> errors)
    {
        IsValid = isValid;
        Errors = errors.ToList().AsReadOnly();
    }

    public static ValidationResult Success()
    {
        return new ValidationResult(true, new List<string>());
    }

    public static ValidationResult Failure(params string[] errors)
    {
        return new ValidationResult(false, errors);
    }

    public static ValidationResult Failure(IEnumerable<string> errors)
    {
        return new ValidationResult(false, errors);
    }

    public ValidationResult Combine(ValidationResult other)
    {
        if (IsValid && other.IsValid)
            return Success();

        var combinedErrors = Errors.Concat(other.Errors);
        return Failure(combinedErrors);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IsValid;
        foreach (var error in Errors)
        {
            yield return error;
        }
    }
}
