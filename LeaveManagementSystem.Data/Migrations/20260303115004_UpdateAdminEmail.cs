using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "DateOfBirth", "Email", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "SecurityStamp", "UserName" },
                values: new object[] { "db3532d8-ad0f-4870-a831-fa09adab89cb", new DateTime(2003, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@leaveapp.com", "ADMIN@LEAVEAPP.COM", "ADMIN@LEAVEAPP.COM", "AQAAAAIAAYagAAAAENryByItC4DNZ4m74UwW2OwTyT3EltM/WDTZZ3oEb4RKlVjHC+7xMHwYu29tQF5Emg==", "50c5c023-e822-456b-b4c6-6f530f70476d", "admin@leaveapp.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "DateOfBirth", "Email", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "SecurityStamp", "UserName" },
                values: new object[] { "fc14ddb6-121b-4284-a8c9-96ed2381f432", new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@localhost", "ADMIN@LOCALHOST", "ADMIN@LOCALHOST", "AQAAAAIAAYagAAAAEF3dPAhsgI7MzUWS8B3NDHR+qp76zELTHpQL26nc6S5sOurS/ELmCRU2+x3eZWrI6A==", "7d2ef436-724e-4529-91b9-31bb164a682a", "admin@localhost" });
        }
    }
}
