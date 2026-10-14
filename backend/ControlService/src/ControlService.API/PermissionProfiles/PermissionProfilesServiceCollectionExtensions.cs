using ControlService.Application.Common;
using ControlService.Application.PermissionProfiles;
using ControlService.Application.PermissionProfiles.CreateProfile;
using ControlService.Application.PermissionProfiles.GetProfile;
using ControlService.Application.PermissionProfiles.ListProfiles;

namespace ControlService.API.PermissionProfiles;

public static class PermissionProfilesServiceCollectionExtensions
{
    public static IServiceCollection AddPermissionProfilesFeature(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<ListProfilesQuery, IReadOnlyList<ProfileListItemResponse>>, ListProfilesHandler>();
        services.AddScoped<IQueryHandler<GetProfileQuery, ProfileResponse>, GetProfileHandler>();
        services.AddScoped<ICommandHandler<CreateProfileCommand, ProfileResponse>, CreateProfileHandler>();

        return services;
    }
}
