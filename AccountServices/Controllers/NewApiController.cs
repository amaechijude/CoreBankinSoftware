using AccountServices.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccountServices.Controllers
{
    [ApiController]
    [Route("/api")]
    public class NewApiController(AccountDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var accounts = await context.Accounts.ToListAsync();
            return Ok(accounts);
        }
    }
}