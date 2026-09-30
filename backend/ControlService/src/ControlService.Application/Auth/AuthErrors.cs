using ControlService.Domain.Common;

namespace ControlService.Application.Auth;

/// <summary>The authentication errors and their Portuguese messages, verbatim from
/// docs/product/features/authentication.md (ADR-0009).</summary>
public static class AuthErrors
{
    public static Error InvalidCredentials { get; } = new("invalid_credentials", "Login ou senha incorretos.");
}
