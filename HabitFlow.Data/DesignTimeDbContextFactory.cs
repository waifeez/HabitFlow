using Microsoft.EntityFrameworkCore.Design;

namespace HabitFlow.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) => AppDbContext.Create();
}