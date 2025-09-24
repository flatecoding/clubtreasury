using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class RenameSubTransactionsToTransactionDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tabelle SubTransactions -> TransactionDetails umbenennen
            migrationBuilder.RenameTable(
                name: "SubTransactions",
                newName: "TransactionDetails");

            // Indizes umbenennen
            migrationBuilder.RenameIndex(
                name: "IX_SubTransactions_PersonId",
                table: "TransactionDetails",
                newName: "IX_TransactionDetails_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_SubTransactions_TransactionId",
                table: "TransactionDetails",
                newName: "IX_TransactionDetails_TransactionId");

            // Fremdschlüssel anpassen
            migrationBuilder.DropForeignKey(
                name: "FK_SubTransactions_Persons_PersonId",
                table: "TransactionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SubTransactions_Transactions_TransactionId",
                table: "TransactionDetails");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionDetails_Persons_PersonId",
                table: "TransactionDetails",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionDetails_Transactions_TransactionId",
                table: "TransactionDetails",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Fremdschlüssel auf SubTransactions zurücksetzen
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionDetails_Persons_PersonId",
                table: "TransactionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionDetails_Transactions_TransactionId",
                table: "TransactionDetails");

            // Tabelle zurückbenennen
            migrationBuilder.RenameTable(
                name: "TransactionDetails",
                newName: "SubTransactions");

            // Indizes zurückbenennen
            migrationBuilder.RenameIndex(
                name: "IX_TransactionDetails_PersonId",
                table: "SubTransactions",
                newName: "IX_SubTransactions_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionDetails_TransactionId",
                table: "SubTransactions",
                newName: "IX_SubTransactions_TransactionId");

            // Fremdschlüssel wiederherstellen
            migrationBuilder.AddForeignKey(
                name: "FK_SubTransactions_Persons_PersonId",
                table: "SubTransactions",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubTransactions_Transactions_TransactionId",
                table: "SubTransactions",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
