using CustomerAPI.DTO;
using CustomerAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerAPI.Controlllers
{
    [Route("api/[controller]/register")]
    [ApiController]
    public class AuthController(
        AuthService onboardingCommandHandler,
        IHostEnvironment hostEnvironment
        ) : ControllerBase
    {
        private readonly AuthService _onboardingCommandHandler = onboardingCommandHandler;
        private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
        private readonly string _otpKey = "otp-token";
        private readonly int _expireInminutes = 11;
        private readonly string _cookiePath = "/api/Auth";


        [HttpPost("send-otp")]
        public async Task<IActionResult> OnboardCustomer([FromBody] OnboardingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _onboardingCommandHandler.HandleAsync(request);

            if (result.IsSuccess && result.Data is not null)
                return Ok(result.Data);

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

        [HttpPost("set-password")]
        public async Task<IActionResult> SetPasswordAsync([FromBody] SetPasswordRequest request)
        {
            await Task.Delay(100);
            return Ok();
        }

    }
}
