namespace ControlService.Application.Auth;

public enum CredentialCheckOutcome
{
    Succeeded,
    Failed,
    LockedOut,
}

/// <summary>The result of checking a password against the person's credential, lockout included
/// (AUTH-08). A login with no credential fails like a wrong password.</summary>
public sealed record CredentialCheck(CredentialCheckOutcome Outcome, DateTimeOffset? LockoutEnd = null)
{
    public static CredentialCheck Succeeded { get; } = new(CredentialCheckOutcome.Succeeded);

    public static CredentialCheck Failed { get; } = new(CredentialCheckOutcome.Failed);

    public static CredentialCheck LockedOut(DateTimeOffset lockoutEnd) => new(CredentialCheckOutcome.LockedOut, lockoutEnd);
}
