using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class periodandleaveallocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ab411e7d-e55c-4ba2-9821-5add3fe9fe5d", "AQAAAAIAAYagAAAAEPsd9wg9oOrfwmMPQ7dtsH4Bd/SM/eeRgPztrzkG+bVv5QLq7Vi2NQyW+iW1ZGi6DA==", "33fced2a-dde8-41e2-a900-ad15cdbe7340" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "db3532d8-ad0f-4870-a831-fa09adab89cb", "AQAAAAIAAYagAAAAENryByItC4DNZ4m74UwW2OwTyT3EltM/WDTZZ3oEb4RKlVjHC+7xMHwYu29tQF5Emg==", "50c5c023-e822-456b-b4c6-6f530f70476d" });
        }
    }
}
