using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitFlow.Core.Models;
using HabitFlow.Core.Services;
using HabitFlow.Data;

namespace HabitFlow.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly HabitRepository _repo = new();

    public ObservableCollection<HabitItemViewModel> Habits { get; } = new();
    public ObservableCollection<AchievementItemViewModel> Achievements { get; } = new();

    public string[] Emojis { get; } = { "✅", "🏃", "📚", "💧", "💻", "🧘", "🎸", "😴" };

    public List<DayOptionViewModel> DayOptions { get; } = new()
    {
        new("Пн", WeekDays.Monday),
        new("Вт", WeekDays.Tuesday),
        new("Ср", WeekDays.Wednesday),
        new("Чт", WeekDays.Thursday),
        new("Пт", WeekDays.Friday),
        new("Сб", WeekDays.Saturday),
        new("Вс", WeekDays.Sunday),
    };

    public string TodayText =>
        DateTime.Today.ToString("d MMMM", new CultureInfo("ru-RU"));

    public int XpPerLevel => LevelCalculator.XpPerLevel;

    [ObservableProperty] private string newHabitName = string.Empty;
    [ObservableProperty] private string selectedEmoji = "✅";
    [ObservableProperty] private string summary = string.Empty;
    [ObservableProperty] private string addError = string.Empty;
    [ObservableProperty] private string notice = string.Empty;
    [ObservableProperty] private string achievementsSummary = string.Empty;
    [ObservableProperty] private int level = 1;
    [ObservableProperty] private int xpIntoLevel;

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    public async Task LoadAsync()
    {
        Notice = string.Empty;

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
                Days = h.Days,
                IsScheduledToday = WeekDays.IsScheduled(h.Days, today),
                Streak = StreakCalculator.Current(dates, today, h.Days),
                IsDoneToday = dates.Contains(today)
            });
        }

        var scheduled = Habits.Count(h => h.IsScheduledToday);
        var done = Habits.Count(h => h.IsScheduledToday && h.IsDoneToday);
        Summary = $"{done} из {scheduled} на сегодня";

        var totalChecks = habits.Sum(h => h.Logs.Count);
        var xp = LevelCalculator.Xp(totalChecks);
        Level = LevelCalculator.Level(xp);
        XpIntoLevel = LevelCalculator.XpIntoLevel(xp);

        await UpdateAchievementsAsync(totalChecks);
    }

    private async Task UpdateAchievementsAsync(int totalChecks)
    {
        var bestStreak = Habits.Count == 0 ? 0 : Habits.Max(h => h.Streak);
        var stats = new AchievementStats(totalChecks, Habits.Count, bestStreak);

        var unlocked = await _repo.GetUnlockedKeysAsync();
        var newKeys = AchievementCatalog.Earned(stats)
            .Where(k => !unlocked.Contains(k))
            .ToList();

        if (newKeys.Count > 0)
        {
            await _repo.UnlockAsync(newKeys);
            unlocked.UnionWith(newKeys);

            var titles = AchievementCatalog.All
                .Where(a => newKeys.Contains(a.Key))
                .Select(a => a.Title);
            Notice = "🎉 Новое достижение: " + string.Join(", ", titles);
        }

        Achievements.Clear();
        foreach (var a in AchievementCatalog.All)
        {
            Achievements.Add(new AchievementItemViewModel
            {
                Emoji = a.Emoji,
                Title = a.Title,
                Description = a.Description,
                IsUnlocked = unlocked.Contains(a.Key)
            });
        }

        AchievementsSummary = $"Получено {unlocked.Count} из {AchievementCatalog.All.Count}";
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var name = NewHabitName.Trim();
        if (name.Length == 0) return;

        var mask = DayOptions.Where(d => d.IsSelected).Sum(d => d.Mask);
        if (mask == 0)
        {
            AddError = "Выберите хотя бы один день недели";
            return;
        }

        await _repo.AddAsync(name, SelectedEmoji, mask);

        AddError = string.Empty;
        NewHabitName = string.Empty;
        foreach (var d in DayOptions) d.IsSelected = true;

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