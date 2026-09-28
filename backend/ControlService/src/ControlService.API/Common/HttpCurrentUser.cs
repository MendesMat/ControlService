using ControlService.Application.Common;
using ControlService.Domain.Common;

namespace ControlService.API.Common;

/// <summary>ICurrentUser backed by the request's authenticated principal (ADR-0015, ADR-0019).
/// Outside a request (startup, seeding), there is no HttpContext at all, and the system acts
/// as the Admin (CNV-20).</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return SystemIds.AdminUser;
            }

            var claim = httpContext.User.FindFirst("sub");
            return claim is null ? null : Guid.Parse(claim.Value);
        }
    }
}
