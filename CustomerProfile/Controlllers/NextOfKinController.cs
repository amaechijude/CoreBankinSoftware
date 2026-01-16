using System.Security.Claims;
using CustomerProfile.DTO;
using CustomerProfile.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerProfile.Controlllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class NextOfKinController(NextOfKinService service) : ControllerBase
{
    private readonly NextOfKinService _service = service;

    [HttpPost]
    public async Task<IActionResult> AddNextOfKin(
        [FromBody] AddNextOfKinRequest request,
        CancellationToken ct
    )
    {
        var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
        if (!Guid.TryParse(userId, out var guid))
        {
            return Unauthorized();
        }

        var result = await _service.AddNextOfKinAsync(guid, request, ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet]
    public async Task<IActionResult> GetNextOfKins(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
        if (!Guid.TryParse(userId, out var guid))
        {
            return Unauthorized();
        }

        var result = await _service.GetNextOfKinsAsync(guid, ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveNextOfKin(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
        if (!Guid.TryParse(userId, out var guid))
        {
            return Unauthorized();
        }

        var result = await _service.RemoveNextOfKinAsync(guid, id, ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }
}
