using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

/// <summary>Jarred, as Participant, enrols in events.</summary>
public class EnrolmentTests
{
    private static (RaceDay.API.Data.RaceDayDbContext db, int eventId, int catId) Seed(string dbName)
    {
        var db = TestHelpers.NewDb(dbName);
        db.Events.Add(new Event
        {
            EventId = 1,
            Name = "Soweto Marathon",
            EventDate = DateTime.UtcNow.AddDays(20),
            Location = "Soweto",
            Distance = 42m,
            EventType = "Run",
            OrganiserId = 99
        });
        db.Categories.Add(new Category { CategoryId = 1, CategoryName = "Senior Male", EventId = 1 });
        db.SaveChanges();
        return (db, 1, 1);
    }

    [Fact]
    public async Task Jarred_AsParticipant_CanEnrol()
    {
        var (db, eventId, catId) = Seed(nameof(Jarred_AsParticipant_CanEnrol));
        var ctrl = new EnrolmentsController(db).WithSession(TestHelpers.JarredParticipant());

        var result = await ctrl.Enrol(new EnrolmentDto(eventId, catId));

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var enr = Assert.IsType<Enrolment>(created.Value);
        Assert.Equal(2, enr.UserId);       // Jarred Participant id
        Assert.Equal(1, enr.EventId);
        Assert.Equal(1, enr.CategoryId);
    }

    [Fact]
    public async Task Jarred_CannotEnrolTwice_InSameEvent()
    {
        var (db, eventId, catId) = Seed(nameof(Jarred_CannotEnrolTwice_InSameEvent));
        var ctrl = new EnrolmentsController(db).WithSession(TestHelpers.JarredParticipant());

        await ctrl.Enrol(new EnrolmentDto(eventId, catId));
        var second = await ctrl.Enrol(new EnrolmentDto(eventId, catId));

        Assert.IsType<ConflictObjectResult>(second);
    }

    [Fact]
    public async Task Jarred_AsOrganiser_CannotEnrol()
    {
        var (db, eventId, catId) = Seed(nameof(Jarred_AsOrganiser_CannotEnrol));
        var ctrl = new EnrolmentsController(db).WithSession(TestHelpers.JarredOrganiser());
        var result = await ctrl.Enrol(new EnrolmentDto(eventId, catId));
        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, forbidden.StatusCode);
    }

    [Fact]
    public async Task Jarred_CannotEnrolWithMismatchedCategory()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_CannotEnrolWithMismatchedCategory));
        db.Events.Add(new Event
        {
            EventId = 1,
            Name = "E1",
            EventDate = DateTime.UtcNow,
            Location = "L",
            Distance = 5,
            EventType = "Run",
            OrganiserId = 99
        });
        db.Events.Add(new Event
        {
            EventId = 2,
            Name = "E2",
            EventDate = DateTime.UtcNow,
            Location = "L",
            Distance = 5,
            EventType = "Run",
            OrganiserId = 99
        });
        db.Categories.Add(new Category { CategoryId = 5, CategoryName = "C", EventId = 2 });
        await db.SaveChangesAsync();

        var ctrl = new EnrolmentsController(db).WithSession(TestHelpers.JarredParticipant());
        var result = await ctrl.Enrol(new EnrolmentDto(1, 5));   // cat belongs to event 2
        Assert.IsType<BadRequestObjectResult>(result);
    }
}