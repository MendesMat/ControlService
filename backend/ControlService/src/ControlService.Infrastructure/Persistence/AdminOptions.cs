namespace ControlService.Infrastructure.Persistence;

/// <summary>The Admin's e-mail and initial password come from configuration, each read only when
/// the record it creates is missing (USR-24, USR-33, AUTH-13).</summary>
public sealed class AdminOptions
{
    public const string SectionName = "Admin";

    public required string Email { get; init; }

    /// <summary>Never logged, echoed or committed (AUTH-13): it lives in user secrets.</summary>
    public required string InitialPassword { get; init; }
}
