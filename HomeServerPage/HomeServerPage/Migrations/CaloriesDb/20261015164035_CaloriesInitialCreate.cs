using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServerPage.Migrations.CaloriesDb
{
    /// <inheritdoc />
    public partial class CaloriesInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Calories = table.Column<int>(type: "INTEGER", nullable: false),
                    CarbohydratesGrams = table.Column<int>(type: "INTEGER", nullable: true),
                    ProteinGrams = table.Column<int>(type: "INTEGER", nullable: true),
                    FatGrams = table.Column<int>(type: "INTEGER", nullable: true),
                    Servings = table.Column<int>(type: "INTEGER", nullable: true),
                    LocalDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    LoggedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                    table.CheckConstraint("CK_CaloriesEntries_Calories", "Calories > 0");
                    table.CheckConstraint("CK_CaloriesEntries_Macros", "(CarbohydratesGrams IS NULL OR CarbohydratesGrams >= 0) AND (ProteinGrams IS NULL OR ProteinGrams >= 0) AND (FatGrams IS NULL OR FatGrams >= 0)");
                    table.CheckConstraint("CK_CaloriesEntries_Servings", "Servings IS NULL OR Servings > 0");
                });

            migrationBuilder.CreateTable(
                name: "Foods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    CaloriesPerServing = table.Column<int>(type: "INTEGER", nullable: false),
                    CarbohydratesGrams = table.Column<int>(type: "INTEGER", nullable: true),
                    ProteinGrams = table.Column<int>(type: "INTEGER", nullable: true),
                    FatGrams = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foods", x => x.Id);
                    table.CheckConstraint("CK_CaloriesFoods_Calories", "CaloriesPerServing > 0");
                    table.CheckConstraint("CK_CaloriesFoods_Macros", "(CarbohydratesGrams IS NULL OR CarbohydratesGrams >= 0) AND (ProteinGrams IS NULL OR ProteinGrams >= 0) AND (FatGrams IS NULL OR FatGrams >= 0)");
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DailyCalorieGoal = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                    table.CheckConstraint("CK_CaloriesSettings_DailyGoal", "DailyCalorieGoal IS NULL OR DailyCalorieGoal > 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entries_LocalDate",
                table: "Entries",
                column: "LocalDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entries");

            migrationBuilder.DropTable(
                name: "Foods");

            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
