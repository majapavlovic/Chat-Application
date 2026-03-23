using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net.Http.Json;

namespace Chat.Gateway.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : GatewayControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [EnableRateLimiting("auth_ip")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] object body)
    {
        var client = _httpClientFactory.CreateClient("auth");
        var res = await client.PostAsJsonAsync("/api/auth/register", body);

        if (!res.IsSuccessStatusCode)
            return await ToActionResultAsync(res);

        return await HandleAuthResponseAsync(res);
    }

    [EnableRateLimiting("auth_ip")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] object body)
    {
        var client = _httpClientFactory.CreateClient("auth");
        var res = await client.PostAsJsonAsync("/api/auth/login", body);

        if (!res.IsSuccessStatusCode)
            return await ToActionResultAsync(res);

        return await HandleAuthResponseAsync(res);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { message = "No refresh token." });

        var client = _httpClientFactory.CreateClient("auth");
        var res = await client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });

        if (!res.IsSuccessStatusCode)
        {
            ClearAuthCookies();
            return await ToActionResultAsync(res);
        }

        return await HandleAuthResponseAsync(res);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var client = _httpClientFactory.CreateClient("auth");
            await client.PostAsJsonAsync("/api/auth/revoke", new { refreshToken });
        }

        ClearAuthCookies();
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var token = Request.Cookies["access_token"]!;
        var client = _httpClientFactory.CreateClient("auth");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var res = await client.GetAsync("/api/auth/me");
        return await ToActionResultAsync(res);
    }

    private async Task<IActionResult> HandleAuthResponseAsync(HttpResponseMessage res)
    {
        var auth = await res.Content.ReadFromJsonAsync<InternalAuthResponse>();
        if (auth is null)
            return StatusCode(502, new { message = "Invalid response from auth service." });

        SetAuthCookies(auth.AccessToken, auth.ExpiresAtUtc,
                       auth.RefreshToken, auth.RefreshTokenExpiresAtUtc);

        return Ok(new { auth.UserId, auth.Username, auth.DisplayName });
    }

    private void SetAuthCookies(string accessToken, DateTime accessExpiry,
                                string refreshToken, DateTime refreshExpiry)
    {
        Response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = accessExpiry
        });

        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            Expires = refreshExpiry
        });
    }

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete("access_token", new CookieOptions { Path = "/" });
        Response.Cookies.Delete("refresh_token", new CookieOptions { Path = "/api/auth" });
    }

    private record InternalAuthResponse(
        string AccessToken,
        DateTime ExpiresAtUtc,
        string UserId,
        string Username,
        string DisplayName,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc);
}
