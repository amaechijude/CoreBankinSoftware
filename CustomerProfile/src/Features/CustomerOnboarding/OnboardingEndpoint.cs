using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace src.Features.CustomerOnboarding
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnboardingEndpoint(OnboardingCommandHandler onboardingCommandHandler) : ControllerBase
    {
        private readonly OnboardingCommandHandler _onboardingCommandHandler = onboardingCommandHandler;

        [HttpPost]
        public async Task<IActionResult> OnboardCustomer([FromBody] OnboardingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _onboardingCommandHandler.HandleAsync(request);
            
            return result.IsSuccess 
                ? Ok(result.Data) 
                : BadRequest(result.ErrorMessage);
        }
    }
}
