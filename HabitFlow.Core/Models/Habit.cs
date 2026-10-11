namespace HabitFlow.Core.Models;

public class Habit
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Emoji { get; set; } = "✅";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<HabitLog> Logs { get; set; } = new();
}