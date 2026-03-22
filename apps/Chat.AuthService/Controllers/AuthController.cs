using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Chat.AuthService.Data;
using Chat.AuthService.Data.Entities;
using Chat.AuthService.Models;
using Chat.AuthService.Options;
using Chat.AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Chat.AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _db;
    private readonly PasswordHasher<AuthAccountEntity> _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthController(AuthDbContext db, JwtTokenService jwtTokenService, IOptions<JwtOptions> jwtOptions)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
        _passwordHasher = new PasswordHasher<AuthAccountEntity>();
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) ||
            string.IsNullOrWhiteSpace(req.DisplayName) ||
            string.IsNullOrWhiteSpace(req.Password))
        {
            return BadRequest("Username, DisplayName and Password are required.");
        }

        if (req.Password.Length < 8)
        {
            return BadRequest("Password must be at least 8 characters.");
        }

        var normalizedUsername = req.Username.Trim().ToLowerInvariant();
        var usernameExists = await _db.Accounts
            .AsNoTracking()
            .AnyAsync(a => a.Username == normalizedUsername);

        if (usernameExists)
        {
            return Conflict("Username already exists.");
        }

        var now = DateTime.UtcNow;
        var account = new AuthAccountEntity
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid().ToString("N"),
            Username = normalizedUsername,
            DisplayName = req.DisplayName.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        account.PasswordHash = _passwordHasher.HashPassword(account, req.Password);

        _db.Accounts.Add(account);

        var response = await CreateAuthResponseAsync(account);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        {
            return BadRequest("Username and Password are required.");
        }

        var normalizedUsername = req.Username.Trim().ToLowerInvariant();
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Username == normalizedUsername);
        if (account is null)
        {
            return Unauthorized("Invalid credentials.");
        }

        var verify = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, req.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid credentials.");
        }

        var response = await CreateAuthResponseAsync(account);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest req)
    {
        var existingToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == req.RefreshToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            return Unauthorized("Invalid or expired refresh token.");
        }

        var account = await _db.Accounts
            .FirstOrDefaultAsync(a => a.UserId == existingToken.UserId);

        if (account is null)
        {
            return Unauthorized("Account not found.");
        }

        existingToken.RevokedAtUtc = DateTime.UtcNow;
        var newRefreshString = _jwtTokenService.GenerateRefreshTokenString();
        existingToken.ReplacedByToken = newRefreshString;

        var refreshExpiry = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays);
        _db.RefreshTokens.Add(new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = account.UserId,
            Token = newRefreshString,
            ExpiresAtUtc = refreshExpiry,
            CreatedAtUtc = DateTime.UtcNow
        });

        var (accessToken, accessExpiry) = _jwtTokenService.CreateAccessToken(account);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponse(accessToken, accessExpiry, account.UserId,
            account.Username, account.DisplayName, newRefreshString, refreshExpiry));
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RevokeRequest req)
    {
        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == req.RefreshToken);

        if (token is null)
        {
            return NotFound();
        }

        token.RevokedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<CurrentUserDto> Me()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.FindFirstValue("username") ?? User.FindFirstValue(ClaimTypes.Name);
        var displayName = User.FindFirstValue(JwtRegisteredClaimNames.UniqueName) ?? User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        return Ok(new CurrentUserDto(userId, username ?? userId, displayName ?? username ?? userId));
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(AuthAccountEntity account)
    {
        var (accessToken, accessExpiry) = _jwtTokenService.CreateAccessToken(account);

        var refreshTokenString = _jwtTokenService.GenerateRefreshTokenString();
        var refreshExpiry = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays);

        _db.RefreshTokens.Add(new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = account.UserId,
            Token = refreshTokenString,
            ExpiresAtUtc = refreshExpiry,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return new AuthResponse(accessToken, accessExpiry, account.UserId,
            account.Username, account.DisplayName, refreshTokenString, refreshExpiry);
    }
}
