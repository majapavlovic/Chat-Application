using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Gateway.Controllers;

[Authorize]
[ApiController]
[Route("api/conversations")]
public class ConversationsController : GatewayControllerBase
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
        return await ToActionResultAsync(res);
    }

    [HttpGet("{conversationId}")]
    public async Task<IActionResult> GetById(string conversationId)
    {
        var client = _httpClientFactory.CreateClient("conversation");
        var res = await client.GetAsync($"/api/conversations/{Uri.EscapeDataString(conversationId)}");
        return await ToActionResultAsync(res);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object body)
    {
        var client = _httpClientFactory.CreateClient("conversation");
        var res = await client.PostAsJsonAsync("/api/conversations", body);
        return await ToActionResultAsync(res);
    }
}