using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Gateway.Controllers;

[Authorize]
[ApiController]
[Route("api/chat")]
public class ChatHistoryController : GatewayControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ChatHistoryController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<IActionResult> GetConversationMessages(string conversationId)
    {
        var client = _httpClientFactory.CreateClient("messaging");
        var res = await client.GetAsync(
            $"/api/messages/conversation/{Uri.EscapeDataString(conversationId)}");
        return await ToActionResultAsync(res);
    }
}