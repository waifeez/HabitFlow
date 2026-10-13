using System.Windows.Media;

namespace HabitFlow.App.ViewModels;

// Один столбец диаграммы «последние 7 дней»
public class BarItemViewModel
{
    public string Label { get; init; } = string.Empty;
    public int Count { get; init; }
    public double BarHeight { get; init; }
}

// Одна клетка календаря активности
public class HeatCellViewModel
{
    public Brush Fill { get; init; } = Brushes.Transparent;
    public string? Tip { get; init; }
}