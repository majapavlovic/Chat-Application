using Microsoft.AspNetCore.Mvc;

namespace Chat.Gateway.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatHistoryController : ControllerBase
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

        var res = await client.GetAsync($"/api/messages/conversation/{Uri.EscapeDataString(conversationId)}");
        if (!res.IsSuccessStatusCode)
        {
            return StatusCode((int)res.StatusCode);
        }

        var messages = await res.Content.ReadFromJsonAsync<object>();
        return Ok(messages);
    }
}