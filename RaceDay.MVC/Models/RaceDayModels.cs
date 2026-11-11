
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace RaceDay.MVC.Models;

public class UserResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public class LoginVm
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RegisterVm
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "Participant";
    public string? PhoneNumber { get; set; }
}

public class EventVm
{
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal Distance { get; set; }
    public string EventType { get; set; } = "Run";
    public string? BannerImageUrl { get; set; }
    public int OrganiserId { get; set; }

    public List<CategoryVm> Categories { get; set; } = new();
    public List<EnrolmentVm> Enrolments { get; set; } = new();
}

public class EventFormVm
{
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30);
    public string Location { get; set; } = "";
    public decimal Distance { get; set; }
    public string EventType { get; set; } = "Run";
    public string? BannerImageUrl { get; set; }
    public IFormFile? BannerImage { get; set; }
}

public class CategoryVm
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string CategoryName { get; set; } = "";
    public string? Description { get; set; }
}

public class CategoryFormVm
{
    public string CategoryName { get; set; } = "";
    public string? Description { get; set; }
}

public class EnrolmentVm
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public string Status { get; set; } = "Confirmed";

    public EventVm? Event { get; set; }
    public CategoryVm? Category { get; set; }
    public UserResponse? User { get; set; }
    public ResultVm? Result { get; set; }
}

public class ResultVm
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int FinishingPosition { get; set; }
    public string? EventName { get; set; }
    public DateTime EventDate { get; set; }
    public string? Category { get; set; }
    public int TotalFinishers { get; set; }
}

public class ResultFormVm
{
    public int EnrolmentId { get; set; }
    public string FinishTime { get; set; } = "00:45:00";
    public int FinishingPosition { get; set; } = 1;
}

public class ProfileVm
{
    public string FullName { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public IFormFile? ProfilePicture { get; set; }
}

public class ApiMessage
{
    public string? Message { get; set; }
}
