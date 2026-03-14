using Chat.ConversationService.Data;
using Chat.ConversationService.Domain.Entities;
using Chat.ConversationService.Domain.Enums;
using Chat.ConversationService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat.ConversationService.Controllers;

[ApiController]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly ConversationDbContext _db;

    public ConversationsController(ConversationDbContext db) => _db = db;

    // GET api/conversations?userId={userid}
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConversationDto>>> GetForUser([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        var conversations = await _db.Conversations
            .AsNoTracking()
            .Include(c => c.Participants)
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync();

        return Ok(conversations.Select(ToDto));
    }

    // GET api/conversations/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConversationDto>> GetById(Guid id)
    {
        var conv = await _db.Conversations
            .AsNoTracking()
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (conv is null) return NotFound();

        return Ok(ToDto(conv));
    }

    // POST api/conversations
    [HttpPost]
    public async Task<ActionResult<ConversationDto>> Create([FromBody] CreateConversationRequest req)
    {
        if (req.ParticipantIds is null || req.ParticipantIds.Count < 2)
            return BadRequest("At least 2 participants are required.");

        if (req.Type == ConversationType.Group && string.IsNullOrWhiteSpace(req.Name))
            return BadRequest("Name is required for group conversations.");

        if (req.Type == ConversationType.Direct && req.ParticipantIds.Count != 2)
            return BadRequest("Direct conversation must have exactly 2 participants.");

        if (req.Type == ConversationType.Direct)
        {
            var userA = req.ParticipantIds[0];
            var userB = req.ParticipantIds[1];

            var existing = await _db.Conversations
                .AsNoTracking()
                .Include(c => c.Participants)
                .Where(c => c.Type == ConversationType.Direct &&
                            c.Participants.Any(p => p.UserId == userA) &&
                            c.Participants.Any(p => p.UserId == userB))
                .FirstOrDefaultAsync();

            if (existing is not null)
                return Ok(ToDto(existing));
        }

        var now = DateTime.UtcNow;
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Type = req.Type,
            Name = req.Type == ConversationType.Group ? req.Name!.Trim() : null,
            CreatedAtUtc = now,
            Participants = req.ParticipantIds.Distinct().Select(uid => new ConversationParticipant
            {
                UserId = uid,
                JoinedAtUtc = now
            }).ToList()
        };

        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = conversation.Id }, ToDto(conversation));
    }

    // POST api/conversations/{id}/participants
    [HttpPost("{id:guid}/participants")]
    public async Task<IActionResult> AddParticipant(Guid id, [FromBody] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        var conv = await _db.Conversations
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (conv is null) return NotFound();
        if (conv.Type == ConversationType.Direct)
            return BadRequest("Cannot add participants to a direct conversation.");

        if (conv.Participants.Any(p => p.UserId == userId))
            return Conflict("User is already a participant.");

        conv.Participants.Add(new ConversationParticipant
        {
            ConversationId = id,
            UserId = userId,
            JoinedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE api/conversations/{id}/participants/{userId}
    [HttpDelete("{id:guid}/participants/{userId}")]
    public async Task<IActionResult> RemoveParticipant(Guid id, string userId)
    {
        var participant = await _db.Participants
            .FirstOrDefaultAsync(p => p.ConversationId == id && p.UserId == userId);

        if (participant is null) return NotFound();

        _db.Participants.Remove(participant);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ConversationDto ToDto(Conversation c) => new(
        c.Id.ToString("N"),
        c.Type,
        c.Name,
        c.CreatedAtUtc,
        c.Participants.Select(p => p.UserId).ToList()
    );
}
