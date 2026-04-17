using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDateOfEmploymentToSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "DateOfBirth", "DateOfEmployment", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c35e6119-c6c0-4baa-9ddf-2249a7d1453c", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(2024, 1, 1), "AQAAAAIAAYagAAAAENQ5hC/pYR4oH5oGJtJ0zKReCAJ8rfBi9g4qtWw9rK1XPyAxmQe117qgdnjFTW+TGw==", "0b3de679-a59a-4ec1-b9aa-0cc953fa862a" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "DateOfBirth", "DateOfEmployment", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c8fe2dc6-4319-4fa3-be42-7fc84320c640", new DateTime(2003, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(1, 1, 1), "AQAAAAIAAYagAAAAEHKg2KyfvBgdfMDAavIU1+Kp5/pZgGfyq5NcO5H4ut+pJ3x0uPgEUmoX22MKJxWkGA==", "328541ad-b1d0-45b7-96e9-94e242efa396" });
        }
    }
}
