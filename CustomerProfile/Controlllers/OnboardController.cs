using System.Security.Claims;
using CustomerProfile.DTO;
using CustomerProfile.JwtTokenService;
using CustomerProfile.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerProfile.Controlllers;

[ApiController]
[Route("api/[controller]")]
public class OnboardController(OnboardService onboardService) : ControllerBase
{
    private readonly OnboardService _onboardService = onboardService;

    [HttpPost("initiate")]
    public async Task<IActionResult> InitateOnboard(
        [FromBody] OnboardingRequest request,
        CancellationToken ct
    )
    {
        var response = await _onboardService.InitiateOnboard(request, ct);

        return response.IsSuccess ? Ok(response.Data) : BadRequest(response.ErrorMessage);
    }

    [Authorize]
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(OtpVerifyRequestBody request, CancellationToken ct)
    {
        var verfifiactionIdClaim = User.FindFirst(ClaimTypes.Sid)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (role != RolesUtils.VerificationRole)
            return BadRequest("Invalid Otp");

        if (!Guid.TryParse(verfifiactionIdClaim, out Guid id))
            return BadRequest("Invalid Otp");

        var response = await _onboardService.VerifyOtpAsync(id, request, ct);

        return response.IsSuccess ? Ok(response.Data) : BadRequest(response.ErrorMessage);
    }

    [Authorize]
    [HttpPost("set-pin")]
    public async Task<IActionResult> SetSixDigitPin(
        SetSixDigitPinRequest request,
        CancellationToken ct
    )
    {
        var verfifiactionIdClaim = User.FindFirst(ClaimTypes.Sid)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (role != RolesUtils.VerificationRole)
            return BadRequest("Invalid Otp");

        if (!Guid.TryParse(verfifiactionIdClaim, out Guid id))
            return BadRequest("Invalid Otp");

        var response = await _onboardService.SetSixDigitPinAsync(id, request, ct);

        return response.IsSuccess ? Ok(response.Data) : BadRequest(response.ErrorMessage);
    }
}
