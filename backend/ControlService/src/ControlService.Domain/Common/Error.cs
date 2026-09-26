using System.Diagnostics.CodeAnalysis;

namespace ControlService.Domain.Common;

[SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "The type name is Error by decision of ADR-0009 and the domain glossary.")]
public sealed record Error(string Code, string Message);
