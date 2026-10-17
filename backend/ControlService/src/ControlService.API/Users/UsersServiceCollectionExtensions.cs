using ControlService.API.Common;
using ControlService.Application.Common;
using ControlService.Application.Users;
using ControlService.Application.Users.CreateUser;
using ControlService.Application.Users.GetUser;
using ControlService.Application.Users.SuggestLogin;

namespace ControlService.API.Users;

public static class UsersServiceCollectionExtensions
{
    public static IServiceCollection AddUsersFeature(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetUserQuery, UserResponse>, GetUserHandler>();
        services.AddScoped<IQueryHandler<SuggestLoginQuery, SuggestLoginResponse>, SuggestLoginHandler>();

        services.AddValidatedCommandHandler<CreateUserCommand, UserResponse, CreateUserHandler, CreateUserValidator>();

        return services;
    }
}
