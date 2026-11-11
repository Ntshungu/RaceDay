using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers;
public class ProfileController : Controller
{
    private readonly RaceDayApiClient _api;
    public ProfileController(RaceDayApiClient api) => _api=api;
    [HttpGet] public async Task<IActionResult> Index()
    {
        if (_api.CurrentUser()==null) return RedirectToAction("Login","Account");
        var r=await _api.Get<UserResponse>("api/profile");
        if (!r.Ok || r.Data==null) { TempData["Error"]=r.Error; return RedirectToAction("Index","Home"); }
        return View(new ProfileVm { FullName=r.Data.FullName, PhoneNumber=r.Data.PhoneNumber, ProfilePictureUrl=r.Data.ProfilePictureUrl });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileVm vm)
    {
        if (_api.CurrentUser()==null) return RedirectToAction("Login","Account");
        if (vm.ProfilePicture!=null && vm.ProfilePicture.Length>0)
        {
            var up=await _api.UploadImage("api/images/profile-picture",vm.ProfilePicture);
            if (!up.Ok) { ModelState.AddModelError("",up.Error); return View(vm); }
            vm.ProfilePictureUrl=up.Url;
        }
        var r=await _api.Put("api/profile",new { vm.FullName, vm.PhoneNumber, vm.ProfilePictureUrl });
        if (!r.Ok) { ModelState.AddModelError("",r.Error); return View(vm); }
        TempData["Success"]="Profile updated."; return RedirectToAction(nameof(Index));
    }
}
