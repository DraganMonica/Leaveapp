using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralManagerRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d66e7e86-2e45-4813-97c5-08e176975f2d", "AQAAAAIAAYagAAAAEH4tXIhWjZzjCPUJkUl94TLJrXD3O3G02IDmtgY7mxu99EDFiH8QpCF2wQtn6nrQjA==", "3ac8d0a0-f111-4c36-bfeb-bfe12427602e" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9bbf66bf-e293-4231-9868-791ffa988c12", "AQAAAAIAAYagAAAAEIm72kVf//oY+oyqScVoN+OJdOnO99GKB8qbhq3MBakfFAn5tn2QNXIdNDjtPAYgDw==", "e7d22551-4d03-406c-b958-3cff153886f1" });
        }
    }
}
