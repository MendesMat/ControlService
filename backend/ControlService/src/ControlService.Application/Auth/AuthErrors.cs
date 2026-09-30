using ControlService.Domain.Common;

namespace ControlService.Application.Auth;

/// <summary>The authentication errors and their Portuguese messages, verbatim from
/// docs/product/features/authentication.md (ADR-0009).</summary>
public static class AuthErrors
{
    public static Error InvalidCredentials { get; } = new("invalid_credentials", "Login ou senha incorretos.");

    public static Error SessionExpired { get; } = new(
        "session_expired", "Sua sessão terminou. Entre de novo para continuar. Suas abas continuam abertas.");

    public static Error AccountInactive { get; } = new(
        "account_inactive", "Este acesso está desativado. Fale com o responsável pelo sistema.");

    public static Error PasswordAlreadyCreated { get; } = new(
        "forbidden", "Sua senha já foi criada. Para trocá-la, use “Esqueci minha senha”.");

    public static Error NewPasswordEqualsInitial { get; } = new(
        "validation_failed",
        "Alguns campos precisam ser corrigidos.",
        Fields: new Dictionary<string, string[]> { ["password"] = ["A nova senha precisa ser diferente da senha inicial."] });

    public static Error LockedOut(int lockoutMinutes, int retryAfterSeconds) => new(
        "locked_out",
        $"Muitas tentativas sem sucesso. Aguarde {lockoutMinutes} minutos e tente de novo.",
        Details: new Dictionary<string, object?> { ["retryAfterSeconds"] = retryAfterSeconds });
}
