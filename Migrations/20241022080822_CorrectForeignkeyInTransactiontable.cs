using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class CorrectForeignkeyInTransactiontable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_UnitDetails_UnitDetailsD",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UnitDetailsD",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "UnitDetailsId",
                table: "Transactions",
                newName: "UnitDetailsID");

            migrationBuilder.RenameColumn(
                name: "UnitDetailsD",
                table: "Transactions",
                newName: "UnitDetaislId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UnitDetailsID",
                table: "Transactions",
                column: "UnitDetailsID");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_UnitDetails_UnitDetailsID",
                table: "Transactions",
                column: "UnitDetailsID",
                principalTable: "UnitDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_UnitDetails_UnitDetailsID",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UnitDetailsID",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "UnitDetailsID",
                table: "Transactions",
                newName: "UnitDetailsId");

            migrationBuilder.RenameColumn(
                name: "UnitDetaislId",
                table: "Transactions",
                newName: "UnitDetailsD");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UnitDetailsD",
                table: "Transactions",
                column: "UnitDetailsD");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_UnitDetails_UnitDetailsD",
                table: "Transactions",
                column: "UnitDetailsD",
                principalTable: "UnitDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
