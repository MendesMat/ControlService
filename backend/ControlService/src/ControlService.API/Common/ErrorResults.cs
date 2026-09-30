using ControlService.Domain.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ControlService.API.Common;

/// <summary>Translates a domain <see cref="Error"/> into the Problem Details response documented
/// in ADR-0009 and `docs/api/conventions.md#errors` (API-12, API-13).</summary>
public static class ErrorResults
{
    /// <param name="error">The error to translate.</param>
    /// <param name="statusOverride">For the one code whose status depends on the endpoint: `account_inactive`
    /// is a 403 at sign-in and a 401 during a session (ADR-0009).</param>
    public static ProblemHttpResult ToProblem(this Error error, int? statusOverride = null)
    {
        // An unmapped code is a bug: the exception handler logs it and answers with the generic 500 (API-12).
        var status = statusOverride ?? ErrorStatusCodes.For(error.Code)
            ?? throw new InvalidOperationException(
                $"The error code '{error.Code}' has no HTTP status in ErrorStatusCodes (ADR-0009).");
        var (type, title) = ProblemTypes.For(status);

        var problemDetails = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = status,
        };

        problemDetails.Extensions["code"] = error.Code;
        problemDetails.Extensions["message"] = error.Message;

        if (error.Fields is not null)
        {
            problemDetails.Extensions["errors"] = error.Fields;
        }

        if (error.Details is not null)
        {
            problemDetails.Extensions["details"] = error.Details;
        }

        return TypedResults.Problem(problemDetails);
    }
}
