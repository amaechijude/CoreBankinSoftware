using CustomerAPI.External;
using CustomerAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerAPI.Controlllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NinBvnController(NinBvnService ninBvnService) : ControllerBase
    {

        [HttpPost("nin-search")]
        public async Task<IActionResult> NinSearchAsync([FromBody] NinSearchRequest request)
        {
            var result = await ninBvnService.SearchNin(request);

            return result.IsSuccess
                ? Ok(result.Data)
                : BadRequest(result.ErrorMessage);
        }
    }
}
