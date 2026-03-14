using Microsoft.AspNetCore.Mvc;

namespace Chat.Gateway.Controllers;

[ApiController]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ConversationsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetForUser([FromQuery] string userId)
    {
        var client = _httpClientFactory.CreateClient("conversation");
        var res = await client.GetAsync($"/api/conversations?userId={Uri.EscapeDataString(userId)}");

        if (!res.IsSuccessStatusCode)
        {
            return StatusCode((int)res.StatusCode);
        }

        return Ok(await res.Content.ReadFromJsonAsync<object>());
    }

    [HttpGet("{conversationId}")]
    public async Task<IActionResult> GetById(string conversationId)
    {
        var client = _httpClientFactory.CreateClient("conversation");
        var res = await client.GetAsync($"/api/conversations/{Uri.EscapeDataString(conversationId)}");

        if (!res.IsSuccessStatusCode)
        {
            return StatusCode((int)res.StatusCode);
        }

        return Ok(await res.Content.ReadFromJsonAsync<object>());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object body)
    {
        var client = _httpClientFactory.CreateClient("conversation");
        var res = await client.PostAsJsonAsync("/api/conversations", body);
        var payload = await res.Content.ReadFromJsonAsync<object>();

        return StatusCode((int)res.StatusCode, payload);
    }
}