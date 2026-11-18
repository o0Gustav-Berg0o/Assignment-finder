using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentFinder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBrokerLoginUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoginUrl",
                table: "Brokers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoginUrl",
                table: "Brokers");
        }
    }
}
