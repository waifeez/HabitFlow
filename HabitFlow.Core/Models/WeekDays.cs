namespace HabitFlow.Core.Models;

public static class WeekDays
{
    public const int Monday = 1;
    public const int Tuesday = 2;
    public const int Wednesday = 4;
    public const int Thursday = 8;
    public const int Friday = 16;
    public const int Saturday = 32;
    public const int Sunday = 64;
    public const int All = 127;

    private static readonly string[] ShortNames = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };

    // Бит дня недели для даты (понедельник = 1, воскресенье = 64)
    public static int FromDate(DateOnly date) => 1 << (((int)date.DayOfWeek + 6) % 7);

    public static bool IsScheduled(int mask, DateOnly date) => (mask & FromDate(date)) != 0;

    public static string ToText(int mask)
    {
        if (mask == All) return "Каждый день";
        var days = ShortNames.Where((_, i) => (mask & (1 << i)) != 0);
        return string.Join(", ", days);
    }
}