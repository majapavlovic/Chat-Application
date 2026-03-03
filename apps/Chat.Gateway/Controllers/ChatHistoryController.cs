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

    [HttpGet("rooms/{roomId}/messages")]
    public async Task<IActionResult> GetRoomMessages(string roomId)
    {
        var client = _httpClientFactory.CreateClient("messaging");

        var res = await client.GetAsync($"/api/messages/{Uri.EscapeDataString(roomId)}");
        if (!res.IsSuccessStatusCode)
        {
            return StatusCode((int)res.StatusCode);
        }

        var messages = await res.Content.ReadFromJsonAsync<object>();
        return Ok(messages);
    }
}