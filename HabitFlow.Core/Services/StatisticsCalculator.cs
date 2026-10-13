using HabitFlow.Core.Models;

namespace HabitFlow.Core.Services;

// Данные одной привычки для расчёта статистики
public record HabitStatsInput(int Days, DateOnly CreatedOn, IReadOnlyCollection<DateOnly> Dates);

public static class StatisticsCalculator
{
    // Сколько всего отметок сделано в каждый день (по всем привычкам)
    public static Dictionary<DateOnly, int> CountsByDay(IEnumerable<HabitStatsInput> habits)
    {
        var result = new Dictionary<DateOnly, int>();
        foreach (var date in habits.SelectMany(h => h.Dates))
            result[date] = result.GetValueOrDefault(date) + 1;
        return result;
    }

    // Самая длинная серия за всё время (по плановым дням привычки)
    public static int LongestStreak(IReadOnlyCollection<DateOnly> dates, int daysMask)
    {
        if (dates.Count == 0 || daysMask == 0) return 0;

        var set = dates.ToHashSet();
        var first = set.Min();
        var last = set.Max();
        var best = 0;
        var current = 0;

        for (var day = first; day <= last; day = day.AddDays(1))
        {
            if (!WeekDays.IsScheduled(daysMask, day)) continue;

            if (set.Contains(day))
            {
                current++;
                best = Math.Max(best, current);
            }
            else
            {
                current = 0;
            }
        }

        return best;
    }

    // Процент выполнения плановых отметок за последние дни.
    // Сегодняшний день не считается пропуском, пока отметки ещё нет.
    public static int CompletionPercent(IEnumerable<HabitStatsInput> habits, DateOnly today, int windowDays = 30)
    {
        var list = habits.ToList();
        var scheduled = 0;
        var done = 0;

        for (var i = 0; i < windowDays; i++)
        {
            var day = today.AddDays(-i);
            foreach (var h in list)
            {
                if (day < h.CreatedOn || !WeekDays.IsScheduled(h.Days, day)) continue;

                var isDone = h.Dates.Contains(day);
                if (day == today && !isDone) continue;

                scheduled++;
                if (isDone) done++;
            }
        }

        return scheduled == 0 ? 0 : (int)Math.Round(done * 100.0 / scheduled);
    }
}