namespace HabitFlow.Core.Services;

public static class LevelCalculator
{
    public const int XpPerCheck = 10;
    public const int XpPerLevel = 100;

    public static int Xp(int totalChecks) => totalChecks * XpPerCheck;
    public static int Level(int xp) => xp / XpPerLevel + 1;
    public static int XpIntoLevel(int xp) => xp % XpPerLevel;
}