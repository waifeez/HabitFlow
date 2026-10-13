namespace HabitFlow.Core.Models;

public class UnlockedAchievement
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public DateTime UnlockedAt { get; set; } = DateTime.Now;
}