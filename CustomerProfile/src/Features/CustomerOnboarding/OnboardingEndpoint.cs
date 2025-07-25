using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace src.Features.CustomerOnboarding
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnboardingEndpoint : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Customer Onboarding API is running.");
        }
    }
}
