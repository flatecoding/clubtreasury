using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class PreventDeleteOfTransactionWhenBasicUnitIsDeleated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_BasicUnits_BasicUnitId",
                table: "Transactions");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Transactions",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_BasicUnits_BasicUnitId",
                table: "Transactions",
                column: "BasicUnitId",
                principalTable: "BasicUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_BasicUnits_BasicUnitId",
                table: "Transactions");

            migrationBuilder.UpdateData(
                table: "Transactions",
                keyColumn: "Description",
                keyValue: null,
                column: "Description",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Transactions",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_BasicUnits_BasicUnitId",
                table: "Transactions",
                column: "BasicUnitId",
                principalTable: "BasicUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
