using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace src.Features.CustomerManagement.Onboarding
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnboardingEndpoint(
        OnboardingCommandHandler onboardingCommandHandler
        ) : ControllerBase
    {
        private readonly OnboardingCommandHandler _onboardingCommandHandler = onboardingCommandHandler;

        private readonly string _otpKey = "otp-token";


        [HttpPost]
        public async Task<IActionResult> OnboardCustomer([FromBody] OnboardingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _onboardingCommandHandler.HandleAsync(request);

            if (result.IsSuccess && result.Data is not null)
            {
                Response.Cookies.Append(_otpKey, result.Data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Unspecified,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(11),

                });
            }

            return BadRequest(result.ErrorMessage);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtpAsync([FromBody] OtpVerifyRequest request)
        {
            var tokenString = Request.Cookies[_otpKey];
            if (string.IsNullOrEmpty( tokenString))
                return BadRequest("Invalid Request");

            return Ok();
        }


        [HttpPost("compare-photos")]
        public async Task<IActionResult> NinSearch([FromForm] SendIformFile request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _onboardingCommandHandler.Compare(request);
            return result.IsSuccess
                ? Ok(result.Data)
                : BadRequest(result.ErrorMessage);
        }

    }

    public record OtpVerifyRequest(string Code);

    public record SendIformFile (IFormFile Image);
    
}
