using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    public partial class RefactorTransactionAndAllocationModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 2) Neuen eindeutigen Composite-Index anlegen (damit FK auf CostCenterId weiter einen passenden Index hat)
            migrationBuilder.CreateIndex(
                name: "IX_Allocations_CostCenterId_CategoryId_ItemDetailId",
                table: "Allocations",
                columns: new[] { "CostCenterId", "CategoryId", "ItemDetailId" },
                unique: true);

            // 3) Nur den alten CostCenter-Index droppen – aber NUR WENN er existiert
            //    (die Category- und ItemDetail-Indexe NICHT droppen, da sie FKs benötigen)
            migrationBuilder.Sql(@"
SET @exists := (
  SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'Allocations'
    AND INDEX_NAME = 'IX_Accounts_CostCenterId'
);
SET @sql := IF(@exists > 0,
  'ALTER TABLE `Allocations` DROP INDEX `IX_Accounts_CostCenterId`',
  'SELECT 1');
PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;");

            // 4) FK von Transactions -> Allocations mit RESTRICT wieder hinzufügen
            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Allocations_AllocationId",
                table: "Transactions",
                column: "AllocationId",
                principalTable: "Allocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // FK wieder entfernen
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Allocations_AllocationId",
                table: "Transactions");

            // Composite-Index entfernen
            migrationBuilder.DropIndex(
                name: "IX_Allocations_CostCenterId_CategoryId_ItemDetailId",
                table: "Allocations");

            // Den alten CostCenter-Index wiederherstellen (nur falls du den Down-Stand exakt abbilden willst)
            migrationBuilder.Sql(@"CREATE INDEX `IX_Accounts_CostCenterId` ON `Allocations` (`CostCenterId`);");

            // FK mit Cascading wie zuvor wiederherstellen
            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Allocations_AllocationId",
                table: "Transactions",
                column: "AllocationId",
                principalTable: "Allocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
