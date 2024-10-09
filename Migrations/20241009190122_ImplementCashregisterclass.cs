using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class ImplementCashregisterclass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CashRegisterID",
                table: "Entries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CashRegister",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CurrentBalance = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashRegister", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_CashRegisterID",
                table: "Entries",
                column: "CashRegisterID");

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_CashRegister_CashRegisterID",
                table: "Entries",
                column: "CashRegisterID",
                principalTable: "CashRegister",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_CashRegister_CashRegisterID",
                table: "Entries");

            migrationBuilder.DropTable(
                name: "CashRegister");

            migrationBuilder.DropIndex(
                name: "IX_Entries_CashRegisterID",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "CashRegisterID",
                table: "Entries");
        }
    }
}
