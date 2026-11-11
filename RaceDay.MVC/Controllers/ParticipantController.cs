using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers;
public class ParticipantController : Controller
{
    private readonly RaceDayApiClient _api;
    public ParticipantController(RaceDayApiClient api) => _api = api;
    private bool Allowed() => _api.CurrentUser()?.Role == "Participant";
    public async Task<IActionResult> Dashboard()
    {
        if (!Allowed()) return RedirectToAction("Login","Account");
        var r=await _api.Get<List<EnrolmentVm>>("api/enrolments/my-enrolments");
        ViewBag.Error=r.Error; return View(r.Data ?? new());
    }
    public async Task<IActionResult> Results()
    {
        if (!Allowed()) return RedirectToAction("Login","Account");
        var r=await _api.Get<List<ResultVm>>("api/results/my-results");
        ViewBag.Error=r.Error; return View(r.Data ?? new());
    }
}
