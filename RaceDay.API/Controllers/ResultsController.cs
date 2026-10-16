using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public ResultsController(RaceDayDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Capture([FromBody] ResultDto dto)
    {
        if (!HttpContext.Session.IsOrganiser()) return Forbid();
        if (dto is null || dto.FinishTime <= TimeSpan.Zero || dto.FinishingPosition <= 0)
            return BadRequest(new { message = "Finish time and finishing position must be positive." });
        var uid = HttpContext.Session.GetUser()!.UserId;

        var enr = await _db.Enrolments
            .Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentId == dto.EnrolmentId);
        if (enr is null) return NotFound();
        if (enr.Event!.OrganiserId != uid) return Forbid();

        if (await _db.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId))
            return Conflict(new { message = "Result already captured for this enrolment." });

        var result = new Result
        {
            EnrolmentId = dto.EnrolmentId,
            FinishTime = dto.FinishTime,
            FinishingPosition = dto.FinishingPosition
        };
        _db.Results.Add(result);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(EventResults), new { eventId = enr.EventId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ResultDto dto)
    {
        if (!HttpContext.Session.IsOrganiser()) return Forbid();
        if (dto is null || dto.FinishTime <= TimeSpan.Zero || dto.FinishingPosition <= 0)
            return BadRequest(new { message = "Finish time and finishing position must be positive." });
        var uid = HttpContext.Session.GetUser()!.UserId;

        var result = await _db.Results
            .Include(r => r.Enrolment)!.ThenInclude(e => e!.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);
        if (result is null) return NotFound();
        if (result.Enrolment!.Event!.OrganiserId != uid) return Forbid();

        result.FinishTime = dto.FinishTime;
        result.FinishingPosition = dto.FinishingPosition;
        await _db.SaveChangesAsync();
        return Ok(result);
    }

    [HttpGet("my-results")]
    public async Task<IActionResult> MyResults()
    {
        if (!HttpContext.Session.IsParticipant()) return Forbid();
        var uid = HttpContext.Session.GetUser()!.UserId;

        var list = await _db.Results
            .Include(r => r.Enrolment)!.ThenInclude(e => e!.Event)
            .Include(r => r.Enrolment)!.ThenInclude(e => e!.Category)
            .Where(r => r.Enrolment!.UserId == uid)
            .Select(r => new
            {
                r.ResultId,
                EventName = r.Enrolment!.Event!.Name,
                EventDate = r.Enrolment.Event.EventDate,
                Category = r.Enrolment.Category!.CategoryName,
                r.FinishTime,
                r.FinishingPosition,
                TotalFinishers = _db.Results.Count(x => x.Enrolment!.EventId == r.Enrolment.EventId)
            })
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("/api/events/{eventId}/results")]
    public async Task<IActionResult> EventResults(int eventId)
    {
        if (!HttpContext.Session.IsOrganiser()) return Forbid();
        var uid = HttpContext.Session.GetUser()!.UserId;

        var ev = await _db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != uid) return Forbid();

        var list = await _db.Results
            .Include(r => r.Enrolment)!.ThenInclude(e => e!.User)
            .Where(r => r.Enrolment!.EventId == eventId)
            .OrderBy(r => r.FinishingPosition)
            .ToListAsync();
        return Ok(list);
    }
}