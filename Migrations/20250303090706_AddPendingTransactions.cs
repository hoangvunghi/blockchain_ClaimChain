using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockchainTest.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingTransactions",
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
                    TransactionHash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingTransactions_TransactionHash",
                table: "PendingTransactions",
                column: "TransactionHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingTransactions");
        }
    }
}
