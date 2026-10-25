using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServerPage.Migrations.CaloriesDb
{
    /// <inheritdoc />
    public partial class CaloriesServingGrams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GramsPerServing",
                table: "Foods",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GramsConsumed",
                table: "Entries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CaloriesFoods_GramsPerServing",
                table: "Foods",
                sql: "GramsPerServing IS NULL OR CAST(GramsPerServing AS REAL) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CaloriesEntries_GramsConsumed",
                table: "Entries",
                sql: "GramsConsumed IS NULL OR CAST(GramsConsumed AS REAL) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CaloriesFoods_GramsPerServing",
                table: "Foods");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CaloriesEntries_GramsConsumed",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "GramsPerServing",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "GramsConsumed",
                table: "Entries");
        }
    }
}
