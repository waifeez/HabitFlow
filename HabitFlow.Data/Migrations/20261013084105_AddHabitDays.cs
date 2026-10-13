using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHabitDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Days",
                table: "Habits",
                type: "INTEGER",
                nullable: false,
                defaultValue: 127);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Days",
                table: "Habits");
        }
    }
}
