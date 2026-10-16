using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using Xunit;

namespace RaceDay.Tests;

/// <summary>Authentication tests, narrated around Jarred.</summary>
public class AuthTests
{
    [Fact]
    public async Task Jarred_CanRegisterAsOrganiser()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_CanRegisterAsOrganiser));
        var ctrl = new AuthController(db);

        var dto = new RegisterDto("Jarred Naidoo", "jarred@raceday.co.za",
                                  "Password123!", "Organiser", "0821234567");
        var result = await ctrl.Register(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var body = Assert.IsType<UserResponse>(created.Value);
        Assert.Equal("jarred@raceday.co.za", body.Email);
        Assert.Equal("Organiser", body.Role);
        Assert.Single(db.Users);
        // Password is stored hashed, not plaintext
        Assert.NotEqual("Password123!", db.Users.First().PasswordHash);
    }

    [Fact]
    public async Task Jarred_CannotRegisterWithDuplicateEmail()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_CannotRegisterWithDuplicateEmail));
        var ctrl = new AuthController(db);

        await ctrl.Register(new RegisterDto("Jarred Naidoo", "jarred@raceday.co.za",
                                            "Password123!", "Organiser", null));
        var second = await ctrl.Register(new RegisterDto("Jarred Clone", "jarred@raceday.co.za",
                                                         "Password456!", "Participant", null));

        Assert.IsType<ConflictObjectResult>(second);
    }

    [Fact]
    public async Task Jarred_CannotRegisterWithInvalidRole()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_CannotRegisterWithInvalidRole));
        var ctrl = new AuthController(db);

        var result = await ctrl.Register(new RegisterDto("Jarred", "j@r.co.za",
                                                         "Password123!", "Admin", null));
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Jarred_CanLogin_WithCorrectCredentials()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_CanLogin_WithCorrectCredentials));
        await new AuthController(db).Register(
            new RegisterDto("Jarred Naidoo", "jarred@raceday.co.za",
                            "Password123!", "Organiser", null));

        var ctrl = new AuthController(db).WithSession(null);
        var result = await ctrl.Login(new LoginDto("jarred@raceday.co.za", "Password123!"));

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<UserResponse>(ok.Value);
        Assert.Equal("Organiser", body.Role);
    }

    [Fact]
    public async Task Jarred_CannotLogin_WithWrongPassword()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_CannotLogin_WithWrongPassword));
        await new AuthController(db).Register(
            new RegisterDto("Jarred Naidoo", "jarred@raceday.co.za",
                            "Password123!", "Organiser", null));

        var ctrl = new AuthController(db).WithSession(null);
        var result = await ctrl.Login(new LoginDto("jarred@raceday.co.za", "WrongPass"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}