using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitFlow.Core.Models;
using HabitFlow.Core.Services;
using HabitFlow.Data;

namespace HabitFlow.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private static readonly string[] DayNames = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };

    // от пустой клетки к самой насыщенной
    private static readonly Brush[] HeatBrushes =
    {
        MakeBrush("#EEEEEE"), MakeBrush("#D1C4E9"), MakeBrush("#B39DDB"),
        MakeBrush("#7E57C2"), MakeBrush("#512DA8")
    };

    private readonly HabitRepository _repo = new();

    public ObservableCollection<HabitItemViewModel> Habits { get; } = new();
    public ObservableCollection<AchievementItemViewModel> Achievements { get; } = new();
    public ObservableCollection<BarItemViewModel> WeekBars { get; } = new();
    public ObservableCollection<HeatCellViewModel> HeatCells { get; } = new();

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

    [ObservableProperty] private int statTotalChecks;
    [ObservableProperty] private int statLongestStreak;
    [ObservableProperty] private int statHabitCount;
    [ObservableProperty] private string statCompletion = "0%";

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    private static Brush MakeBrush(string hex)
    {
        var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
        brush.Freeze();
        return brush;
    }

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

        UpdateStats(habits);
        await UpdateAchievementsAsync(totalChecks);
    }

    private void UpdateStats(List<Habit> habits)
    {
        var today = Today;

        var inputs = habits
            .Select(h => new HabitStatsInput(
                h.Days,
                DateOnly.FromDateTime(h.CreatedAt),
                h.Logs.Select(l => l.Date).ToHashSet()))
            .ToList();

        var counts = StatisticsCalculator.CountsByDay(inputs);

        StatTotalChecks = habits.Sum(h => h.Logs.Count);
        StatHabitCount = habits.Count;
        StatLongestStreak = inputs.Count == 0
            ? 0
            : inputs.Max(i => StatisticsCalculator.LongestStreak(i.Dates, i.Days));
        StatCompletion = StatisticsCalculator.CompletionPercent(inputs, today) + "%";

        // последние 7 дней
        var week = Enumerable.Range(0, 7).Select(i => today.AddDays(i - 6)).ToList();
        var weekMax = Math.Max(1, week.Max(d => counts.GetValueOrDefault(d)));

        WeekBars.Clear();
        foreach (var d in week)
        {
            var c = counts.GetValueOrDefault(d);
            WeekBars.Add(new BarItemViewModel
            {
                Label = DayNames[((int)d.DayOfWeek + 6) % 7],
                Count = c,
                BarHeight = c == 0 ? 2 : Math.Max(6, 90.0 * c / weekMax)
            });
        }

        // календарь активности: 12 недель, строки = дни недели, столбцы = недели
        var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var start = monday.AddDays(-7 * 11);
        var heatMax = Enumerable.Range(0, 84).Max(i => counts.GetValueOrDefault(start.AddDays(i)));

        HeatCells.Clear();
        for (var row = 0; row < 7; row++)
        {
            for (var col = 0; col < 12; col++)
            {
                var d = start.AddDays(col * 7 + row);
                if (d > today)
                {
                    HeatCells.Add(new HeatCellViewModel { Fill = Brushes.Transparent, Tip = null });
                    continue;
                }

                var c = counts.GetValueOrDefault(d);
                HeatCells.Add(new HeatCellViewModel
                {
                    Fill = HeatBrushes[HeatLevel(c, heatMax)],
                    Tip = $"{d:dd.MM.yyyy}: {c} отм."
                });
            }
        }
    }

    private static int HeatLevel(int count, int max)
    {
        if (count <= 0 || max <= 0) return 0;

        var ratio = (double)count / max;
        return ratio <= 0.25 ? 1 : ratio <= 0.5 ? 2 : ratio <= 0.75 ? 3 : 4;
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