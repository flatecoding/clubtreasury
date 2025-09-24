using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class RenameUNitDetailsToItemDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Spalte für UnitDetails umbenennen
            migrationBuilder.RenameColumn(
                name: "UnitDetailsId",
                table: "Accounts",
                newName: "ItemDetailId");

            // Index entsprechend umbenennen
            migrationBuilder.RenameIndex(
                name: "IX_Accounts_UnitDetailsId",
                table: "Accounts",
                newName: "IX_Accounts_ItemDetailId");

            // Tabelle UnitDetails -> ItemDetails
            migrationBuilder.RenameTable(
                name: "UnitDetails",
                newName: "ItemDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Spalte zurückbenennen
            migrationBuilder.RenameColumn(
                name: "ItemDetailId",
                table: "Accounts",
                newName: "UnitDetailsId");

            // Index zurückbenennen
            migrationBuilder.RenameIndex(
                name: "IX_Accounts_ItemDetailId",
                table: "Accounts",
                newName: "IX_Accounts_UnitDetailsId");

            // Tabelle zurückbenennen
            migrationBuilder.RenameTable(
                name: "ItemDetails",
                newName: "UnitDetails");
        }
    }
}