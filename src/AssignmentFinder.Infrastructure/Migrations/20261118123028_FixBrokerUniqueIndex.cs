using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentFinder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBrokerUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Brokers_ConnectorKey",
                table: "Brokers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Brokers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ConnectorKey",
                table: "Brokers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Brokers_Name",
                table: "Brokers",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Brokers_Name",
                table: "Brokers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Brokers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ConnectorKey",
                table: "Brokers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Brokers_ConnectorKey",
                table: "Brokers",
                column: "ConnectorKey",
                unique: true);
        }
    }
}
