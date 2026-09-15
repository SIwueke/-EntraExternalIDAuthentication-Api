using System.Security.Claims;
using ExternalIdDemo.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ExternalIdDemo.Api.Authorization;

public class MfaAuthorizationHandler
    : AuthorizationHandler<MfaRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMfaSessionStore _mfaSessionStore;

    public MfaAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor,
        IMfaSessionStore mfaSessionStore)
    {
        _httpContextAccessor = httpContextAccessor;
        _mfaSessionStore = mfaSessionStore;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,  MfaRequirement requirement)
    {
        var httpContext =   _httpContextAccessor.HttpContext;

        var entraObjectId =  context.User.FindFirst("oid")?.Value
            ?? context.User.FindFirst(
                "http://schemas.microsoft.com/identity/claims/objectidentifier"
            )?.Value
            ?? context.User.FindFirst(
                ClaimTypes.NameIdentifier
            )?.Value;

        var sessionId =   httpContext?.Request.Cookies["mfa_session"];

        if (
            string.IsNullOrWhiteSpace(entraObjectId) ||
            string.IsNullOrWhiteSpace(sessionId))
        {
            return;
        }

        var valid =
            await _mfaSessionStore.IsValidAsync(
                sessionId,
                entraObjectId);

        if (valid)
        {
            context.Succeed(requirement);
        }
    }
}