using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class RefactorFolderStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_BasicUnits_BasicUnitID",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CashRegisters_CashRegisterID",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CostUnits_CostUnitID",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_SpecialItems_SpecialItemID",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "SpecialItemID",
                table: "Transactions",
                newName: "SpecialItemId");

            migrationBuilder.RenameColumn(
                name: "CostUnitID",
                table: "Transactions",
                newName: "CostUnitId");

            migrationBuilder.RenameColumn(
                name: "CashRegisterID",
                table: "Transactions",
                newName: "CashRegisterId");

            migrationBuilder.RenameColumn(
                name: "BasicUnitID",
                table: "Transactions",
                newName: "BasicUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_SpecialItemID",
                table: "Transactions",
                newName: "IX_Transactions_SpecialItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_CostUnitID",
                table: "Transactions",
                newName: "IX_Transactions_CostUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_CashRegisterID",
                table: "Transactions",
                newName: "IX_Transactions_CashRegisterId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_BasicUnitID",
                table: "Transactions",
                newName: "IX_Transactions_BasicUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_BasicUnits_BasicUnitId",
                table: "Transactions",
                column: "BasicUnitId",
                principalTable: "BasicUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CashRegisters_CashRegisterId",
                table: "Transactions",
                column: "CashRegisterId",
                principalTable: "CashRegisters",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CostUnits_CostUnitId",
                table: "Transactions",
                column: "CostUnitId",
                principalTable: "CostUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_SpecialItems_SpecialItemId",
                table: "Transactions",
                column: "SpecialItemId",
                principalTable: "SpecialItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_BasicUnits_BasicUnitId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CashRegisters_CashRegisterId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CostUnits_CostUnitId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_SpecialItems_SpecialItemId",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "SpecialItemId",
                table: "Transactions",
                newName: "SpecialItemID");

            migrationBuilder.RenameColumn(
                name: "CostUnitId",
                table: "Transactions",
                newName: "CostUnitID");

            migrationBuilder.RenameColumn(
                name: "CashRegisterId",
                table: "Transactions",
                newName: "CashRegisterID");

            migrationBuilder.RenameColumn(
                name: "BasicUnitId",
                table: "Transactions",
                newName: "BasicUnitID");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_SpecialItemId",
                table: "Transactions",
                newName: "IX_Transactions_SpecialItemID");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_CostUnitId",
                table: "Transactions",
                newName: "IX_Transactions_CostUnitID");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_CashRegisterId",
                table: "Transactions",
                newName: "IX_Transactions_CashRegisterID");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_BasicUnitId",
                table: "Transactions",
                newName: "IX_Transactions_BasicUnitID");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_BasicUnits_BasicUnitID",
                table: "Transactions",
                column: "BasicUnitID",
                principalTable: "BasicUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CashRegisters_CashRegisterID",
                table: "Transactions",
                column: "CashRegisterID",
                principalTable: "CashRegisters",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CostUnits_CostUnitID",
                table: "Transactions",
                column: "CostUnitID",
                principalTable: "CostUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_SpecialItems_SpecialItemID",
                table: "Transactions",
                column: "SpecialItemID",
                principalTable: "SpecialItems",
                principalColumn: "Id");
        }
    }
}
