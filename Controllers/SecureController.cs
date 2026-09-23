using ExternalIdDemo.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace ExternalIdDemo.Api.Controllers
{
    [ApiController]
    [Route("api/secure")]
    public class SecureController : ControllerBase
    {
        [Authorize(Policy = "MfaRequired")]
        [RequiredScope("access_as_user")]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "You have accessed the secure endpoint.",
                authenticated = true,
                mfa = true
            });
        }
    }
}