using System.Windows;
using HabitFlow.Data;

namespace HabitFlow.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Пока без миграций: база создаётся при первом запуске.
        using (var db = AppDbContext.Create())
        {
            db.Database.EnsureCreated();
        }

        base.OnStartup(e);
    }
}