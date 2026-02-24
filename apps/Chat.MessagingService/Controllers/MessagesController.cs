using Chat.MessagingService.Data;
using Chat.MessagingService.Data.Entities;
using Chat.MessagingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat.MessagingService.Controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly MessagingDbContext _db;

    public MessagesController(MessagingDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> Create([FromBody] CreateMessageRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RoomId) ||
            string.IsNullOrWhiteSpace(req.SenderId) ||
            string.IsNullOrWhiteSpace(req.Text))
            return BadRequest("RoomId, SenderId and Text are required.");

        var entity = new MessageEntity
        {
            Id = Guid.NewGuid(),
            RoomId = req.RoomId.Trim(),
            SenderId = req.SenderId.Trim(),
            Text = req.Text.Trim(),
            PersistedAtUtc = DateTime.UtcNow
        };

        _db.Messages.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(new MessageDto(
            entity.Id.ToString("N"),
            entity.RoomId,
            entity.SenderId,
            entity.Text,
            entity.PersistedAtUtc
        ));
    }

    [HttpGet("{roomId}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetByRoom(string roomId)
    {
        var items = await _db.Messages
            .Where(m => m.RoomId == roomId)
            .OrderBy(m => m.PersistedAtUtc)
            .Select(m => new MessageDto(
                m.Id.ToString("N"),
                m.RoomId,
                m.SenderId,
                m.Text,
                m.PersistedAtUtc
            ))
            .ToListAsync();

        return Ok(items);
    }
}