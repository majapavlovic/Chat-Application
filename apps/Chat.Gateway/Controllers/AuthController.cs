using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

        return await ToActionResultAsync(res);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] object body)
    {
        var client = _httpClientFactory.CreateClient("auth");
        var res = await client.PostAsJsonAsync("/api/auth/login", body);

        return await ToActionResultAsync(res);
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return Unauthorized();
        }

        var token = authHeader.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        var client = _httpClientFactory.CreateClient("auth");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var res = await client.GetAsync("/api/auth/me");

        return await ToActionResultAsync(res);
    }

    private async Task<IActionResult> ToActionResultAsync(HttpResponseMessage res)
    {
        var statusCode = (int)res.StatusCode;
        var raw = await res.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(raw))
        {
            if (!res.IsSuccessStatusCode)
            {
                return StatusCode(statusCode, new
                {
                    message = res.ReasonPhrase ?? "Request failed.",
                    statusCode
                });
            }

            return StatusCode(statusCode);
        }

        var contentType = res.Content.Headers.ContentType?.MediaType;
        if (!string.IsNullOrWhiteSpace(contentType) &&
            contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var payload = JsonSerializer.Deserialize<object>(raw);
                return StatusCode(statusCode, payload);
            }
            catch
            {
            }
        }

        if (!res.IsSuccessStatusCode)
        {
            return StatusCode(statusCode, new
            {
                message = raw,
                statusCode
            });
        }

        return StatusCode(statusCode, raw);
    }
}
