using ControlService.Application.Common;
using ControlService.Application.Users;
using ControlService.Application.Users.CreateUser;
using ControlService.Application.Users.SuggestLogin;

namespace ControlService.API.Users;

public static class UsersServiceCollectionExtensions
{
    public static IServiceCollection AddUsersFeature(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<SuggestLoginQuery, SuggestLoginResponse>, SuggestLoginHandler>();

        services.AddScoped<ICommandHandler<CreateUserCommand, UserResponse>, CreateUserHandler>();

        return services;
    }
}
