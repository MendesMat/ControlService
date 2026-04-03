using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ControlService.Domain.SeedWork;

namespace ControlService.API.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, extensions) = MapException(exception);

        httpContext.Response.StatusCode = status;

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = exception.Message
        };

        if (extensions is not null)
            foreach (var (key, value) in extensions)
                problemDetails.Extensions[key] = value;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    private static (int status, string title, Dictionary<string, object?>? extensions) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => MapValidationException(validationException),
            DomainException => (StatusCodes.Status400BadRequest, "Regra de negócio violada.", null),
            EntityNotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado.", null),
            _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado.", null)
        };
    }

    private static (int, string, Dictionary<string, object?>) MapValidationException(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => (object?)g.Select(e => e.ErrorMessage).ToArray());

        return (StatusCodes.Status422UnprocessableEntity, "Dados de entrada inválidos.", new Dictionary<string, object?> { ["errors"] = errors });
    }
}
