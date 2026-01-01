using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Date_Id",
                table: "Transactions",
                columns: new[] { "Date", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Description",
                table: "Transactions",
                column: "Description");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionDetails_PersonId_TransactionId",
                table: "TransactionDetails",
                columns: new[] { "PersonId", "TransactionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Person_Name",
                table: "Persons",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenter_CostUnitName",
                table: "CostCenters",
                column: "CostUnitName");

            migrationBuilder.CreateIndex(
                name: "IX_Category_Name",
                table: "Categories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ItemDetail_CostDetails",
                table: "ItemDetails",
                column: "CostDetails");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Transactions_Date_Id", table: "Transactions");
            migrationBuilder.DropIndex(name: "IX_Transactions_Description", table: "Transactions");
            migrationBuilder.DropIndex(name: "IX_TransactionDetails_PersonId_TransactionId",
                table: "TransactionDetails");
            migrationBuilder.DropIndex(name: "IX_Person_Name", table: "Persons");
            migrationBuilder.DropIndex(name: "IX_CostCenter_CostUnitName", table: "CostCenters");
            migrationBuilder.DropIndex(name: "IX_Category_Name", table: "Categories");
            migrationBuilder.DropIndex(name: "IX_ItemDetail_CostDetails", table: "ItemDetails");
        }
    }
}
