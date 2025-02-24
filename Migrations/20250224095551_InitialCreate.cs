using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockchainTest.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BlockNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    BlockHash = table.Column<string>(type: "TEXT", nullable: false),
                    PreviousBlockHash = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NextBlockId = table.Column<int>(type: "INTEGER", nullable: true),
                    BlockChainId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blocks_Blocks_NextBlockId",
                        column: x => x.NextBlockId,
                        principalTable: "Blocks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClaimNumber = table.Column<string>(type: "TEXT", nullable: false),
                    SettlementAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    SettlementDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CarRegistration = table.Column<string>(type: "TEXT", nullable: false),
                    Mileage = table.Column<int>(type: "INTEGER", nullable: false),
                    ClaimType = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionHash = table.Column<string>(type: "TEXT", nullable: false),
                    BlockId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_BlockHash",
                table: "Blocks",
                column: "BlockHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_BlockNumber",
                table: "Blocks",
                column: "BlockNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_NextBlockId",
                table: "Blocks",
                column: "NextBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BlockId",
                table: "Transactions",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionHash",
                table: "Transactions",
                column: "TransactionHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Blocks");
        }
    }
}
