using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DemoBackend.Migrations
{
    /// <inheritdoc />
    public partial class SeedRolesWithHardcodedGuids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "d1b6a7e1-8c4b-4b8a-9b1d-1a2b3c4d5e6f", null, "Admin", "ADMIN" },
                    { "e2c7b8f2-9d5c-4e8b-8a1d-2b3c4d5e6f7g", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d1b6a7e1-8c4b-4b8a-9b1d-1a2b3c4d5e6f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e2c7b8f2-9d5c-4e8b-8a1d-2b3c4d5e6f7g");
        }
    }
}
