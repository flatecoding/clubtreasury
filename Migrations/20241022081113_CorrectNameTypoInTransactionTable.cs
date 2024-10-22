using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class CorrectNameTypoInTransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_UnitDetails_UnitDetailsID",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UnitDetaislId",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "UnitDetailsID",
                table: "Transactions",
                newName: "UnitDetailsId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_UnitDetailsID",
                table: "Transactions",
                newName: "IX_Transactions_UnitDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_UnitDetails_UnitDetailsId",
                table: "Transactions",
                column: "UnitDetailsId",
                principalTable: "UnitDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_UnitDetails_UnitDetailsId",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "UnitDetailsId",
                table: "Transactions",
                newName: "UnitDetailsID");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_UnitDetailsId",
                table: "Transactions",
                newName: "IX_Transactions_UnitDetailsID");

            migrationBuilder.AddColumn<int>(
                name: "UnitDetaislId",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_UnitDetails_UnitDetailsID",
                table: "Transactions",
                column: "UnitDetailsID",
                principalTable: "UnitDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
