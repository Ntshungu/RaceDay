using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers;
public class EventsController : Controller
{
    private readonly RaceDayApiClient _api;
    public EventsController(RaceDayApiClient api) => _api = api;
    public async Task<IActionResult> Index()
    {
        var r = await _api.Get<List<EventVm>>("api/events");
        ViewBag.Error = r.Error; return View(r.Data ?? new());
    }
    public async Task<IActionResult> Details(int id)
    {
        var r = await _api.Get<EventVm>($"api/events/{id}");
        if (!r.Ok) { TempData["Error"] = r.Error; return RedirectToAction(nameof(Index)); }
        return View(r.Data);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        if (_api.CurrentUser()?.Role != "Participant") return RedirectToAction("Login", "Account");
        var r = await _api.Post("api/enrolments", new { EventId = eventId, CategoryId = categoryId });
        TempData[r.Ok ? "Success" : "Error"] = r.Ok ? "You have successfully entered this event." : r.Error;
        return RedirectToAction(nameof(Details), new { id = eventId });
    }
}
