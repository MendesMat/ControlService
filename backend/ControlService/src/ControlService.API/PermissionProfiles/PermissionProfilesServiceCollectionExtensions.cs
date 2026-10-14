using ControlService.API.Common;
using ControlService.Application.Common;
using ControlService.Application.PermissionProfiles;
using ControlService.Application.PermissionProfiles.CreateProfile;
using ControlService.Application.PermissionProfiles.DeleteProfile;
using ControlService.Application.PermissionProfiles.GetProfile;
using ControlService.Application.PermissionProfiles.ListProfiles;
using ControlService.Application.PermissionProfiles.UpdateProfile;

namespace ControlService.API.PermissionProfiles;

public static class PermissionProfilesServiceCollectionExtensions
{
    public static IServiceCollection AddPermissionProfilesFeature(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<ListProfilesQuery, IReadOnlyList<ProfileListItemResponse>>, ListProfilesHandler>();
        services.AddScoped<IQueryHandler<GetProfileQuery, ProfileResponse>, GetProfileHandler>();
        services.AddValidatedCommandHandler<CreateProfileCommand, ProfileResponse, CreateProfileHandler, CreateProfileValidator>();
        services.AddValidatedCommandHandler<UpdateProfileCommand, ProfileResponse, UpdateProfileHandler, UpdateProfileValidator>();
        services.AddScoped<ICommandHandler<DeleteProfileCommand, Unit>, DeleteProfileHandler>();

        return services;
    }
}
