using System.Windows;
using HabitFlow.Data;
using Microsoft.EntityFrameworkCore;

namespace HabitFlow.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Применяет все ещё не применённые миграции (создаёт базу, если её нет).
        using (var db = AppDbContext.Create())
        {
            db.Database.Migrate();
        }

        base.OnStartup(e);
    }
}