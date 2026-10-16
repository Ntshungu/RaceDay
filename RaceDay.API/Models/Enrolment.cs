namespace RaceDay.API.Models;

public class Enrolment
{
    public int EnrolmentId { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Confirmed"; // Pending | Confirmed | Cancelled

    public Result? Result { get; set; }   // 1-to-1 with Result
}