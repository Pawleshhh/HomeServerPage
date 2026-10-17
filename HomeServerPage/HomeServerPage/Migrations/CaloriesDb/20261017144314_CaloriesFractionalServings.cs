using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServerPage.Migrations.CaloriesDb
{
    /// <inheritdoc />
    public partial class CaloriesFractionalServings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CaloriesEntries_Servings",
                table: "Entries");

            migrationBuilder.AlterColumn<decimal>(
                name: "Servings",
                table: "Entries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CaloriesEntries_Servings",
                table: "Entries",
                sql: "Servings IS NULL OR CAST(Servings AS REAL) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CaloriesEntries_Servings",
                table: "Entries");

            migrationBuilder.AlterColumn<int>(
                name: "Servings",
                table: "Entries",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CaloriesEntries_Servings",
                table: "Entries",
                sql: "Servings IS NULL OR Servings > 0");
        }
    }
}
