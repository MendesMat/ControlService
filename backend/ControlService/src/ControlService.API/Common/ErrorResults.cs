using ControlService.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ControlService.API.Common;

/// <summary>Translates a domain <see cref="Error"/> into the Problem Details response documented
/// in ADR-0009 and `docs/api/conventions.md#errors` (API-12, API-13).</summary>
public static class ErrorResults
{
    public static IResult ToProblem(this Error error, HttpContext httpContext)
    {
        var status = ErrorStatusCodes.For(error.Code) ?? StatusCodes.Status500InternalServerError;
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

        return Results.Problem(problemDetails);
    }
}
