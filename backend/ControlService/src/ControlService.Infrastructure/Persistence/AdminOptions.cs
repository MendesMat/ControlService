namespace ControlService.Infrastructure.Persistence;

/// <summary>The Admin's e-mail comes from configuration, read only when the Admin is first
/// created (USR-24, USR-33, ADR-0022).</summary>
public sealed class AdminOptions
{
    public const string SectionName = "Admin";

    public required string Email { get; init; }
}
