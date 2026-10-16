using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class Event
{
    public int EventId { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime EventDate { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public decimal Distance { get; set; }

    [Required]
    public string EventType { get; set; } = "Run"; // Run | Walk | Cycle

    public string? BannerImageUrl { get; set; }

    public int OrganiserId { get; set; }               // FK
    public User? Organiser { get; set; }               // Navigation

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}