using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CodingCleanProject.Migrations
{
    /// <inheritdoc />
    public partial class Promjene : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2abd2f9c-f2e6-4ea2-9eb3-5bd115bace67");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "751f4ef2-9bec-410e-9d4c-b41ebab3d0bd");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "RefreshToken",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2ab71a3a-31a7-4924-86a7-b9a242374e4a", null, "Admin", "ADMIN" },
                    { "a1825631-c11c-47ff-8d29-375b1c39d999", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2ab71a3a-31a7-4924-86a7-b9a242374e4a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1825631-c11c-47ff-8d29-375b1c39d999");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RefreshToken");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2abd2f9c-f2e6-4ea2-9eb3-5bd115bace67", null, "Admin", "ADMIN" },
                    { "751f4ef2-9bec-410e-9d4c-b41ebab3d0bd", null, "User", "USER" }
                });
        }
    }
}
