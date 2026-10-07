using System.Diagnostics.CodeAnalysis;

namespace ControlService.Domain.Common;

[SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "The type name is Error, as in the domain glossary.")]
public sealed record Error(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Fields = null,
    IReadOnlyDictionary<string, object?>? Details = null);
