using HabitFlow.Core.Models;

namespace HabitFlow.Core.Services;

public static class StreakCalculator
{
    // Серия считается по плановым дням привычки: дни, в которые привычку
    // выполнять не нужно, пропускаются и серию не прерывают.
    // Если сегодня плановый день и отметки ещё нет, серия не обрывается до конца дня.
    public static int Current(IEnumerable<DateOnly> dates, DateOnly today, int daysMask = WeekDays.All)
    {
        if (daysMask == 0) return 0;

        var set = dates.ToHashSet();
        if (set.Count == 0) return 0;

        var earliest = set.Min();
        var count = 0;
        var day = today;

        while (day >= earliest)
        {
            if (WeekDays.IsScheduled(daysMask, day))
            {
                if (set.Contains(day)) count++;
                else if (day != today) break;
            }
            day = day.AddDays(-1);
        }

        return count;
    }
}