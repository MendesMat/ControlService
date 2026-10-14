using ControlService.Application.Common;
using ControlService.Application.PermissionProfiles.ListProfiles;

namespace ControlService.API.PermissionProfiles;

public static class PermissionProfilesServiceCollectionExtensions
{
    public static IServiceCollection AddPermissionProfilesFeature(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<ListProfilesQuery, IReadOnlyList<ProfileListItemResponse>>, ListProfilesHandler>();

        return services;
    }
}
