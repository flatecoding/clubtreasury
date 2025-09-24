using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class RenameBasicUnitToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. FK in Accounts vorübergehend löschen
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_BasicUnits_BasicUnitId",
                table: "Accounts");

            // 2. Tabelle umbenennen
            migrationBuilder.RenameTable(
                name: "BasicUnits",
                newName: "Categories");

            // 3. Spalten in Accounts umbenennen
            migrationBuilder.RenameColumn(
                name: "BasicUnitId",
                table: "Accounts",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_BasicUnitId",
                table: "Accounts",
                newName: "IX_Accounts_CategoryId");

            // 4. Spalten und Indizes in der umbenannten Tabelle ggf. anpassen
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Categories",
                newName: "Name"); // falls Name bleibt, kann man weglassen

            // 5. FK wiederherstellen
            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Categories_CategoryId",
                table: "Accounts",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // FK in Accounts löschen
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Categories_CategoryId",
                table: "Accounts");

            // Tabelle zurück umbenennen
            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "BasicUnits");

            // Spalten in Accounts zurück umbenennen
            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Accounts",
                newName: "BasicUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_CategoryId",
                table: "Accounts",
                newName: "IX_Accounts_BasicUnitId");

            // FK wiederherstellen
            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_BasicUnits_BasicUnitId",
                table: "Accounts",
                column: "BasicUnitId",
                principalTable: "BasicUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
