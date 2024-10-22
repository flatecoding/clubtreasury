using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class SetUnitDetailsIdToOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_UnitDetails_UnitDetailsId",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "UnitDetailsId",
                table: "Transactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_UnitDetails_UnitDetailsId",
                table: "Transactions",
                column: "UnitDetailsId",
                principalTable: "UnitDetails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_UnitDetails_UnitDetailsId",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "UnitDetailsId",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_UnitDetails_UnitDetailsId",
                table: "Transactions",
                column: "UnitDetailsId",
                principalTable: "UnitDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
