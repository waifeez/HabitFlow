namespace HabitFlow.Core.Models;

public class HabitLog
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public Habit Habit { get; set; } = null!;
    public DateOnly Date { get; set; }
}