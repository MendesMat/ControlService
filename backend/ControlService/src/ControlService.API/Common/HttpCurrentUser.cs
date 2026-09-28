using ControlService.Application.Common;

namespace ControlService.API.Common;

/// <summary>ICurrentUser backed by the request's authenticated principal (ADR-0015, ADR-0019).</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirst("sub");
            return claim is null ? null : Guid.Parse(claim.Value);
        }
    }
}
