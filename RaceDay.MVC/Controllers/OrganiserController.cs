using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers;
public class OrganiserController : Controller
{
    private readonly RaceDayApiClient _api;
    public OrganiserController(RaceDayApiClient api) => _api = api;
    private bool Allowed() => _api.CurrentUser()?.Role == "Organiser";
    private IActionResult Denied() => RedirectToAction("Login", "Account");
    public async Task<IActionResult> Dashboard()
    {
        if (!Allowed()) return Denied();
        var r = await _api.Get<List<EventVm>>("api/events/my-events");
        ViewBag.Error = r.Error; return View(r.Data ?? new());
    }
    [HttpGet] public IActionResult Create() => Allowed() ? View(new EventFormVm()) : Denied();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormVm vm)
    {
        if (!Allowed()) return Denied();
        if (!ModelState.IsValid) return View(vm);
        if (vm.BannerImage != null && vm.BannerImage.Length > 0)
        {
            var up = await _api.UploadImage("api/images/event-banner", vm.BannerImage);
            if (!up.Ok) { ModelState.AddModelError("", up.Error); return View(vm); }
            vm.BannerImageUrl = up.Url;
        }
        var r = await _api.Post("api/events", new { vm.Name, vm.Description, vm.EventDate, vm.Location, vm.Distance, vm.EventType, vm.BannerImageUrl });
        if (!r.Ok) { ModelState.AddModelError("", r.Error); return View(vm); }
        TempData["Success"] = "Event created."; return RedirectToAction(nameof(Dashboard));
    }
    [HttpGet] public async Task<IActionResult> Edit(int id)
    {
        if (!Allowed()) return Denied();
        var r = await _api.Get<EventVm>($"api/events/{id}");
        if (!r.Ok || r.Data == null) return RedirectToAction(nameof(Dashboard));
        var e = r.Data;
        return View(new EventFormVm { EventId=e.EventId, Name=e.Name, Description=e.Description, EventDate=e.EventDate, Location=e.Location, Distance=e.Distance, EventType=e.EventType, BannerImageUrl=e.BannerImageUrl });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EventFormVm vm)
    {
        if (!Allowed()) return Denied();
        if (vm.BannerImage != null && vm.BannerImage.Length > 0)
        {
            var up = await _api.UploadImage("api/images/event-banner", vm.BannerImage);
            if (!up.Ok) { ModelState.AddModelError("", up.Error); return View(vm); }
            vm.BannerImageUrl = up.Url;
        }
        var r = await _api.Put($"api/events/{vm.EventId}", new { vm.Name, vm.Description, vm.EventDate, vm.Location, vm.Distance, vm.EventType, vm.BannerImageUrl });
        if (!r.Ok) { ModelState.AddModelError("", r.Error); return View(vm); }
        TempData["Success"] = "Event updated."; return RedirectToAction(nameof(Dashboard));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!Allowed()) return Denied();
        var r = await _api.Delete($"api/events/{id}");
        TempData[r.Ok ? "Success" : "Error"] = r.Ok ? "Event deleted." : r.Error;
        return RedirectToAction(nameof(Dashboard));
    }
    public async Task<IActionResult> Categories(int id)
    {
        if (!Allowed()) return Denied();
        var r = await _api.Get<List<CategoryVm>>($"api/events/{id}/categories");
        ViewBag.EventId = id; ViewBag.Error = r.Error; return View(r.Data ?? new());
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(int id, CategoryFormVm vm)
    {
        if (!Allowed()) return Denied();
        var r = await _api.Post($"api/events/{id}/categories", vm);
        TempData[r.Ok ? "Success" : "Error"] = r.Ok ? "Category added." : r.Error;
        return RedirectToAction(nameof(Categories), new { id });
    }
    public async Task<IActionResult> Enrolments(int id)
    {
        if (!Allowed()) return Denied();
        var r = await _api.Get<List<EnrolmentVm>>($"api/events/{id}/enrolments");
        ViewBag.EventId=id; ViewBag.Error=r.Error; return View(r.Data ?? new());
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CaptureResult(ResultFormVm vm)
    {
        if (!Allowed()) return Denied();
        if (!TimeSpan.TryParse(vm.FinishTime, out var time)) { TempData["Error"]="Enter a valid finish time (hh:mm:ss)."; return RedirectToAction(nameof(Enrolments), new { id = Request.Form["eventId"] }); }
        var r = await _api.Post("api/results", new { vm.EnrolmentId, FinishTime=time, vm.FinishingPosition });
        TempData[r.Ok ? "Success" : "Error"] = r.Ok ? "Result captured." : r.Error;
        return RedirectToAction(nameof(Enrolments), new { id = Request.Form["eventId"] });
    }
}
