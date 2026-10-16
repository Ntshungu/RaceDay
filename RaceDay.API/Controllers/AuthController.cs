using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public AuthController(RaceDayDbContext db) => _db = db;

    /// <summary>Registers a new Organiser or Participant.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (dto is null ||
            string.IsNullOrWhiteSpace(dto.FullName) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password) ||
            string.IsNullOrWhiteSpace(dto.Role))
        {
            return BadRequest(new { message = "Full name, email, password and role are required." });
        }

        if (dto.Password.Length < 8)
            return BadRequest(new { message = "Password must contain at least 8 characters." });

        if (!MailAddress.TryCreate(dto.Email.Trim(), out var mailAddress))
            return BadRequest(new { message = "Enter a valid email address." });

        var role = dto.Role.Trim();
        if (!role.Equals("Organiser", StringComparison.OrdinalIgnoreCase) &&
            !role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Role must be Organiser or Participant." });
        }

        role = role.Equals("Organiser", StringComparison.OrdinalIgnoreCase)
            ? "Organiser"
            : "Participant";

        var email = mailAddress.Address.ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { message = "Email already registered." });

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = role,
            PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber)
                ? null
                : dto.PhoneNumber.Trim()
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var response = ToResponse(user);
        return CreatedAtAction(nameof(Me), routeValues: null, value: response);
    }

    /// <summary>Authenticates a user and starts a session.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        var response = ToResponse(user);
        HttpContext.Session.SetUser(response);
        return Ok(response);
    }

    /// <summary>Ends the current session.</summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out." });
    }

    /// <summary>Returns the currently logged-in user.</summary>
    [HttpGet("me")]
    public IActionResult Me()
    {
        var user = HttpContext.Session.GetUser();
        return user is null ? Unauthorized() : Ok(user);
    }

    private static UserResponse ToResponse(User user) =>
        new(user.UserId, user.FullName, user.Email, user.Role,
            user.PhoneNumber, user.ProfilePictureUrl);
}
