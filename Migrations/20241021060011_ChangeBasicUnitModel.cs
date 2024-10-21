using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class ChangeBasicUnitModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BasicUnits_CostUnits_CostUnitId",
                table: "BasicUnits");

            migrationBuilder.AlterColumn<int>(
                name: "CostUnitId",
                table: "BasicUnits",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_BasicUnits_CostUnits_CostUnitId",
                table: "BasicUnits",
                column: "CostUnitId",
                principalTable: "CostUnits",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BasicUnits_CostUnits_CostUnitId",
                table: "BasicUnits");

            migrationBuilder.AlterColumn<int>(
                name: "CostUnitId",
                table: "BasicUnits",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BasicUnits_CostUnits_CostUnitId",
                table: "BasicUnits",
                column: "CostUnitId",
                principalTable: "CostUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
