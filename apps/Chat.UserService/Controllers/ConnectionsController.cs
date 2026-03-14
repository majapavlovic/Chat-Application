using Chat.UserService.Data;
using Chat.UserService.Data.Entities;
using Chat.UserService.Domain.Enums;
using Chat.UserService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat.UserService.Controllers;

[ApiController]
[Route("api/connections")]
public class ConnectionsController : ControllerBase
{
    private readonly UserDbContext _db;

    public ConnectionsController(UserDbContext db) => _db = db;

    [HttpGet("{userId}")]
    public async Task<ActionResult<IEnumerable<ConnectionDto>>> GetForUser(string userId, [FromQuery] ConnectionStatus? status)
    {
        var items = await _db.Connections
            .AsNoTracking()
            .Where(c => c.UserAId == userId || c.UserBId == userId)
            .Where(c => status == null || c.Status == status)
            .OrderByDescending(c => c.UpdatedAtUtc)
            .ToListAsync();

        return Ok(items.Select(c => ToDto(userId, c)));
    }

    [HttpPut]
    public async Task<ActionResult<ConnectionDto>> Upsert([FromBody] UpsertConnectionRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.OtherUserId))
            return BadRequest("UserId and OtherUserId are required.");

        var actorId = req.UserId.Trim();
        var otherId = req.OtherUserId.Trim();

        if (actorId == otherId)
            return BadRequest("Cannot create a connection with yourself.");

        var userExists = await _db.Users.AsNoTracking().AnyAsync(u => u.Id == actorId);
        var otherExists = await _db.Users.AsNoTracking().AnyAsync(u => u.Id == otherId);

        if (!userExists || !otherExists)
            return NotFound("Both users must exist.");

        var (userAId, userBId) = NormalizePair(actorId, otherId);

        var entity = await _db.Connections.FirstOrDefaultAsync(c => c.UserAId == userAId && c.UserBId == userBId);
        if (entity is null)
        {
            entity = new UserConnectionEntity
            {
                UserAId = userAId,
                UserBId = userBId,
                RequestedByUserId = actorId,
                Status = req.Status,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _db.Connections.Add(entity);
        }
        else
        {
            entity.Status = req.Status;
            entity.RequestedByUserId = actorId;
            entity.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return Ok(ToDto(actorId, entity));
    }

    private static (string UserAId, string UserBId) NormalizePair(string userId, string otherUserId)
        => string.CompareOrdinal(userId, otherUserId) <= 0
            ? (userId, otherUserId)
            : (otherUserId, userId);

    private static ConnectionDto ToDto(string userId, UserConnectionEntity c)
    {
        var otherUserId = c.UserAId == userId ? c.UserBId : c.UserAId;

        return new ConnectionDto(
            userId,
            otherUserId,
            c.Status,
            c.RequestedByUserId,
            c.UpdatedAtUtc
        );
    }
}
