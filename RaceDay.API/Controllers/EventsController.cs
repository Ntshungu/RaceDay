using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public EventsController(RaceDayDbContext db) => _db = db;

    /// <summary>List all events, optionally filtered by type or date.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? type, [FromQuery] DateTime? from)
    {
        var q = _db.Events.Include(e => e.Categories).AsQueryable();
        if (!string.IsNullOrEmpty(type)) q = q.Where(e => e.EventType == type);
        if (from.HasValue) q = q.Where(e => e.EventDate >= from.Value);
        return Ok(await q.OrderBy(e => e.EventDate).ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var ev = await _db.Events.Include(e => e.Categories)
                                 .FirstOrDefaultAsync(e => e.EventId == id);
        return ev is null ? NotFound() : Ok(ev);
    }

    /// <summary>Events created by the current organiser.</summary>
    [HttpGet("my-events")]
    public async Task<IActionResult> MyEvents()
    {
        if (!HttpContext.Session.IsOrganiser()) return StatusCode(403, new { message = "Access denied. Please sign in with the correct account and check your permissions." });
        var uid = HttpContext.Session.GetUser()!.UserId;

        var events = await _db.Events.Include(e => e.Categories)
                                     .Include(e => e.Enrolments)
                                     .Where(e => e.OrganiserId == uid)
                                     .OrderBy(e => e.EventDate)
                                     .ToListAsync();
        return Ok(events);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EventDto dto)
    {
        if (!HttpContext.Session.IsOrganiser()) return StatusCode(403, new { message = "Access denied. Please sign in with the correct account and check your permissions." });
        if (!IsValid(dto)) return BadRequest(new { message = "Enter valid event details." });
        var uid = HttpContext.Session.GetUser()!.UserId;

        var ev = new Event
        {
            Name = dto.Name,
            Description = dto.Description,
            EventDate = dto.EventDate,
            Location = dto.Location,
            Distance = dto.Distance,
            EventType = dto.EventType,
            BannerImageUrl = dto.BannerImageUrl,
            OrganiserId = uid
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = ev.EventId }, ev);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EventDto dto)
    {
        if (!HttpContext.Session.IsOrganiser()) return StatusCode(403, new { message = "Access denied. Please sign in with the correct account and check your permissions." });
        if (!IsValid(dto)) return BadRequest(new { message = "Enter valid event details." });
        var uid = HttpContext.Session.GetUser()!.UserId;

        var ev = await _db.Events.FindAsync(id);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != uid) return StatusCode(403, new { message = "Access denied. Please sign in with the correct account and check your permissions." });   // cannot touch others' events

        ev.Name = dto.Name;
        ev.Description = dto.Description;
        ev.EventDate = dto.EventDate;
        ev.Location = dto.Location;
        ev.Distance = dto.Distance;
        ev.EventType = dto.EventType;
        ev.BannerImageUrl = dto.BannerImageUrl;
        await _db.SaveChangesAsync();
        return Ok(ev);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!HttpContext.Session.IsOrganiser()) return StatusCode(403, new { message = "Access denied. Please sign in with the correct account and check your permissions." });
        var uid = HttpContext.Session.GetUser()!.UserId;

        var ev = await _db.Events.FindAsync(id);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != uid) return StatusCode(403, new { message = "Access denied. Please sign in with the correct account and check your permissions." });

        if (await _db.Enrolments.AnyAsync(e => e.EventId == id))
            return Conflict(new { message = "Cannot delete an event that has enrolments." });

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }
    private static bool IsValid(EventDto dto) =>
        dto is not null &&
        !string.IsNullOrWhiteSpace(dto.Name) &&
        !string.IsNullOrWhiteSpace(dto.Location) &&
        dto.Distance > 0 &&
        new[] { "Run", "Walk", "Cycle" }.Contains(dto.EventType) &&
        dto.EventDate != default;

}