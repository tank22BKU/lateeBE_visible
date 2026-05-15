using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Auth;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();

        var tokens = await _authService.LoginAsync(request, ip, userAgent, cancellationToken);
        if (tokens is null)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        return Ok(tokens);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();

        var tokens = await _authService.RefreshAsync(request.RefreshToken, ip, userAgent, cancellationToken);
        if (tokens is null)
        {
            return Unauthorized(new { message = "Refresh token is invalid or expired." });
        }

        return Ok(tokens);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

        DateTime? accessExpiresAt = null;
        if (long.TryParse(expClaim, out var expUnix))
        {
            accessExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
        }

        await _authService.LogoutAsync(request.RefreshToken, jti, accessExpiresAt, userId, cancellationToken);

        return Ok(new { message = "Logged out successfully." });
    }
}
