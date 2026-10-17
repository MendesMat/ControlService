using ControlService.Application.Common;
using ControlService.Application.Users.SuggestLogin;

namespace ControlService.API.Users;

public static class UsersServiceCollectionExtensions
{
    public static IServiceCollection AddUsersFeature(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<SuggestLoginQuery, SuggestLoginResponse>, SuggestLoginHandler>();

        return services;
    }
}
