using HabitFlow.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitFlow.Data;

public class AppDbContext : DbContext
{
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<HabitLog> HabitLogs => Set<HabitLog>();
    public DbSet<UnlockedAchievement> UnlockedAchievements => Set<UnlockedAchievement>();

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
        modelBuilder.Entity<Habit>(e =>
        {
            e.Property(h => h.Name).HasMaxLength(100).IsRequired();
            // уже существующие привычки получат «каждый день»
            e.Property(h => h.Days).HasDefaultValue(WeekDays.All);
        });

        // одна отметка на привычку в день
        modelBuilder.Entity<HabitLog>()
            .HasIndex(l => new { l.HabitId, l.Date })
            .IsUnique();

        // каждое достижение можно получить только один раз
        modelBuilder.Entity<UnlockedAchievement>(e =>
        {
            e.Property(a => a.Key).HasMaxLength(50).IsRequired();
            e.HasIndex(a => a.Key).IsUnique();
        });
    }
}