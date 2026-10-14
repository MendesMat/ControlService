using ControlService.Domain.Common;

namespace ControlService.Application.Common;

/// <summary>Errors that any feature may answer, with the Portuguese message of docs/api/conventions.md.</summary>
public static class CommonErrors
{
    public static Error NotFound { get; } = new("not_found", "Registro não encontrado.");
}
