using ControlService.Application.Common;
using FluentValidation;

namespace ControlService.API.Common;

public static class ValidatedCommandHandlerRegistration
{
    /// <summary>Registers a command handler wrapped in <see cref="ValidatingCommandHandler{TCommand, TResponse}"/>,
    /// which runs the validator before the handler. The decorator is written here and nowhere else; handler and
    /// validator are built by the container, so their constructor arguments come from it.</summary>
    public static IServiceCollection AddValidatedCommandHandler<TCommand, TResponse, THandler, TValidator>(
        this IServiceCollection services)
        where THandler : class, ICommandHandler<TCommand, TResponse>
        where TValidator : class, IValidator<TCommand> =>
        services.AddScoped<ICommandHandler<TCommand, TResponse>>(serviceProvider =>
            new ValidatingCommandHandler<TCommand, TResponse>(
                ActivatorUtilities.CreateInstance<THandler>(serviceProvider),
                ActivatorUtilities.CreateInstance<TValidator>(serviceProvider)));
}
