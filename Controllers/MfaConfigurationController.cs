using Microsoft.AspNetCore.Mvc;

namespace ExternalIdDemo.Api.Controllers;

[ApiController]
[Route("api/mfa/configuration")]
public class MfaConfigurationController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public MfaConfigurationController(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var mode =
            _configuration["Mfa:Mode"] ?? "Existing";

        return Ok(new
        {
            mode
        });
    }
}