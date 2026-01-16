using System.Security.Claims;
using CustomerProfile.DTO;
using CustomerProfile.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerProfile.Controlllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class KycController(KycService service) : ControllerBase
{
    private readonly KycService _service = service;

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(
        [FromForm] UploadKycRequest request,
        CancellationToken ct
    )
    {
        var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
        if (!Guid.TryParse(userId, out var guid))
        {
            return Unauthorized();
        }

        var result = await _service.UploadDocumentAsync(guid, request, ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet]
    public async Task<IActionResult> GetDocuments(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
        if (!Guid.TryParse(userId, out var guid))
        {
            return Unauthorized();
        }

        var result = await _service.GetDocumentsAsync(guid, ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }
}
