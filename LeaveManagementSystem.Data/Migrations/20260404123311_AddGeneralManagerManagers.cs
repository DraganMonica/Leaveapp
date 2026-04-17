using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralManagerManagers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneralManagerManagers",
                columns: table => new
                {
                    GeneralManagerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ManagerId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralManagerManagers", x => new { x.GeneralManagerId, x.ManagerId });
                    table.ForeignKey(
                        name: "FK_GeneralManagerManagers_AspNetUsers_GeneralManagerId",
                        column: x => x.GeneralManagerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GeneralManagerManagers_AspNetUsers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "00161dac-fdd1-41dd-bea9-d69e4725c145", "AQAAAAIAAYagAAAAEJzxKSuVmcdod7T/jyCmOh1CYDfGUKowQwzKQyOU52I6+qUDA1J1ww55Sa1ai2heZA==", "d9ef2eab-7a3c-4b67-9a43-6e66f1561d1c" });

            migrationBuilder.CreateIndex(
                name: "IX_GeneralManagerManagers_ManagerId",
                table: "GeneralManagerManagers",
                column: "ManagerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralManagerManagers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d66e7e86-2e45-4813-97c5-08e176975f2d", "AQAAAAIAAYagAAAAEH4tXIhWjZzjCPUJkUl94TLJrXD3O3G02IDmtgY7mxu99EDFiH8QpCF2wQtn6nrQjA==", "3ac8d0a0-f111-4c36-bfeb-bfe12427602e" });
        }
    }
}
