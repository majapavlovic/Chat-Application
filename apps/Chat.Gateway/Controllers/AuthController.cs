using Microsoft.AspNetCore.Mvc;

namespace Chat.Gateway.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] object body)
    {
        var client = _httpClientFactory.CreateClient("auth");
        var res = await client.PostAsJsonAsync("/api/auth/register", body);
        var payload = await res.Content.ReadFromJsonAsync<object>();

        return StatusCode((int)res.StatusCode, payload);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] object body)
    {
        var client = _httpClientFactory.CreateClient("auth");
        var res = await client.PostAsJsonAsync("/api/auth/login", body);
        var payload = await res.Content.ReadFromJsonAsync<object>();

        return StatusCode((int)res.StatusCode, payload);
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var client = _httpClientFactory.CreateClient("auth");

        if (Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authHeader.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim());
        }

        var res = await client.GetAsync("/api/auth/me");
        var payload = await res.Content.ReadFromJsonAsync<object>();

        return StatusCode((int)res.StatusCode, payload);
    }
}
