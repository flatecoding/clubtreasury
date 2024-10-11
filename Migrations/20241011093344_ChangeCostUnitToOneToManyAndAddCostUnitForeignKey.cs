using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCostUnitToOneToManyAndAddCostUnitForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CostUnits_CostUnitsDetails_CostUnitDetailsID",
                table: "CostUnits");

            migrationBuilder.DropIndex(
                name: "IX_CostUnits_CostUnitDetailsID",
                table: "CostUnits");

            migrationBuilder.DropColumn(
                name: "CostUnitDetailsID",
                table: "CostUnits");

            migrationBuilder.AddColumn<int>(
                name: "CostUnitId",
                table: "CostUnitsDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CostUnitID",
                table: "Transactions",
                column: "CostUnitID");

            migrationBuilder.CreateIndex(
                name: "IX_CostUnitsDetails_CostUnitId",
                table: "CostUnitsDetails",
                column: "CostUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_CostUnitsDetails_CostUnits_CostUnitId",
                table: "CostUnitsDetails",
                column: "CostUnitId",
                principalTable: "CostUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CostUnits_CostUnitID",
                table: "Transactions",
                column: "CostUnitID",
                principalTable: "CostUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CostUnitsDetails_CostUnits_CostUnitId",
                table: "CostUnitsDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CostUnits_CostUnitID",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CostUnitID",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_CostUnitsDetails_CostUnitId",
                table: "CostUnitsDetails");

            migrationBuilder.DropColumn(
                name: "CostUnitId",
                table: "CostUnitsDetails");

            migrationBuilder.AddColumn<int>(
                name: "CostUnitDetailsID",
                table: "CostUnits",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CostUnits_CostUnitDetailsID",
                table: "CostUnits",
                column: "CostUnitDetailsID");

            migrationBuilder.AddForeignKey(
                name: "FK_CostUnits_CostUnitsDetails_CostUnitDetailsID",
                table: "CostUnits",
                column: "CostUnitDetailsID",
                principalTable: "CostUnitsDetails",
                principalColumn: "Id");
        }
    }
}
