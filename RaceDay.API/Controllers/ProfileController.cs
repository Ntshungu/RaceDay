using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public ProfileController(RaceDayDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var sessionUser = HttpContext.Session.GetUser();
        if (sessionUser is null) return Unauthorized();

        var user = await _db.Users.FindAsync(sessionUser.UserId);
        if (user is null) return NotFound();

        return Ok(new UserResponse(user.UserId, user.FullName, user.Email,
                                   user.Role, user.PhoneNumber, user.ProfilePictureUrl));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ProfileUpdateDto dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.FullName))
            return BadRequest(new { message = "Full name is required." });

        var sessionUser = HttpContext.Session.GetUser();
        if (sessionUser is null) return Unauthorized();

        var user = await _db.Users.FindAsync(sessionUser.UserId);
        if (user is null) return NotFound();

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.ProfilePictureUrl = dto.ProfilePictureUrl;
        await _db.SaveChangesAsync();

        var updated = new UserResponse(user.UserId, user.FullName, user.Email,
                                       user.Role, user.PhoneNumber, user.ProfilePictureUrl);
        HttpContext.Session.SetUser(updated);   // refresh session with new data
        return Ok(updated);
    }
}