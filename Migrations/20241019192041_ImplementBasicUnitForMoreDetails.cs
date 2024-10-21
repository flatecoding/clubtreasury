using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTCCashRegister.Migrations
{
    /// <inheritdoc />
    public partial class ImplementBasicUnitForMoreDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CashRegisters",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrentBalance = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashRegisters", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CostUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CostUnitName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostUnits", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpecialItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Betrag = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialItems", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BasicUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CostUnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasicUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BasicUnits_CostUnits_CostUnitId",
                        column: x => x.CostUnitId,
                        principalTable: "CostUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UnitDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CostDetails = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BasicUnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitDetails_BasicUnits_BasicUnitId",
                        column: x => x.BasicUnitId,
                        principalTable: "BasicUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Documentnumber = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sum = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AccountMovement = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CostUnitID = table.Column<int>(type: "int", nullable: false),
                    BasicUnitID = table.Column<int>(type: "int", nullable: false),
                    UnitDetailsId = table.Column<int>(type: "int", nullable: false),
                    UnitDetailsD = table.Column<int>(type: "int", nullable: false),
                    CashRegisterID = table.Column<int>(type: "int", nullable: false),
                    SpecialItemID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_BasicUnits_BasicUnitID",
                        column: x => x.BasicUnitID,
                        principalTable: "BasicUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_CashRegisters_CashRegisterID",
                        column: x => x.CashRegisterID,
                        principalTable: "CashRegisters",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_CostUnits_CostUnitID",
                        column: x => x.CostUnitID,
                        principalTable: "CostUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_SpecialItems_SpecialItemID",
                        column: x => x.SpecialItemID,
                        principalTable: "SpecialItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_UnitDetails_UnitDetailsD",
                        column: x => x.UnitDetailsD,
                        principalTable: "UnitDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BasicUnits_CostUnitId",
                table: "BasicUnits",
                column: "CostUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BasicUnitID",
                table: "Transactions",
                column: "BasicUnitID");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CashRegisterID",
                table: "Transactions",
                column: "CashRegisterID");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CostUnitID",
                table: "Transactions",
                column: "CostUnitID");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SpecialItemID",
                table: "Transactions",
                column: "SpecialItemID");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UnitDetailsD",
                table: "Transactions",
                column: "UnitDetailsD");

            migrationBuilder.CreateIndex(
                name: "IX_UnitDetails_BasicUnitId",
                table: "UnitDetails",
                column: "BasicUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "CashRegisters");

            migrationBuilder.DropTable(
                name: "SpecialItems");

            migrationBuilder.DropTable(
                name: "UnitDetails");

            migrationBuilder.DropTable(
                name: "BasicUnits");

            migrationBuilder.DropTable(
                name: "CostUnits");
        }
    }
}
