using CommunityToolkit.Mvvm.ComponentModel;

namespace HabitFlow.App.ViewModels;

// Один флажок дня недели в панели добавления привычки
public partial class DayOptionViewModel : ObservableObject
{
    public DayOptionViewModel(string label, int mask)
    {
        Label = label;
        Mask = mask;
    }

    public string Label { get; }
    public int Mask { get; }

    [ObservableProperty] private bool isSelected = true;
}