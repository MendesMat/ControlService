using ControlService.Domain.Common;

namespace ControlService.Application.Users;

/// <summary>The errors of the user operations, with the messages verbatim from docs/product/features/users.md.</summary>
public static class UserErrors
{
    /// <summary>A login or display name that another user already has (USR-03, USR-06). They are validation errors
    /// like any other, each under its field (decision 7, API-13). A field that is not taken is not in the error.</summary>
    public static Error Duplicates(string? takenLogin, string? takenDisplayName)
    {
        var fields = new Dictionary<string, string[]>();
        if (takenLogin is not null)
        {
            fields["login"] = [$"Já existe um usuário com o login “{takenLogin}”. Escolha outro."];
        }

        if (takenDisplayName is not null)
        {
            fields["displayName"] =
                [$"Já existe um usuário chamado “{takenDisplayName}”. Use um nome diferente, por exemplo acrescentando o sobrenome ou o setor."];
        }

        return new Error("validation_failed", "Alguns campos precisam ser corrigidos.", Fields: fields);
    }
}
