using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentFinder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBrokerCredentialsAndAssignmentInterest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoginPasswordEncrypted",
                table: "Brokers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoginUsername",
                table: "Brokers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssignmentInterests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    MarkedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentInterests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentInterests_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Xml = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentInterests_AssignmentId",
                table: "AssignmentInterests",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentInterests_UserId_AssignmentId",
                table: "AssignmentInterests",
                columns: new[] { "UserId", "AssignmentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentInterests");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropColumn(
                name: "LoginPasswordEncrypted",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "LoginUsername",
                table: "Brokers");
        }
    }
}
