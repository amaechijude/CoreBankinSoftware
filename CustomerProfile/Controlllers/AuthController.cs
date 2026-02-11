using CustomerProfile.DTO;
using CustomerProfile.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerProfile.Controlllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    private readonly AuthService _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken ct
    )
    {
        var response = await _authService.HandleLoginAsync(request, ct);
        return response.IsSuccess ? Ok(response.Data) : BadRequest(response.ErrorMessage);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct
    )
    {
        var response = await _authService.HandleRefreshTokenAsync(request, ct);
        return response.IsSuccess ? Ok(response.Data) : BadRequest(response.ErrorMessage);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct
    )
    {
        var response = await _authService.HandleForgotPasswordAsync(request, ct);
        return response.IsSuccess ? Ok(response.Data) : BadRequest(response.ErrorMessage);
    }

    [HttpPost("reset-password/{id:guid}")]
    public async Task<IActionResult> ResetPasswordAsync(
        Guid id,
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct
    )
    {
        var response = await _authService.HandleResetPasswordAsync(id, request, ct);
        return response.IsSuccess ? Ok(response.Data) : BadRequest(response.ErrorMessage);
    }
}
