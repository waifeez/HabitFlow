namespace HabitFlow.App.ViewModels;

public class AchievementItemViewModel
{
    public string Emoji { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsUnlocked { get; init; }

    public string DisplayEmoji => IsUnlocked ? Emoji : "🔒";
}