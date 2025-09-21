using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class ImplementConnetionTableAccountInDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. Neue Tabelle Accounts erstellen
    migrationBuilder.CreateTable(
        name: "Accounts",
        columns: table => new
        {
            Id = table.Column<int>(type: "int", nullable: false)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
            CostUnitId = table.Column<int>(type: "int", nullable: false),
            BasicUnitId = table.Column<int>(type: "int", nullable: false),
            UnitDetailsId = table.Column<int>(type: "int", nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Accounts", x => x.Id);
            table.ForeignKey(
                name: "FK_Accounts_CostUnits_CostUnitId",
                column: x => x.CostUnitId,
                principalTable: "CostUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                name: "FK_Accounts_BasicUnits_BasicUnitId",
                column: x => x.BasicUnitId,
                principalTable: "BasicUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            table.ForeignKey(
                name: "FK_Accounts_UnitDetails_UnitDetailsId",
                column: x => x.UnitDetailsId,
                principalTable: "UnitDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        })
        .Annotation("MySql:CharSet", "utf8mb4");

    // 2. Daten aus Transactions in Accounts übertragen (nur neue Kombinationen)
    migrationBuilder.Sql(@"
        INSERT INTO Accounts (CostUnitId, BasicUnitId, UnitDetailsId)
        SELECT DISTINCT T.CostUnitId, T.BasicUnitId, T.UnitDetailsId
        FROM Transactions T
        WHERE NOT EXISTS (
            SELECT 1
            FROM Accounts A
            WHERE A.CostUnitId = T.CostUnitId
              AND A.BasicUnitId = T.BasicUnitId
              AND (
                (T.UnitDetailsId IS NULL AND A.UnitDetailsId IS NULL)
                OR (T.UnitDetailsId = A.UnitDetailsId)
              )
        );
    ");

    // 3. Neue Spalte AccountId in Transactions hinzufügen
    migrationBuilder.AddColumn<int>(
        name: "AccountsId",
        table: "Transactions",
        type: "int",
        nullable: true);

    // 4. AccountId in Transactions korrekt zuweisen
    migrationBuilder.Sql(@"
        UPDATE Transactions T
        JOIN Accounts A
          ON T.CostUnitId = A.CostUnitId
          AND T.BasicUnitId = A.BasicUnitId
          AND (
            (T.UnitDetailsId IS NULL AND A.UnitDetailsId IS NULL)
            OR (T.UnitDetailsId = A.UnitDetailsId)
          )
        SET T.AccountsId = A.Id;
    ");
    
    migrationBuilder.AlterColumn<int>(
        name: "AccountsId",
        table: "Transactions",
        type: "int",
        nullable: false,
        oldClrType: typeof(int),
        oldType: "int",
        oldNullable: true);

    // 5. Alte Fremdschlüssel entfernen
    migrationBuilder.DropForeignKey(
        name: "FK_Transactions_CostUnits_CostUnitId",
        table: "Transactions");

    migrationBuilder.DropForeignKey(
        name: "FK_Transactions_BasicUnits_BasicUnitId",
        table: "Transactions");

    migrationBuilder.DropForeignKey(
        name: "FK_Transactions_UnitDetails_UnitDetailsId",
        table: "Transactions");

    // 6. Alte Spalten entfernen
    migrationBuilder.DropColumn(name: "CostUnitId", table: "Transactions");
    migrationBuilder.DropColumn(name: "BasicUnitId", table: "Transactions");
    migrationBuilder.DropColumn(name: "UnitDetailsId", table: "Transactions");

    // 7. Fremdschlüssel auf Accounts setzen
    migrationBuilder.CreateIndex(
        name: "IX_Transactions_AccountsId",
        table: "Transactions",
        column: "AccountsId");

    migrationBuilder.AddForeignKey(
        name: "FK_Transactions_Accounts_AccountsId",
        table: "Transactions",
        column: "AccountsId",
        principalTable: "Accounts",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);

    // 8. Weitere Indizes für Accounts
    migrationBuilder.CreateIndex(
        name: "IX_Accounts_CostUnitId",
        table: "Accounts",
        column: "CostUnitId");

    migrationBuilder.CreateIndex(
        name: "IX_Accounts_BasicUnitId",
        table: "Accounts",
        column: "BasicUnitId");

    migrationBuilder.CreateIndex(
        name: "IX_Accounts_UnitDetailsId",
        table: "Accounts",
        column: "UnitDetailsId");

    // 9. Optional: Index auf Documentnumber
    migrationBuilder.CreateIndex(
        name: "IX_Transactions_Documentnumber",
        table: "Transactions",
        column: "Documentnumber",
        unique: true);
}
        
    

        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
{
    // 1. Fremdschlüssel und Index auf AccountId entfernen
    migrationBuilder.DropForeignKey(
        name: "FK_Transactions_Accounts_AccountsId",
        table: "Transactions");

    migrationBuilder.DropIndex(
        name: "IX_Transactions_AccountsId",
        table: "Transactions");

    // 2. Alte Spalten wiederherstellen
    migrationBuilder.AddColumn<int>(
        name: "CostUnitId",
        table: "Transactions",
        type: "int",
        nullable: false,
        defaultValue: 0);

    migrationBuilder.AddColumn<int>(
        name: "BasicUnitId",
        table: "Transactions",
        type: "int",
        nullable: false,
        defaultValue: 0);

    migrationBuilder.AddColumn<int>(
        name: "UnitDetailsId",
        table: "Transactions",
        type: "int",
        nullable: true);

    // 3. Daten aus Accounts zurück in Transactions übertragen
    migrationBuilder.Sql(@"
        UPDATE Transactions T
        JOIN Accounts A ON T.AccountsId = A.Id
        SET T.CostUnitId = A.CostUnitId,
            T.BasicUnitId = A.BasicUnitId,
            T.UnitDetailsId = A.UnitDetailsId;
    ");

    // 4. Fremdschlüssel wiederherstellen
    migrationBuilder.AddForeignKey(
        name: "FK_Transactions_CostUnits_CostUnitId",
        table: "Transactions",
        column: "CostUnitId",
        principalTable: "CostUnits",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);

    migrationBuilder.AddForeignKey(
        name: "FK_Transactions_BasicUnits_BasicUnitId",
        table: "Transactions",
        column: "BasicUnitId",
        principalTable: "BasicUnits",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);

    migrationBuilder.AddForeignKey(
        name: "FK_Transactions_UnitDetails_UnitDetailsId",
        table: "Transactions",
        column: "UnitDetailsId",
        principalTable: "UnitDetails",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);

    // 5. AccountId-Spalte entfernen
    migrationBuilder.DropColumn(
        name: "AccountsId",
        table: "Transactions");

    // 6. Indizes auf Accounts entfernen
    migrationBuilder.DropIndex(
        name: "IX_Accounts_CostUnitId",
        table: "Accounts");

    migrationBuilder.DropIndex(
        name: "IX_Accounts_BasicUnitId",
        table: "Accounts");

    migrationBuilder.DropIndex(
        name: "IX_Accounts_UnitDetailsId",
        table: "Accounts");

    // 7. Accounts-Tabelle löschen
    migrationBuilder.DropTable(name: "Accounts");

    // 8. Optional: Index auf Documentnumber entfernen
    migrationBuilder.DropIndex(
        name: "IX_Transactions_Documentnumber",
        table: "Transactions");
}

    }
}
