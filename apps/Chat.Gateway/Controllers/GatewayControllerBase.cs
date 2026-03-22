using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Chat.Gateway.Controllers;

public abstract class GatewayControllerBase : ControllerBase
{
    protected async Task<IActionResult> ToActionResultAsync(HttpResponseMessage res)
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
