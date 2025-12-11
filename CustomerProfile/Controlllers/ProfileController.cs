using System.Security.Claims;
using CustomerAPI.DTO;
using CustomerAPI.DTO.BvnNinVerification;
using CustomerAPI.JwtTokenService;
using CustomerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerAPI.Controlllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController(NinBvnService ninBvnService) : ControllerBase
    {
        [Authorize]
        [HttpPost("bvn-search")]
        public async Task<IActionResult> SearchBvnAsync(
            [FromBody] BvnSearchRequest request,
            CancellationToken ct
        )
        {
            var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            bool IsVAlidGuid = Guid.TryParse(userId, out var validUserId);
            if (!IsVAlidGuid || userRole != RolesUtils.UserRole)
                return Unauthorized("Unauthorised: Try Login Again");

            var result = await ninBvnService.SearchBvnAsync(validUserId, request, ct);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpPost("face-verification")]
        public async Task<IActionResult> FaceVerificationAsync(
            [FromBody] FaceVerificationRequest request,
            CancellationToken ct
        )
        {
            var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            bool IsVAlidGuid = Guid.TryParse(userId, out var validUserId);
            if (!IsVAlidGuid || userRole != RolesUtils.UserRole)
                return Unauthorized("Unauthorised: Try Login Again");

            var result = await ninBvnService.FaceVerificationAsync(validUserId, request, ct);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpPost("set-profile")]
        public async Task<IActionResult> SetProfileAsync(
            [FromBody] SetProfileRequest request,
            CancellationToken ct
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            bool IsVAlidGuid = Guid.TryParse(userId, out var validId);
            if (!IsVAlidGuid || userRole != RolesUtils.VerificationRole)
                return BadRequest("Request Timeout");

            var result = await ninBvnService.HandleSetProfileAsync(validId, request, ct);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }
    }
}
