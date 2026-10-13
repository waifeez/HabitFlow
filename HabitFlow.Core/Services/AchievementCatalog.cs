namespace HabitFlow.Core.Services;

public record AchievementDefinition(string Key, string Emoji, string Title, string Description);

public record AchievementStats(int TotalChecks, int HabitCount, int BestStreak);

public static class AchievementCatalog
{
    public static IReadOnlyList<AchievementDefinition> All { get; } = new List<AchievementDefinition>
    {
        new("first_check", "🌱", "Первая отметка", "Отметьте выполнение любой привычки"),
        new("habits_5", "📋", "Коллекционер", "Заведите 5 привычек"),
        new("streak_3", "🔥", "Три дня подряд", "Серия в 3 дня у любой привычки"),
        new("streak_7", "⚡", "Неделя без пропусков", "Серия в 7 дней у любой привычки"),
        new("streak_30", "🏆", "Железная воля", "Серия в 30 дней у любой привычки"),
        new("checks_50", "💪", "Полсотни", "Сделайте 50 отметок в сумме"),
    };

    private static readonly Dictionary<string, Func<AchievementStats, bool>> Rules = new()
    {
        ["first_check"] = s => s.TotalChecks >= 1,
        ["habits_5"] = s => s.HabitCount >= 5,
        ["streak_3"] = s => s.BestStreak >= 3,
        ["streak_7"] = s => s.BestStreak >= 7,
        ["streak_30"] = s => s.BestStreak >= 30,
        ["checks_50"] = s => s.TotalChecks >= 50,
    };

    // Ключи достижений, условия которых выполнены при данной статистике
    public static IEnumerable<string> Earned(AchievementStats stats) =>
        All.Where(a => Rules[a.Key](stats)).Select(a => a.Key);
}