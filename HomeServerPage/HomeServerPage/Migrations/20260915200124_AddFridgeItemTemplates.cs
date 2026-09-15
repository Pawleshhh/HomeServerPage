using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServerPage.Migrations
{
    /// <inheritdoc />
    public partial class AddFridgeItemTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FridgeItemTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    QuantityValue = table.Column<double>(type: "REAL", nullable: false),
                    QuantityType = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpirationDaysAfterAdded = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeAfterOpen = table.Column<TimeSpan>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FridgeItemTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FridgeItemTemplates_Name",
                table: "FridgeItemTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO FridgeItemTemplates
                    (Name, QuantityValue, QuantityType, ExpirationDaysAfterAdded, TimeAfterOpen)
                SELECT
                    trim(source.Name),
                    source.QuantityValue,
                    source.QuantityType,
                    CAST(julianday(date(source.ExpirationDate)) - julianday(date(source.AddedDate)) AS INTEGER),
                    source.TimeAfterOpen
                FROM FridgeItems AS source
                WHERE trim(source.Name) <> ''
                    AND date(source.ExpirationDate) >= date(source.AddedDate)
                    AND source.Id =
                    (
                        SELECT MAX(candidate.Id)
                        FROM FridgeItems AS candidate
                        WHERE lower(trim(candidate.Name)) = lower(trim(source.Name))
                            AND trim(candidate.Name) <> ''
                            AND date(candidate.ExpirationDate) >= date(candidate.AddedDate)
                    );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FridgeItemTemplates");
        }
    }
}
