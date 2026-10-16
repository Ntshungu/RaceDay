using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models;

public class Category
{
    public int CategoryId { get; set; }

    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int EventId { get; set; }        // FK
    public Event? Event { get; set; }       // Navigation

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}