using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitFlow.Core.Services;
using HabitFlow.Data;

namespace HabitFlow.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly HabitRepository _repo = new();

    public ObservableCollection<HabitItemViewModel> Habits { get; } = new();

    public string[] Emojis { get; } = { "✅", "🏃", "📚", "💧", "💻", "🧘", "🎸", "😴" };

    public string TodayText =>
        DateTime.Today.ToString("d MMMM", new CultureInfo("ru-RU"));

    public int XpPerLevel => LevelCalculator.XpPerLevel;

    [ObservableProperty] private string newHabitName = string.Empty;
    [ObservableProperty] private string selectedEmoji = "✅";
    [ObservableProperty] private string summary = string.Empty;
    [ObservableProperty] private int level = 1;
    [ObservableProperty] private int xpIntoLevel;

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    public async Task LoadAsync()
    {
        var today = Today;
        var habits = await _repo.GetAllAsync();

        Habits.Clear();
        foreach (var h in habits)
        {
            var dates = h.Logs.Select(l => l.Date).ToList();
            Habits.Add(new HabitItemViewModel
            {
                Id = h.Id,
                Name = h.Name,
                Emoji = h.Emoji,
                Streak = StreakCalculator.Current(dates, today),
                IsDoneToday = dates.Contains(today)
            });
        }

        var done = Habits.Count(h => h.IsDoneToday);
        Summary = $"{done} из {Habits.Count} выполнено";

        var xp = LevelCalculator.Xp(habits.Sum(h => h.Logs.Count));
        Level = LevelCalculator.Level(xp);
        XpIntoLevel = LevelCalculator.XpIntoLevel(xp);
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var name = NewHabitName.Trim();
        if (name.Length == 0) return;

        await _repo.AddAsync(name, SelectedEmoji);
        NewHabitName = string.Empty;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ToggleAsync(HabitItemViewModel? item)
    {
        if (item is null) return;

        await _repo.ToggleAsync(item.Id, Today);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteAsync(HabitItemViewModel? item)
    {
        if (item is null) return;

        await _repo.DeleteAsync(item.Id);
        await LoadAsync();
    }
}