using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public EnrolmentsController(RaceDayDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Enrol([FromBody] EnrolmentDto dto)
    {
        if (!HttpContext.Session.IsParticipant()) return StatusCode(403, new { message = "Access denied. Please sign in with the correct account and check your permissions." });
        var uid = HttpContext.Session.GetUser()!.UserId;

        if (!await _db.Events.AnyAsync(e => e.EventId == dto.EventId))
            return NotFound(new { message = "Event not found." });

        if (!await _db.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId && c.EventId == dto.EventId))
            return BadRequest(new { message = "Category does not belong to this event." });

        if (await _db.Enrolments.AnyAsync(e => e.UserId == uid && e.EventId == dto.EventId))
            return Conflict(new { message = "You are already enrolled in this event." });

        var enr = new Enrolment
        {
            UserId = uid,
            EventId = dto.EventId,
            CategoryId = dto.CategoryId
        };
        _db.Enrolments.Add(enr);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(MyEnrolments), new { id = enr.EnrolmentId }, enr);
    }

    [HttpGet("my-enrolments")]
    public async Task<IActionResult> MyEnrolments()
    {
        if (!HttpContext.Session.IsParticipant()) return StatusCode(403, new { message = "Access denied. Please sign in with the correct account and check your permissions." });
        var uid = HttpContext.Session.GetUser()!.UserId;

        var list = await _db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Include(e => e.Result)
            .Where(e => e.UserId == uid)
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("/api/events/{eventId}/enrolments")]
    public async Task<IActionResult> EventEnrolments(int eventId)
    {
        if (!HttpContext.Session.IsOrganiser()) return StatusCode(403, new { message = "Access denied. Please sign in with the correct account and check your permissions." });
        var uid = HttpContext.Session.GetUser()!.UserId;

        var ev = await _db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != uid) return StatusCode(403, new { message = "Access denied. Please sign in with the correct account and check your permissions." });

        var list = await _db.Enrolments
            .Include(e => e.User)
            .Include(e => e.Category)
            .Include(e => e.Result)
            .Where(e => e.EventId == eventId)
            .ToListAsync();
        return Ok(list);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(int id)
    {
        if (!HttpContext.Session.IsParticipant()) return StatusCode(403, new { message = "Access denied. Please sign in with the correct account and check your permissions." });
        var uid = HttpContext.Session.GetUser()!.UserId;

        var enr = await _db.Enrolments.FindAsync(id);
        if (enr is null) return NotFound();
        if (enr.UserId != uid) return StatusCode(403, new { message = "Access denied. Please sign in with the correct account and check your permissions." });

        _db.Enrolments.Remove(enr);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}