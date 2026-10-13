using HabitFlow.Core.Models;

namespace HabitFlow.App.ViewModels;

public class HabitItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Emoji { get; init; } = string.Empty;
    public int Streak { get; init; }
    public bool IsDoneToday { get; init; }
    public int Days { get; init; } = WeekDays.All;
    public bool IsScheduledToday { get; init; } = true;

    public string StreakText => Streak > 0 ? $"🔥 {Streak} дн." : "—";
    public string ScheduleText => WeekDays.ToText(Days);
}