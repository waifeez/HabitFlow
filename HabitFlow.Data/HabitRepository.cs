using HabitFlow.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitFlow.Data;

public class HabitRepository
{
    public async Task<List<Habit>> GetAllAsync()
    {
        using var db = AppDbContext.Create();
        return await db.Habits
            .Include(h => h.Logs)
            .AsNoTracking()
            .OrderBy(h => h.Id)
            .ToListAsync();
    }

    public async Task AddAsync(string name, string emoji, int days = WeekDays.All)
    {
        using var db = AppDbContext.Create();
        db.Habits.Add(new Habit { Name = name, Emoji = emoji, Days = days });
        await db.SaveChangesAsync();
    }

    public async Task ToggleAsync(int habitId, DateOnly date)
    {
        using var db = AppDbContext.Create();
        var log = await db.HabitLogs
            .FirstOrDefaultAsync(l => l.HabitId == habitId && l.Date == date);

        if (log is null)
            db.HabitLogs.Add(new HabitLog { HabitId = habitId, Date = date });
        else
            db.HabitLogs.Remove(log);

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int habitId)
    {
        using var db = AppDbContext.Create();
        var habit = await db.Habits.FindAsync(habitId);
        if (habit is null) return;

        db.Habits.Remove(habit);
        await db.SaveChangesAsync();
    }

    public async Task<HashSet<string>> GetUnlockedKeysAsync()
    {
        using var db = AppDbContext.Create();
        var keys = await db.UnlockedAchievements.Select(a => a.Key).ToListAsync();
        return keys.ToHashSet();
    }

    public async Task UnlockAsync(IEnumerable<string> keys)
    {
        using var db = AppDbContext.Create();
        foreach (var key in keys)
            db.UnlockedAchievements.Add(new UnlockedAchievement { Key = key });

        await db.SaveChangesAsync();
    }
}