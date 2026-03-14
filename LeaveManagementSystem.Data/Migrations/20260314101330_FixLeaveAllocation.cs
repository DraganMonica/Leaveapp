using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixLeaveAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4ca0d39e-03b4-4ec8-acb7-e989be549a5f", "AQAAAAIAAYagAAAAECOg15Pum4ol8FzfBNnsRl+N6QF39vLTBJo2o3a1oTPYjqnJP9muDeGJxWva71g5SA==", "f33a1461-0b8c-4848-a434-a551b093766b" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c2f66cfe-655a-4ba6-bdc0-259c054180d3", "AQAAAAIAAYagAAAAEOoRUAXY6oiVy5mXtFZkCSWmMPhUWwgioKsACVAchtfrOUzzPUOsv8ftKBDi3DXQnw==", "fc27447e-5cbf-4dff-a06a-27a22b0b425c" });
        }
    }
}
