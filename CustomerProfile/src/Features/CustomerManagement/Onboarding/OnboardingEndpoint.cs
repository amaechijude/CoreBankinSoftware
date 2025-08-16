using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using UserProfile.API.Features.CustomerManagement.BvnNinVerification;

namespace UserProfile.API.Features.CustomerManagement.Onboarding
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnboardingController(
        OnboardingCommandHandler onboardingCommandHandler
        ) : ControllerBase
    {
        private readonly OnboardingCommandHandler _onboardingCommandHandler = onboardingCommandHandler;

        private readonly string _otpKey = "otp-token";


        [HttpPost("send-otp")]
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
        public async Task<IActionResult> VerifyRegistrationOtpAsync([FromBody] OtpVerifyRequest request)
        {
            var tokenString = Request.Cookies[_otpKey];
            if (string.IsNullOrEmpty(tokenString))
                return BadRequest("Invalid Request");

            var result = await _onboardingCommandHandler.VerifyRegistrationOtpAsync(request.Code, tokenString);

            return result.IsSuccess
                ? Ok(result.Data)
                : BadRequest(result.ErrorMessage);
        }

        [HttpPost("nin-search")]
        public async Task<IActionResult> NinSearchAsync([FromBody] NinSearchRequest request)
        {
            var result = await _onboardingCommandHandler.NinSearch(request);

            return result.IsSuccess
                ? Ok(result.Data)
                : BadRequest(result.ErrorMessage);   
        }

        [HttpGet("match-image")]
        public IActionResult MatchImage()
        {
            ////if (!ModelState.IsValid) return BadRequest();
            //await Task.Delay(100);
            return Ok(new { hello = "hello"} );
        }
    }
    
    public class SendImageRequest
    {
        [Required]
        public IFormFile? Image { get; set; }
    }
}
