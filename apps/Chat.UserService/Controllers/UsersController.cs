using Chat.UserService.Data;
using Chat.UserService.Data.Entities;
using Chat.UserService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat.UserService.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _db;

    public UsersController(UserDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll([FromQuery] string? query)
    {
        var usersQuery = _db.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            usersQuery = usersQuery.Where(u => u.Id.Contains(q) || u.DisplayName.Contains(q));
        }

        var users = await usersQuery
            .OrderBy(u => u.DisplayName)
            .ToListAsync();

        return Ok(users.Select(ToDto));
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetById(string userId)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return NotFound();

        return Ok(ToDto(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.DisplayName))
            return BadRequest("UserId and DisplayName are required.");

        var normalizedUserId = req.UserId.Trim();
        var existing = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == normalizedUserId);

        if (existing is not null)
            return Ok(ToDto(existing));

        var entity = new UserEntity
        {
            Id = normalizedUserId,
            DisplayName = req.DisplayName.Trim(),
            IsOnline = false,
            LastSeenAtUtc = null,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Users.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { userId = entity.Id }, ToDto(entity));
    }

    [HttpPatch("{userId}/presence")]
    public async Task<IActionResult> UpdatePresence(string userId, [FromBody] UpdatePresenceRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return NotFound();

        user.IsOnline = req.IsOnline;
        user.LastSeenAtUtc = req.IsOnline ? null : DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static UserDto ToDto(UserEntity u) => new(
        u.Id,
        u.DisplayName,
        u.IsOnline,
        u.LastSeenAtUtc,
        u.CreatedAtUtc
    );
}
