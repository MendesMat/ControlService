using System.Security.Claims;
using ControlService.API.Common;
using ControlService.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace ControlService.Api.IntegrationTests.Common;

public class HttpCurrentUserTests
{
    [Fact]
    public void Current_user_is_read_from_the_sub_claim() // ADR-0015, ADR-0019
    {
        var userId = Guid.CreateVersion7();
        var identity = new ClaimsIdentity([new Claim("sub", userId.ToString())], authenticationType: "Test");
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };

        var currentUser = new HttpCurrentUser(accessor);

        currentUser.UserId.ShouldBe(userId);
    }

    [Fact]
    public void Request_without_a_signed_in_user_has_no_current_user() // CNV-20
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };

        var currentUser = new HttpCurrentUser(accessor);

        currentUser.UserId.ShouldBeNull();
    }

    [Fact]
    public void Outside_a_request_the_system_acts_as_the_admin() // CNV-20
    {
        var accessor = new HttpContextAccessor { HttpContext = null };

        var currentUser = new HttpCurrentUser(accessor);

        currentUser.UserId.ShouldBe(SystemIds.AdminUser);
    }
}
