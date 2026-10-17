using ControlService.API.Auth;
using ControlService.API.Common;
using ControlService.Application.Common;
using ControlService.Application.Users;
using ControlService.Application.Users.CreateUser;
using ControlService.Application.Users.SuggestLogin;
using ControlService.Domain.Access;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ControlService.API.Users;

public static class UsersEndpoints
{
    private const string UsersRoute = "/users";

    public static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder api)
    {
        api.MapPost($"{UsersRoute}/login-suggestion", SuggestLogin)
            .RequireScreenAccess(ScreenKeys.Users, AccessLevel.Editor)
            .WithSummary("Suggests a login from a full name; the name travels in the body, never in the URL.");

        api.MapPost(UsersRoute, CreateUser)
            .RequireScreenAccess(ScreenKeys.Users, AccessLevel.Editor)
            .WithSummary("Registers a user with a temporary password the person must replace at the first sign-in.");

        return api;
    }

    private static async Task<Results<Ok<SuggestLoginResponse>, ProblemHttpResult>> SuggestLogin(
        SuggestLoginRequest request,
        IQueryHandler<SuggestLoginQuery, SuggestLoginResponse> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new SuggestLoginQuery(request.FullName ?? string.Empty), cancellationToken);
        return result.IsFailure ? result.Error.ToProblem() : TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Created<UserResponse>, ProblemHttpResult>> CreateUser(
        CreateUserRequest request,
        ICommandHandler<CreateUserCommand, UserResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.ToCommand(), cancellationToken);
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        httpContext.Response.WriteETag(result.Value.Version);
        return TypedResults.Created($"{ApiV1Group.Prefix}{UsersRoute}/{result.Value.Id}", result.Value);
    }

    // A missing or null field is treated as empty, so it fails validation instead of crashing (API-12).
    // `id`, `status`, `isSystem` and the audit fields are not read: the server decides them.
    private sealed record SuggestLoginRequest(string? FullName = null);

    private sealed record CreateUserRequest(
        string? Login = null,
        string? Email = null,
        string? FullName = null,
        string? DisplayName = null,
        string? Cpf = null,
        string? Rg = null,
        string? BirthDate = null,
        string? BloodType = null,
        string? Phone = null,
        AddressRequest? Address = null,
        EmergencyContactRequest? EmergencyContact = null,
        IReadOnlyList<Guid>? ProfileIds = null,
        string? TemporaryPassword = null)
    {
        public CreateUserCommand ToCommand() => new(
            Login ?? string.Empty,
            Email ?? string.Empty,
            FullName ?? string.Empty,
            DisplayName ?? string.Empty,
            Cpf ?? string.Empty,
            Rg ?? string.Empty,
            BirthDate ?? string.Empty,
            BloodType ?? string.Empty,
            Phone ?? string.Empty,
            new AddressInput(
                Address?.Cep ?? string.Empty,
                Address?.Street ?? string.Empty,
                Address?.Number ?? string.Empty,
                Address?.Complement ?? string.Empty,
                Address?.District ?? string.Empty,
                Address?.City ?? string.Empty),
            new EmergencyContactInput(
                EmergencyContact?.Name ?? string.Empty,
                EmergencyContact?.Relationship ?? string.Empty,
                EmergencyContact?.Phone ?? string.Empty),
            ProfileIds ?? [],
            TemporaryPassword ?? string.Empty);
    }

    private sealed record AddressRequest(
        string? Cep = null, string? Street = null, string? Number = null, string? Complement = null, string? District = null, string? City = null);

    private sealed record EmergencyContactRequest(string? Name = null, string? Relationship = null, string? Phone = null);
}
