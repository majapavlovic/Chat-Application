using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Chat.Gateway.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public UsersController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET api/users?query={username}
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? query)
    {
        var client = _httpClientFactory.CreateClient("users");
        var url = query is not null
            ? $"/api/users?query={Uri.EscapeDataString(query)}"
            : "/api/users";

        var res = await client.GetAsync(url);
        return await ToActionResultAsync(res);
    }

    // GET api/users/{userId}
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetById(string userId)
    {
        var client = _httpClientFactory.CreateClient("users");
        var res = await client.GetAsync($"/api/users/{Uri.EscapeDataString(userId)}");
        return await ToActionResultAsync(res);
    }

    // POST api/users
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object body)
    {
        var client = _httpClientFactory.CreateClient("users");
        var res = await client.PostAsJsonAsync("/api/users", body);
        return await ToActionResultAsync(res);
    }

    // PATCH api/users/{userId}/presence
    [HttpPatch("{userId}/presence")]
    public async Task<IActionResult> UpdatePresence(string userId, [FromBody] object body)
    {
        var client = _httpClientFactory.CreateClient("users");
        var res = await client.PatchAsJsonAsync($"/api/users/{Uri.EscapeDataString(userId)}/presence", body);
        return await ToActionResultAsync(res);
    }

    // GET api/users/{userId}/connections?status=Accepted
    [HttpGet("{userId}/connections")]
    public async Task<IActionResult> GetConnections(string userId, [FromQuery] string? status)
    {
        var client = _httpClientFactory.CreateClient("users");
        var url = status is not null
            ? $"/api/connections/{Uri.EscapeDataString(userId)}?status={Uri.EscapeDataString(status)}"
            : $"/api/connections/{Uri.EscapeDataString(userId)}";

        var res = await client.GetAsync(url);
        return await ToActionResultAsync(res);
    }

    // PUT api/users/connections
    [HttpPut("connections")]
    public async Task<IActionResult> UpsertConnection([FromBody] object body)
    {
        var client = _httpClientFactory.CreateClient("users");
        var res = await client.PutAsJsonAsync("/api/connections", body);
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
