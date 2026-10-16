using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

/// <summary>Event management tests, all about Jarred.</summary>
public class EventTests
{
    private static EventDto JarredsEvent() => new(
        "Jarred's Comrades Warmup",
        "A 21km road run in Pietermaritzburg.",
        new DateTime(2025, 6, 15, 6, 0, 0),
        "Pietermaritzburg", 21.10m, "Run", null);

    [Fact]
    public async Task Jarred_AsOrganiser_CanCreateEvent()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_AsOrganiser_CanCreateEvent));
        db.Users.Add(new User
        {
            UserId = 1,
            FullName = "Jarred Naidoo",
            Email = "j@r.co.za",
            PasswordHash = "h",
            Role = "Organiser"
        });
        await db.SaveChangesAsync();

        var ctrl = new EventsController(db).WithSession(TestHelpers.JarredOrganiser());
        var result = await ctrl.Create(JarredsEvent());

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var ev = Assert.IsType<Event>(created.Value);
        Assert.Equal("Jarred's Comrades Warmup", ev.Name);
        Assert.Equal(1, ev.OrganiserId);
    }

    [Fact]
    public async Task Jarred_AsParticipant_IsForbidden_FromCreatingEvent()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_AsParticipant_IsForbidden_FromCreatingEvent));
        var ctrl = new EventsController(db).WithSession(TestHelpers.JarredParticipant());
        var result = await ctrl.Create(JarredsEvent());
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Jarred_CanViewEvents_WhenNotLoggedIn()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_CanViewEvents_WhenNotLoggedIn));
        db.Events.Add(new Event
        {
            EventId = 1,
            Name = "Comrades",
            EventDate = DateTime.UtcNow.AddDays(30),
            Location = "Durban",
            Distance = 89.9m,
            EventType = "Run",
            OrganiserId = 1
        });
        await db.SaveChangesAsync();

        var ctrl = new EventsController(db).WithSession(null);
        var result = await ctrl.GetAll(null, null);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task Jarred_CanUpdate_HisOwnEvent()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_CanUpdate_HisOwnEvent));
        db.Events.Add(new Event
        {
            EventId = 1,
            Name = "Old Name",
            EventDate = DateTime.UtcNow.AddDays(30),
            Location = "PMB",
            Distance = 21m,
            EventType = "Run",
            OrganiserId = 1
        });
        await db.SaveChangesAsync();

        var ctrl = new EventsController(db).WithSession(TestHelpers.JarredOrganiser());
        var result = await ctrl.Update(1, JarredsEvent());

        Assert.IsType<OkObjectResult>(result);
        var ev = await db.Events.FindAsync(1);
        Assert.Equal("Jarred's Comrades Warmup", ev!.Name);
    }

    [Fact]
    public async Task Jarred_CannotUpdate_SomeoneElsesEvent()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_CannotUpdate_SomeoneElsesEvent));
        db.Events.Add(new Event
        {
            EventId = 1,
            Name = "Not Jarred's",
            EventDate = DateTime.UtcNow,
            Location = "X",
            Distance = 5m,
            EventType = "Run",
            OrganiserId = 99
        });
        await db.SaveChangesAsync();

        var ctrl = new EventsController(db).WithSession(TestHelpers.JarredOrganiser());
        var result = await ctrl.Update(1, JarredsEvent());
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Jarred_CanDelete_HisOwnEvent()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_CanDelete_HisOwnEvent));
        db.Events.Add(new Event
        {
            EventId = 1,
            Name = "Jarred's Race",
            EventDate = DateTime.UtcNow,
            Location = "PMB",
            Distance = 10m,
            EventType = "Walk",
            OrganiserId = 1
        });
        await db.SaveChangesAsync();

        var ctrl = new EventsController(db).WithSession(TestHelpers.JarredOrganiser());
        var result = await ctrl.Delete(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await db.Events.FindAsync(1));
    }
}