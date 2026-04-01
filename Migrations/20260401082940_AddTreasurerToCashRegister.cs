using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClubTreasury.Migrations
{
    /// <inheritdoc />
    public partial class AddTreasurerToCashRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TreasurerId",
                table: "CashRegisters",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashRegisters_TreasurerId",
                table: "CashRegisters",
                column: "TreasurerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CashRegisters_Persons_TreasurerId",
                table: "CashRegisters",
                column: "TreasurerId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashRegisters_Persons_TreasurerId",
                table: "CashRegisters");

            migrationBuilder.DropIndex(
                name: "IX_CashRegisters_TreasurerId",
                table: "CashRegisters");

            migrationBuilder.DropColumn(
                name: "TreasurerId",
                table: "CashRegisters");
        }
    }
}
