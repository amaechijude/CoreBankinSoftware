using System.Security.Claims;
using CustomerProfile.DTO;
using CustomerProfile.JwtTokenService;
using CustomerProfile.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerProfile.Controlllers;

[Route("api/[controller]/register")]
[ApiController]
public sealed class AuthController(
    OnboardService _onboardingCommandHandler,
    AuthService _authService
) : ControllerBase
{
    [HttpPost("send-otp")]
    public async Task<IActionResult> OnboardCustomer(
        [FromBody] OnboardingRequest request,
        CancellationToken ct
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _onboardingCommandHandler.InitiateOnboard(request, ct);

        if (result.IsSuccess && result.Data is not null)
        {
            return Ok(result.Data);
        }

        return BadRequest(result.ErrorMessage);
    }

    [Authorize]
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyRegistrationOtpAsync(
        [FromBody] OtpVerifyRequestBody request,
        CancellationToken ct
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        // Get claims from the current user
        var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        bool IsVAlidGuid = Guid.TryParse(userId, out var validId);
        if (!IsVAlidGuid || userRole != RolesUtils.VerificationRole)
        {
            return BadRequest();
        }

        var result = await _onboardingCommandHandler.VerifyOtpAsync(validId, request, ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken ct
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.HandleLoginAsync(request, ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.HandleForgotPasswordAsync(request, ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [Authorize]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct
    )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        // Get claims from the current user
        var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        bool IsVAlidGuid = Guid.TryParse(userId, out var validId);
        if (!IsVAlidGuid || userRole != RolesUtils.VerificationRole)
        {
            return BadRequest();
        }

        var result = await _authService.HandleResetPasswordAsync(validId, request, ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }
}
