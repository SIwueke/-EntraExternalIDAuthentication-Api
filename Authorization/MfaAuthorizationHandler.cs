using ExternalIdDemo.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ExternalIdDemo.Api.Authorization;

public class MfaAuthorizationHandler
    : AuthorizationHandler<MfaRequirement>
{
    private readonly IMfaSessionStore _sessionStore;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MfaAuthorizationHandler(
        IMfaSessionStore sessionStore,
        IHttpContextAccessor httpContextAccessor)
    {
        _sessionStore = sessionStore;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MfaRequirement requirement)
    {
        var httpContext =
            _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return;
        }

        var entraObjectId =
            context.User.FindFirstValue("oid")
            ?? context.User.FindFirstValue(
                "http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?? context.User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            return;
        }

        if (!httpContext.Request.Cookies.TryGetValue(
                "mfa_session",
                out var sessionId))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return;
        }

        var valid =
            await _sessionStore.IsValidAsync(
                sessionId,
                entraObjectId);

        if (valid)
        {
            context.Succeed(requirement);
        }
    }
}