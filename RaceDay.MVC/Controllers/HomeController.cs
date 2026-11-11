using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.Models;
namespace RaceDay.MVC.Controllers;
public class HomeController : Controller
{
    private readonly RaceDayApiClient _api;
    public HomeController(RaceDayApiClient api) => _api = api;
    public async Task<IActionResult> Index()
    {
        var result = await _api.Get<List<EventVm>>("api/events");
        ViewBag.Error = result.Error;
        return View(result.Data ?? new List<EventVm>());
    }
    public IActionResult Error() => View();
}
