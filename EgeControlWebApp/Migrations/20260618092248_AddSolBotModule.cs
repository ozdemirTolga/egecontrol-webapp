using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EgeControlWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSolBotModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolBotBalanceSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenMint = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    CashBalanceSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    UnrealizedPnlSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    RealizedPnlSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    EquitySol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolBotBalanceSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolBotEventLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Slot = table.Column<long>(type: "INTEGER", nullable: false),
                    TokenMint = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PayloadJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolBotEventLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolBotMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    TagsJson = table.Column<string>(type: "TEXT", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolBotMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolBotPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenMint = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PoolAddress = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Dex = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    QuantityTokens = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    RemainingQuantityTokens = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    AverageEntryPriceSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    HighestObservedPriceSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    StopLossPercent = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    TrailingStopPercent = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    TakeProfitPlanJson = table.Column<string>(type: "TEXT", nullable: false),
                    TriggeredTakeProfitsJson = table.Column<string>(type: "TEXT", nullable: false),
                    LastExitTrigger = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    OpenedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolBotPositions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolBotTrades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Mode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Side = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    TokenMint = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PoolAddress = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Dex = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    InputAmount = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    OutputAmount = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    ExecutionPriceSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    FeesSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    RealizedPnlSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    RevenueShareAccrualSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    PositionId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolBotTrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolBotTrades_SolBotPositions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "SolBotPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SolBotFills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TradeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FillType = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    PriceSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    SlippageBps = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    PoolFeeSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    NetworkFeeSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    JitoTipSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    CommissionSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    TokenTaxSol = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolBotFills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolBotFills_SolBotTrades_TradeId",
                        column: x => x.TradeId,
                        principalTable: "SolBotTrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolBotFills_TradeId",
                table: "SolBotFills",
                column: "TradeId");

            migrationBuilder.CreateIndex(
                name: "IX_SolBotPositions_TokenMint_Status",
                table: "SolBotPositions",
                columns: new[] { "TokenMint", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SolBotTrades_PositionId",
                table: "SolBotTrades",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolBotTrades_TokenMint",
                table: "SolBotTrades",
                column: "TokenMint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolBotBalanceSnapshots");

            migrationBuilder.DropTable(
                name: "SolBotEventLogs");

            migrationBuilder.DropTable(
                name: "SolBotFills");

            migrationBuilder.DropTable(
                name: "SolBotMetrics");

            migrationBuilder.DropTable(
                name: "SolBotTrades");

            migrationBuilder.DropTable(
                name: "SolBotPositions");
        }
    }
}
