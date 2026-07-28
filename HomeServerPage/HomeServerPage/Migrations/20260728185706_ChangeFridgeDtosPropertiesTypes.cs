using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServerPage.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFridgeDtosPropertiesTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "TimeAfterOpen",
                table: "FridgeItems",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "TimeAfterOpen",
                table: "FridgeItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
