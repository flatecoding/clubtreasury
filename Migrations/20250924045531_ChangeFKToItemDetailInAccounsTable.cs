using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFKToItemDetailInAccounsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alten FK löschen
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_UnitDetails_UnitDetailsId",
                table: "Accounts");

            // Neuen FK hinzufügen
            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_ItemDetails_ItemDetailId",
                table: "Accounts",
                column: "ItemDetailId",
                principalTable: "ItemDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // FK auf ItemDetails löschen
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_ItemDetails_ItemDetailId",
                table: "Accounts");

            // Alten FK zurücksetzen
            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_UnitDetails_UnitDetailsId",
                table: "Accounts",
                column: "UnitDetailsId",
                principalTable: "UnitDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

