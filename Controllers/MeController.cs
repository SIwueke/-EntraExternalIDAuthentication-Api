using ExternalIdDemo.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace ExternalIdDemo.Api.Controllers
{
    [Authorize]
    [RequiredScope("access_as_user")]
    [MfaRequired]
    [ApiController]
    [Route("api/me")]
    public class MeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Name = User.Identity?.Name,

                Email =
                    User.FindFirst("preferred_username")?.Value
                    ?? User.FindFirst("emails")?.Value,

                ObjectId =
                    User.FindFirst("oid")?.Value,

                TenantId =
                    User.FindFirst("tid")?.Value,

                Subject =
                    User.FindFirst("sub")?.Value,

                IdentityProvider =
                    User.FindFirst("idp")?.Value,

                Claims = User.Claims.Select(c => new
                {
                    c.Type,
                    c.Value
                })
            });
        }
    }
}