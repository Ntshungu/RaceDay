using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // --- Uniqueness constraints ---
        mb.Entity<User>().HasIndex(u => u.Email).IsUnique();
        mb.Entity<Enrolment>().HasIndex(e => new { e.UserId, e.EventId }).IsUnique();
        mb.Entity<Result>().HasIndex(r => r.EnrolmentId).IsUnique();

        // --- Event → Organiser (User). No cascade, so deleting a user doesn't nuke events. ---
        mb.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(u => u.Events)
            .HasForeignKey(e => e.OrganiserId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Category → Event. Cascade is fine here (only one path). ---
        mb.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Enrolment → User. NO cascade (breaks the cycle). ---
        mb.Entity<Enrolment>()
            .HasOne(e => e.User)
            .WithMany(u => u.Enrolments)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Enrolment → Event. NO cascade. ---
        mb.Entity<Enrolment>()
            .HasOne(e => e.Event)
            .WithMany(ev => ev.Enrolments)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Enrolment → Category. NO cascade. ---
        mb.Entity<Enrolment>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Result → Enrolment. Cascade is fine (leaf table). ---
        mb.Entity<Result>()
            .HasOne(r => r.Enrolment)
            .WithOne(e => e.Result)
            .HasForeignKey<Result>(r => r.EnrolmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}