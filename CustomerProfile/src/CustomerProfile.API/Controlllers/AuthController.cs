using System.Security.Claims;
using CustomerAPI.DTO;
using CustomerAPI.JwtTokenService;
using CustomerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerAPI.Controlllers
{
    [Route("api/[controller]/register")]
    [ApiController]
    public class AuthController(AuthService _onboardingCommandHandler) : ControllerBase
    {

        [HttpPost("send-otp")]
        public async Task<IActionResult> OnboardCustomer([FromBody] OnboardingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _onboardingCommandHandler.InitiateOnboard(request);

            if (result.IsSuccess && result.Data is not null)
                return Ok(result.Data);

            return BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyRegistrationOtpAsync([FromBody] OtpVerifyRequestBody request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            // Get claims from the current user
            var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            bool IsVAlidGuid = Guid.TryParse(userId, out var validId);
            if (!IsVAlidGuid || userRole != RolesUtils.VerificationRole)
                return BadRequest(ModelState);

            var result = await _onboardingCommandHandler.VerifyOtpAsync(validId, request);
            return result.IsSuccess
                ? Ok(result.Data)
                : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        public async Task<IActionResult> SendDetailAsync(SetDetailsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            bool IsVAlidGuid = Guid.TryParse(userId, out var validId);
            if (!IsVAlidGuid || userRole != RolesUtils.VerificationRole)

                return BadRequest("Request Timeout");

            var result = await _onboardingCommandHandler.SetDetailsAsync(validId, request);
            return result.IsSuccess
                ? Ok(result.Data)
                : BadRequest(result.ErrorMessage);
        }

    }
}
