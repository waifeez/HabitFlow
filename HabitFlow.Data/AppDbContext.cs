using HabitFlow.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitFlow.Data;

public class AppDbContext : DbContext
{
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<HabitLog> HabitLogs => Set<HabitLog>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // База лежит в %LocalAppData%\HabitFlow\habitflow.db
    public static AppDbContext Create()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HabitFlow");
        Directory.CreateDirectory(dir);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={Path.Combine(dir, "habitflow.db")}")
            .Options;

        return new AppDbContext(options);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Habit>()
            .Property(h => h.Name)
            .HasMaxLength(100)
            .IsRequired();

        // одна отметка на привычку в день
        modelBuilder.Entity<HabitLog>()
            .HasIndex(l => new { l.HabitId, l.Date })
            .IsUnique();
    }
}