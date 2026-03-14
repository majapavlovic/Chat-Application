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
            usersQuery = usersQuery.Where(u =>
                u.Id.Contains(q) ||
                u.Username.Contains(q) ||
                u.DisplayName.Contains(q)
            );
        }

        var users = await usersQuery
            .OrderBy(u => u.DisplayName)
            .ToListAsync();

        return Ok(users.Select(ToDto));
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetById(string userId)
    {
        var normalizedUserId = userId.Trim();
        var normalizedUserIdLower = normalizedUserId.ToLowerInvariant();
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id.ToLower() == normalizedUserIdLower);
        if (user is null) return NotFound();

        return Ok(ToDto(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserId) ||
            string.IsNullOrWhiteSpace(req.Username) ||
            string.IsNullOrWhiteSpace(req.DisplayName))
            return BadRequest(new { message = "UserId, Username and DisplayName are required." });

        var normalizedUserId = req.UserId.Trim().ToLowerInvariant();
        var normalizedUsername = req.Username.Trim().ToLowerInvariant();
        var normalizedDisplayName = req.DisplayName.Trim();
        var existing = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id.ToLower() == normalizedUserId);

        if (existing is not null)
            return Ok(ToDto(existing));

        var existingByUsername = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == normalizedUsername);
        if (existingByUsername is not null)
        {
            var previousUserId = existingByUsername.Id;

            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE users SET \"Id\" = {normalizedUserId}, \"DisplayName\" = {normalizedDisplayName} WHERE \"Username\" = {normalizedUsername};"
            );

            var previousUserIdLower = previousUserId.ToLowerInvariant();

            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE user_connections SET \"UserAId\" = {normalizedUserId} WHERE lower(\"UserAId\") = {previousUserIdLower};"
            );
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE user_connections SET \"UserBId\" = {normalizedUserId} WHERE lower(\"UserBId\") = {previousUserIdLower};"
            );
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE user_connections SET \"RequestedByUserId\" = {normalizedUserId} WHERE lower(\"RequestedByUserId\") = {previousUserIdLower};"
            );

            var remapped = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id.ToLower() == normalizedUserId);
            if (remapped is not null)
            {
                return Ok(ToDto(remapped));
            }

            return StatusCode(500, new { message = "Failed to remap existing user profile." });
        }

        var entity = new UserEntity
        {
            Id = normalizedUserId,
            Username = normalizedUsername,
            DisplayName = normalizedDisplayName,
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
        var normalizedUserId = userId.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id.ToLower() == normalizedUserId);
        if (user is null) return NotFound();

        user.IsOnline = req.IsOnline;
        user.LastSeenAtUtc = req.IsOnline ? null : DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static UserDto ToDto(UserEntity u) => new(
        u.Id,
        u.Username,
        u.DisplayName,
        u.IsOnline,
        u.LastSeenAtUtc,
        u.CreatedAtUtc
    );
}
