using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDateOfEmployment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfEmployment",
                table: "AspNetUsers",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "DateOfEmployment", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c8fe2dc6-4319-4fa3-be42-7fc84320c640", new DateOnly(1, 1, 1), "AQAAAAIAAYagAAAAEHKg2KyfvBgdfMDAavIU1+Kp5/pZgGfyq5NcO5H4ut+pJ3x0uPgEUmoX22MKJxWkGA==", "328541ad-b1d0-45b7-96e9-94e242efa396" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfEmployment",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4ca0d39e-03b4-4ec8-acb7-e989be549a5f", "AQAAAAIAAYagAAAAECOg15Pum4ol8FzfBNnsRl+N6QF39vLTBJo2o3a1oTPYjqnJP9muDeGJxWva71g5SA==", "f33a1461-0b8c-4848-a434-a551b093766b" });
        }
    }
}
