using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class RenameAccountsToAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fremdschlüssel von Transactions auf Accounts entfernen
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Accounts_AccountsId",
                table: "Transactions");

            // Spalte in Transactions umbenennen
            migrationBuilder.RenameColumn(
                name: "AccountsId",
                table: "Transactions",
                newName: "AllocationId");

            // Index in Transactions umbenennen
            migrationBuilder.RenameIndex(
                name: "IX_Transactions_AccountsId",
                table: "Transactions",
                newName: "IX_Transactions_AllocationId");

            // Tabelle Accounts -> Allocations umbenennen
            migrationBuilder.RenameTable(
                name: "Accounts",
                newName: "Allocations");

            // Fremdschlüssel neu setzen
            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Allocations_AllocationId",
                table: "Transactions",
                column: "AllocationId",
                principalTable: "Allocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // FK von Transactions auf Allocations entfernen
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Allocations_AllocationId",
                table: "Transactions");

            // Spalte in Transactions zurückbenennen
            migrationBuilder.RenameColumn(
                name: "AllocationId",
                table: "Transactions",
                newName: "AccountsId");

            // Index in Transactions zurückbenennen
            migrationBuilder.RenameIndex(
                name: "IX_Transactions_AllocationId",
                table: "Transactions",
                newName: "IX_Transactions_AccountsId");

            // Tabelle Allocations -> Accounts zurückbenennen
            migrationBuilder.RenameTable(
                name: "Allocations",
                newName: "Accounts");

            // Fremdschlüssel neu setzen
            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Accounts_AccountsId",
                table: "Transactions",
                column: "AccountsId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
