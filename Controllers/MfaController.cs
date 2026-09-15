using ExternalIdDemo.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExternalIdDemo.Api.Controllers;

[ApiController]
[Route("api/mfa")]
[Authorize]
public class MfaController : ControllerBase
{
    private readonly IMfaService _mfaService;
    private readonly IMfaSessionStore _mfaSessionStore;

    public MfaController(IMfaService mfaService, IMfaSessionStore mfaSessionStore)
    {
        _mfaService = mfaService;
        _mfaSessionStore = mfaSessionStore;
    }
    //temp
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var claims = User.Claims
            .Select(c => new
            {
                c.Type,
                c.Value
            })
            .ToList();

        var entraObjectId = GetEntraObjectId();

        if (entraObjectId is null)
        {
            return Unauthorized(new
            {
                message = "Entra object ID (oid) was not found.",
                claims
            });
        }

        var result =
            await _mfaService.GetStatusAsync(
                entraObjectId);

        return Ok(result);
    }
    //[HttpGet("status")]
    //public async Task<IActionResult> GetStatus()
    //{
    //    var entraObjectId = GetEntraObjectId();

    //    if (entraObjectId is null)
    //    {
    //        return Unauthorized(
    //            "Entra object ID (oid) was not found.");
    //    }

    //    var result =
    //        await _mfaService.GetStatusAsync(
    //            entraObjectId);

    //    return Ok(result);
    //}

    [HttpPost("enrol")]
    public async Task<IActionResult> Enrol()
    {
        var entraObjectId = GetEntraObjectId();

        if (entraObjectId is null)
        {
            return Unauthorized(
                "Entra object ID (oid) was not found.");
        }

        var username =
            User.FindFirstValue(
                "preferred_username")
            ?? User.FindFirstValue(
                ClaimTypes.Email)
            ?? entraObjectId;

        try
        {
            var result =
                await _mfaService.StartEnrollmentAsync(
                    entraObjectId,
                    username);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("enrol/verify")]
    public async Task<IActionResult> VerifyEnrollment(
        [FromBody] VerifyMfaRequest request)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(
                "A six-digit verification code is required.");
        }

        var entraObjectId = GetEntraObjectId();

        if (entraObjectId is null)
        {
            return Unauthorized(
                "Entra object ID (oid) was not found.");
        }

        var valid =
            await _mfaService.VerifyEnrollmentAsync(
                entraObjectId,
                request.Code);

        if (!valid)
        {
            return BadRequest(
                "The verification code is invalid or the enrollment has expired.");
        }

        return Ok(new
        {
            success = true,
            message = "TOTP MFA enrollment completed successfully."
        });
    }

    private string? GetEntraObjectId()
    {
        // Raw JWT claim
        var objectId = User.FindFirstValue("oid");

        if (!string.IsNullOrWhiteSpace(objectId))
        {
            return objectId;
        }

        // Microsoft identity claim mapping
        objectId = User.FindFirstValue(
            "http://schemas.microsoft.com/identity/claims/objectidentifier");

        if (!string.IsNullOrWhiteSpace(objectId))
        {
            return objectId;
        }

        // ClaimTypes.NameIdentifier fallback
        objectId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return objectId;
    }


    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyMfaRequest request)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(
                "A six-digit verification code is required.");
        }

        var entraObjectId = GetEntraObjectId();

        if (entraObjectId is null)
        {
            return Unauthorized(
                "Entra object ID (oid) was not found.");
        }

        var valid =
            await _mfaService.VerifyAsync(
                entraObjectId,
                request.Code);

        if (!valid)
        {
            return Unauthorized(
                "The MFA verification code is invalid.");
        }

        // =========================================================
        // MFA SUCCESSFUL
        // Create an application MFA session
        // =========================================================

        var sessionId =  Guid.NewGuid().ToString("N");

        await _mfaSessionStore.CreateAsync(sessionId,  entraObjectId,  TimeSpan.FromMinutes(30));

        // =========================================================
        // Store the MFA session ID in an HttpOnly cookie
        // =========================================================

        Response.Cookies.Append(
            "mfa_session",
            sessionId,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                MaxAge = TimeSpan.FromMinutes(30)
            });

        return Ok(new
        {
            success = true,
            message = "MFA verification successful."
        });
    }
}
public class VerifyMfaRequest
{
    public string Code { get; set; } = string.Empty;
}