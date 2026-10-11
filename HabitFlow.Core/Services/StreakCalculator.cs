namespace HabitFlow.Core.Services;

public static class StreakCalculator
{
    public static int Current(IEnumerable<DateOnly> dates, DateOnly today)
    {
        var set = dates.ToHashSet();
        var day = set.Contains(today) ? today : today.AddDays(-1);
        var count = 0;
        while (set.Contains(day))
        {
            count++;
            day = day.AddDays(-1);
        }
        return count;
    }
}