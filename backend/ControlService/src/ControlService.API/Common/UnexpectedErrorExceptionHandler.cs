using ControlService.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace ControlService.API.Common;

/// <summary>Translates an unhandled exception into the generic 500 response documented in
/// API-12: the real cause never reaches the client, only the logs.</summary>
public sealed class UnexpectedErrorExceptionHandler : IExceptionHandler
{
    private static readonly Error UnexpectedError = new(
        "unexpected_error",
        "Não foi possível concluir a operação. Tente de novo em alguns minutos.");

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        await UnexpectedError.ToProblem(httpContext).ExecuteAsync(httpContext);
        return true;
    }
}
