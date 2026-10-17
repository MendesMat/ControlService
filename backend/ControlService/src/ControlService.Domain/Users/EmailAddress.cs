using System.Text.RegularExpressions;
using ControlService.Domain.Common;

namespace ControlService.Domain.Users;

public sealed partial class EmailAddress
{
    private EmailAddress(string value) => Value = value;

    public string Value { get; }

    public static Result<EmailAddress> Create(string email)
    {
        // The e-mail is optional (USR-07): blank means "no e-mail".
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<EmailAddress>.Success(new EmailAddress(string.Empty));
        }

        if (!ValidFormat().IsMatch(email))
        {
            return Result<EmailAddress>.Failure(new Error(
                "validation_failed",
                "Este e-mail não parece válido. Confira se ele tem @ e o domínio, por exemplo ana@empresa.com.br."));
        }

        return Result<EmailAddress>.Success(new EmailAddress(email));
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex ValidFormat();
}
