using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class SecondtryForManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BasicUnitModelUnitDetailsModel_BasicUnits_BasicUnitsId",
                table: "BasicUnitModelUnitDetailsModel");

            migrationBuilder.DropForeignKey(
                name: "FK_BasicUnitModelUnitDetailsModel_UnitDetails_CostUnitDetailsId",
                table: "BasicUnitModelUnitDetailsModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BasicUnitModelUnitDetailsModel",
                table: "BasicUnitModelUnitDetailsModel");

            migrationBuilder.RenameTable(
                name: "BasicUnitModelUnitDetailsModel",
                newName: "PositionDetails");

            migrationBuilder.RenameIndex(
                name: "IX_BasicUnitModelUnitDetailsModel_CostUnitDetailsId",
                table: "PositionDetails",
                newName: "IX_PositionDetails_CostUnitDetailsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PositionDetails",
                table: "PositionDetails",
                columns: new[] { "BasicUnitsId", "CostUnitDetailsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PositionDetails_BasicUnits_BasicUnitsId",
                table: "PositionDetails",
                column: "BasicUnitsId",
                principalTable: "BasicUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PositionDetails_UnitDetails_CostUnitDetailsId",
                table: "PositionDetails",
                column: "CostUnitDetailsId",
                principalTable: "UnitDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PositionDetails_BasicUnits_BasicUnitsId",
                table: "PositionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PositionDetails_UnitDetails_CostUnitDetailsId",
                table: "PositionDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PositionDetails",
                table: "PositionDetails");

            migrationBuilder.RenameTable(
                name: "PositionDetails",
                newName: "BasicUnitModelUnitDetailsModel");

            migrationBuilder.RenameIndex(
                name: "IX_PositionDetails_CostUnitDetailsId",
                table: "BasicUnitModelUnitDetailsModel",
                newName: "IX_BasicUnitModelUnitDetailsModel_CostUnitDetailsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BasicUnitModelUnitDetailsModel",
                table: "BasicUnitModelUnitDetailsModel",
                columns: new[] { "BasicUnitsId", "CostUnitDetailsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BasicUnitModelUnitDetailsModel_BasicUnits_BasicUnitsId",
                table: "BasicUnitModelUnitDetailsModel",
                column: "BasicUnitsId",
                principalTable: "BasicUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BasicUnitModelUnitDetailsModel_UnitDetails_CostUnitDetailsId",
                table: "BasicUnitModelUnitDetailsModel",
                column: "CostUnitDetailsId",
                principalTable: "UnitDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
