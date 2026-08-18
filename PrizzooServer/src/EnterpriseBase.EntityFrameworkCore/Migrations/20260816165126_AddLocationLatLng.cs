using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseBase.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationLatLng : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Locations",
                type: "numeric(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Locations",
                type: "numeric(9,6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Locations");
        }
    }
}
