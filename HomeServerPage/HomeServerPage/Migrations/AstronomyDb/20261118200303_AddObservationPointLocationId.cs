using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServerPage.Migrations.AstronomyDb
{
    /// <inheritdoc />
    public partial class AddObservationPointLocationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "ObservationPoints",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ObservationPoints_LocationId",
                table: "ObservationPoints",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ObservationPoints_LocationId",
                table: "ObservationPoints");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "ObservationPoints");
        }
    }
}
