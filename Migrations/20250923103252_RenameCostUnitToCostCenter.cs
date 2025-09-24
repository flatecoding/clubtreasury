using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class RenameCostUnitToCostCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tabelle umbenennen
            migrationBuilder.RenameTable(
                name: "CostUnits",
                newName: "CostCenters");

            // Spalte in Accounts von CostUnitId → CostCenterId
            migrationBuilder.RenameColumn(
                name: "CostUnitId",
                table: "Accounts",
                newName: "CostCenterId");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_CostUnitId",
                table: "Accounts",
                newName: "IX_Accounts_CostCenterId");

            // Spalte in BasicUnits von CostUnitId → CostCenterId
            migrationBuilder.RenameColumn(
                name: "CostUnitId",
                table: "BasicUnits",
                newName: "CostCenterId");

            migrationBuilder.RenameIndex(
                name: "IX_BasicUnits_CostUnitId",
                table: "BasicUnits",
                newName: "IX_BasicUnits_CostCenterId");

            // Foreign Keys anpassen
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_CostUnits_CostUnitId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_BasicUnits_CostUnits_CostUnitId",
                table: "BasicUnits");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_CostCenters_CostCenterId",
                table: "Accounts",
                column: "CostCenterId",
                principalTable: "CostCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BasicUnits_CostCenters_CostCenterId",
                table: "BasicUnits",
                column: "CostCenterId",
                principalTable: "CostCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Foreign Keys zurück
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_CostCenters_CostCenterId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_BasicUnits_CostCenters_CostCenterId",
                table: "BasicUnits");

            // Spalten zurückbenennen
            migrationBuilder.RenameColumn(
                name: "CostCenterId",
                table: "Accounts",
                newName: "CostUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Accounts_CostCenterId",
                table: "Accounts",
                newName: "IX_Accounts_CostUnitId");

            migrationBuilder.RenameColumn(
                name: "CostCenterId",
                table: "BasicUnits",
                newName: "CostUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_BasicUnits_CostCenterId",
                table: "BasicUnits",
                newName: "IX_BasicUnits_CostUnitId");

            // Tabelle zurück
            migrationBuilder.RenameTable(
                name: "CostCenters",
                newName: "CostUnits");

            // Foreign Keys zurücksetzen
            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_CostUnits_CostUnitId",
                table: "Accounts",
                column: "CostUnitId",
                principalTable: "CostUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BasicUnits_CostUnits_CostUnitId",
                table: "BasicUnits",
                column: "CostUnitId",
                principalTable: "CostUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
