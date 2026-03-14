using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LeaveManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedingDefaultRoleandUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "084de6a2-19f7-44dd-859c-52985c0ae2b8", null, "Administrator", "ADMINISTRATOR" },
                    { "46ed7d4b-3380-4e4b-970f-0fdc0fa6e53b", null, "Employee", "EMPLOYEE" },
                    { "65ad5989-4367-46e3-a137-09005de811a4", null, "Manager", "MANAGER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "a1b2c3d4-e5f6-7890-abcd-ef1234567890", 0, "325318bf-8eb6-44c9-96af-094ea7b8ef02", "admin@localhost", true, false, null, "ADMIN@LOCALHOST", "ADMIN@LOCALHOST", "AQAAAAIAAYagAAAAEMjmwbi5O5kTAml+DeIfh7c+YPVtE8N9WAB12I/RY6E9aZtmW5onolPWV8NQEEFb+Q==", null, false, "d8035df4-06dd-4fb1-abc8-5a6c97ddbd76", false, "admin@localhost" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "084de6a2-19f7-44dd-859c-52985c0ae2b8", "a1b2c3d4-e5f6-7890-abcd-ef1234567890" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "46ed7d4b-3380-4e4b-970f-0fdc0fa6e53b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "65ad5989-4367-46e3-a137-09005de811a4");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "084de6a2-19f7-44dd-859c-52985c0ae2b8", "a1b2c3d4-e5f6-7890-abcd-ef1234567890" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "084de6a2-19f7-44dd-859c-52985c0ae2b8");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        }
    }
}
