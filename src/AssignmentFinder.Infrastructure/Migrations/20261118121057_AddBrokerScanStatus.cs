using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentFinder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBrokerScanStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastScanError",
                table: "Brokers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LastScanSucceeded",
                table: "Brokers",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastScanError",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "LastScanSucceeded",
                table: "Brokers");
        }
    }
}
