using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServerPage.Migrations.ExerciseDb
{
    /// <inheritdoc />
    public partial class ExerciseInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExerciseEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    DistanceKm = table.Column<decimal>(type: "TEXT", nullable: true),
                    CaloriesBurned = table.Column<int>(type: "INTEGER", nullable: false),
                    LocalDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    LoggedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseEntries", x => x.Id);
                    table.CheckConstraint("CK_ExerciseEntries_Calories", "CaloriesBurned > 0");
                    table.CheckConstraint("CK_ExerciseEntries_Distance", "DistanceKm IS NULL OR CAST(DistanceKm AS REAL) > 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseEntries_LocalDate",
                table: "ExerciseEntries",
                column: "LocalDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseEntries");
        }
    }
}
