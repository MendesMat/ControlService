using ControlService.Domain.Common;
using FluentValidation;

namespace ControlService.Application.Common;

/// <summary>Validates a command before it reaches its handler (ADR-0008).</summary>
public sealed class ValidatingCommandHandler<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> innerHandler,
    IValidator<TCommand> validator) : ICommandHandler<TCommand, TResponse>
{
    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);

        if (!validation.IsValid)
        {
            var fields = validation.Errors
                .GroupBy(failure => ToCamelCasePath(failure.PropertyName))
                .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray());

            return Result<TResponse>.Failure(
                new Error("validation_failed", "Alguns campos precisam ser corrigidos.", Fields: fields));
        }

        return await innerHandler.Handle(command, cancellationToken);
    }

    private static string ToCamelCasePath(string propertyPath) =>
        string.Join('.', propertyPath.Split('.').Select(ToCamelCase));

    private static string ToCamelCase(string segment) =>
        segment.Length == 0 ? segment : string.Concat(char.ToLowerInvariant(segment[0]), segment[1..]);
}
