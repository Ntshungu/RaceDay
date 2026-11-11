using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers;
public class AccountController : Controller
{
    private readonly RaceDayApiClient _api;
    public AccountController(RaceDayApiClient api) => _api = api;
    [HttpGet] public IActionResult Login() => View();
    [HttpPost] [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var result = await _api.Login(vm);
        if (!result.Ok) { ModelState.AddModelError("", result.Error); return View(vm); }
        return RedirectToAction("Index", "Home");
    }
    [HttpGet] public IActionResult Register() => View();
    [HttpPost] [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var result = await _api.Register(vm);
        if (!result.Ok) { ModelState.AddModelError("", result.Error); return View(vm); }
        // Registration creates the account; sign in to establish both API and MVC sessions.
        var login = await _api.Login(new LoginVm { Email = vm.Email, Password = vm.Password });
        if (!login.Ok)
        {
            TempData["Success"] = "Account created. Please log in to continue.";
            return RedirectToAction(nameof(Login));
        }
        TempData["Success"] = "Registration successful. Welcome to RaceDay!";
        return RedirectToAction("Index", "Home");
    }
    [HttpPost] [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() { await _api.Logout(); return RedirectToAction("Index", "Home"); }
}
