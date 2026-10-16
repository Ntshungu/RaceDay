using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    public CategoriesController(RaceDayDbContext db) => _db = db;

    [HttpGet("events/{eventId}/categories")]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == eventId))
            return NotFound();

        var categories = await _db.Categories
            .Where(c => c.EventId == eventId)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost("events/{eventId}/categories")]
    public async Task<IActionResult> Create(int eventId, [FromBody] CategoryDto dto)
    {
        if (!HttpContext.Session.IsOrganiser()) return Forbid();
        var uid = HttpContext.Session.GetUser()!.UserId;

        var ev = await _db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != uid) return Forbid();

        if (dto is null || string.IsNullOrWhiteSpace(dto.CategoryName))
            return BadRequest(new { message = "Category name is required." });

        if (await _db.Categories.AnyAsync(c =>
            c.EventId == eventId && c.CategoryName == dto.CategoryName.Trim()))
            return Conflict(new { message = "This category already exists for the event." });

        var cat = new Category
        {
            CategoryName = dto.CategoryName.Trim(),
            Description = dto.Description,
            EventId = eventId
        };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetForEvent), new { eventId }, cat);
    }

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
    {
        if (!HttpContext.Session.IsOrganiser()) return Forbid();
        var cat = await _db.Categories.Include(c => c.Event)
                                      .FirstOrDefaultAsync(c => c.CategoryId == id);
        if (cat is null) return NotFound();
        if (cat.Event!.OrganiserId != HttpContext.Session.GetUser()!.UserId) return Forbid();
        if (dto is null || string.IsNullOrWhiteSpace(dto.CategoryName))
            return BadRequest(new { message = "Category name is required." });

        if (await _db.Categories.AnyAsync(c =>
            c.EventId == cat.EventId && c.CategoryName == dto.CategoryName.Trim() &&
            c.CategoryId != id))
            return Conflict(new { message = "This category already exists for the event." });

        cat.CategoryName = dto.CategoryName.Trim();
        cat.Description = dto.Description;
        await _db.SaveChangesAsync();
        return Ok(cat);
    }

    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!HttpContext.Session.IsOrganiser()) return Forbid();
        var cat = await _db.Categories.Include(c => c.Event)
                                      .FirstOrDefaultAsync(c => c.CategoryId == id);
        if (cat is null) return NotFound();
        if (cat.Event!.OrganiserId != HttpContext.Session.GetUser()!.UserId) return Forbid();

        if (await _db.Enrolments.AnyAsync(e => e.CategoryId == id))
            return Conflict(new { message = "Cannot delete a category that has enrolments." });

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}