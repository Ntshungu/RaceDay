using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

/// <summary>Jarred captures and views race results.</summary>
public class ResultTests
{
    [Fact]
    public async Task Jarred_AsOrganiser_CanCaptureResult()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_AsOrganiser_CanCaptureResult));
        db.Events.Add(new Event
        {
            EventId = 1,
            Name = "Comrades",
            EventDate = DateTime.UtcNow,
            Location = "Durban",
            Distance = 89.9m,
            EventType = "Run",
            OrganiserId = 1
        });
        db.Enrolments.Add(new Enrolment { EnrolmentId = 1, UserId = 2, EventId = 1, CategoryId = 1 });
        await db.SaveChangesAsync();

        var ctrl = new ResultsController(db).WithSession(TestHelpers.JarredOrganiser());
        var result = await ctrl.Capture(new ResultDto(1, TimeSpan.FromHours(5.5), 47));

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Single(db.Results);
    }

    [Fact]
    public async Task Jarred_AsOrganiser_CannotCaptureResult_ForAnotherOrganisersEvent()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_AsOrganiser_CannotCaptureResult_ForAnotherOrganisersEvent));
        db.Events.Add(new Event
        {
            EventId = 1,
            Name = "Other",
            EventDate = DateTime.UtcNow,
            Location = "X",
            Distance = 5m,
            EventType = "Run",
            OrganiserId = 99
        });
        db.Enrolments.Add(new Enrolment { EnrolmentId = 1, UserId = 2, EventId = 1, CategoryId = 1 });
        await db.SaveChangesAsync();

        var ctrl = new ResultsController(db).WithSession(TestHelpers.JarredOrganiser());
        var result = await ctrl.Capture(new ResultDto(1, TimeSpan.FromHours(1), 1));
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Jarred_AsParticipant_CanViewOwnResults()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_AsParticipant_CanViewOwnResults));
        db.Events.Add(new Event
        {
            EventId = 1,
            Name = "Comrades",
            EventDate = DateTime.UtcNow,
            Location = "Durban",
            Distance = 89.9m,
            EventType = "Run",
            OrganiserId = 1
        });
        db.Categories.Add(new Category { CategoryId = 1, CategoryName = "Senior", EventId = 1 });
        db.Enrolments.Add(new Enrolment { EnrolmentId = 1, UserId = 2, EventId = 1, CategoryId = 1 });
        db.Results.Add(new Result
        {
            ResultId = 1,
            EnrolmentId = 1,
            FinishTime = TimeSpan.FromHours(5.5),
            FinishingPosition = 47
        });
        await db.SaveChangesAsync();

        var ctrl = new ResultsController(db).WithSession(TestHelpers.JarredParticipant());
        var result = await ctrl.MyResults();
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Jarred_AsOrganiser_CannotCaptureDuplicateResult()
    {
        var db = TestHelpers.NewDb(nameof(Jarred_AsOrganiser_CannotCaptureDuplicateResult));
        db.Events.Add(new Event
        {
            EventId = 1,
            Name = "Comrades",
            EventDate = DateTime.UtcNow,
            Location = "D",
            Distance = 89.9m,
            EventType = "Run",
            OrganiserId = 1
        });
        db.Enrolments.Add(new Enrolment { EnrolmentId = 1, UserId = 2, EventId = 1, CategoryId = 1 });
        db.Results.Add(new Result
        {
            ResultId = 1,
            EnrolmentId = 1,
            FinishTime = TimeSpan.FromHours(5),
            FinishingPosition = 10
        });
        await db.SaveChangesAsync();

        var ctrl = new ResultsController(db).WithSession(TestHelpers.JarredOrganiser());
        var result = await ctrl.Capture(new ResultDto(1, TimeSpan.FromHours(4), 5));
        Assert.IsType<ConflictObjectResult>(result);
    }
}