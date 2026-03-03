using Chat.MessagingService.Data;
using Chat.MessagingService.Data.Entities;
using Chat.MessagingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Chat.MessagingService.Controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly MessagingDbContext _db;

    public MessagesController(MessagingDbContext db) => _db = db;

    [HttpGet("{roomId}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetByRoom(string roomId)
    {
        var items = await _db.Messages
            .AsNoTracking()
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

    [HttpPost]
    public async Task<ActionResult<MessageDto>> Create([FromBody] CreateMessageRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RoomId) ||
            string.IsNullOrWhiteSpace(req.SenderId) ||
            string.IsNullOrWhiteSpace(req.Text) ||
            string.IsNullOrWhiteSpace(req.ClientMessageId))
            return BadRequest("RoomId, SenderId, Text and ClientMessageId are required.");

        var existing = await _db.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ClientMessageId == req.ClientMessageId);

        if (existing != null)
        {
            return Ok(ToDto(existing));
        }

        var entity = new MessageEntity
        {
            Id = Guid.NewGuid(),
            RoomId = req.RoomId.Trim(),
            SenderId = req.SenderId.Trim(),
            Text = req.Text.Trim(),
            ClientMessageId = req.ClientMessageId.Trim(),
            PersistedAtUtc = DateTime.UtcNow
        };

        _db.Messages.Add(entity);

        try
        {
            await _db.SaveChangesAsync();
            return Ok(ToDto(entity));
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            var winner = await _db.Messages
                .AsNoTracking()
                .FirstAsync(m => m.ClientMessageId == req.ClientMessageId);

            return Ok(ToDto(winner));
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException pg && pg.SqlState == "23505";

    private static MessageDto ToDto(MessageEntity m) => new(
        m.Id.ToString("N"),
        m.RoomId,
        m.SenderId,
        m.Text,
        m.PersistedAtUtc
    );
}