using ControlService.Application.Auth;
using ControlService.Application.Auth.GetMe;
using ControlService.Domain.Access;
using ControlService.Domain.PermissionProfiles;

namespace ControlService.Application.Tests.Auth;

public class GetMeTests
{
    [Fact]
    public async Task Me_returns_the_person_and_every_screen_of_the_catalog() // PERM-05, Operations
    {
        var bed = new AuthTestBed();
        var user = bed.AddActiveUser("ana.souza", "senha-da-ana");
        bed.AddProfile(user, (ScreenKeys.Users, AccessLevel.Editor));

        var result = await bed.CreateGetMeHandler()
            .Handle(new GetMeQuery(user.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(user.Id);
        result.Value.DisplayName.ShouldBe("ana.souza");
        result.Value.Login.ShouldBe("ana.souza");
        result.Value.Levels.Select(entry => entry.Screen).ShouldBe(ScreenKeys.All);
        result.Value.Levels.Single(entry => entry.Screen == ScreenKeys.Users).Level.ShouldBe("editor");
        result.Value.Levels.Where(entry => entry.Screen != ScreenKeys.Users).ShouldAllBe(entry => entry.Level == "negado");
    }

    [Fact]
    public async Task Me_without_profiles_is_denied_everywhere() // USR-21, PERM-05
    {
        var bed = new AuthTestBed();
        var user = bed.AddActiveUser("ana.souza", "senha-da-ana");

        var result = await bed.CreateGetMeHandler()
            .Handle(new GetMeQuery(user.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Levels.Count.ShouldBe(ScreenKeys.All.Count);
        result.Value.Levels.ShouldAllBe(entry => entry.Level == "negado");
    }

    [Fact]
    public async Task Me_of_the_admin_is_manager_everywhere() // PERM-06, PERM-23
    {
        var bed = new AuthTestBed();
        var admin = bed.AddAdminWithInitialPassword("senha-inicial");
        bed.Profiles.Add(PermissionProfile.CreateManagerProfile());

        var result = await bed.CreateGetMeHandler()
            .Handle(new GetMeQuery(admin.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Levels.Count.ShouldBe(ScreenKeys.All.Count);
        result.Value.Levels.ShouldAllBe(entry => entry.Level == "gerenciador");
    }

    [Fact]
    public async Task Me_of_a_user_that_no_longer_exists_is_refused_as_session_expired() // AUTH-17
    {
        var bed = new AuthTestBed();

        var result = await bed.CreateGetMeHandler()
            .Handle(new GetMeQuery(Guid.CreateVersion7()), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(AuthErrors.SessionExpired);
    }
}
