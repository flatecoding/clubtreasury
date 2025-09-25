using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    public partial class CheckDbStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Entferne FK von Categories → CostCenters
            migrationBuilder.DropForeignKey(
                name: "FK_BasicUnits_CostCenters_CostCenterId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_BasicUnits_CostCenterId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "Categories");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Füge CostCenterId in Categories wieder hinzu
            migrationBuilder.AddColumn<int>(
                name: "CostCenterId",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BasicUnits_CostCenterId",
                table: "Categories",
                column: "CostCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_BasicUnits_CostCenters_CostCenterId",
                table: "Categories",
                column: "CostCenterId",
                principalTable: "CostCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}