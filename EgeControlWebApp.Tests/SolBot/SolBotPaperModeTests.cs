using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using EgeControlWebApp.Data;
using EgeControlWebApp.Modules.SolBot;

namespace EgeControlWebApp.Tests.SolBot;

public class SolBotPaperModeTests
{
    [Fact]
    public async Task PaperExecutionAdapter_PersistsPaperLifecycle()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        try
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            await using var context = new ApplicationDbContext(dbOptions);
            await context.Database.EnsureCreatedAsync();

            var options = Options.Create(new SolBotOptions());
            var walletTier = new WalletTierService(options);
            var revenueShare = new RevenueShareService(walletTier);
            var adapter = new PaperExecutionAdapter(
                context,
                new SlippagePricingEngine(walletTier),
                new HeliusPriorityFeeEstimator(options),
                new JitoTipCalculator(options),
                revenueShare,
                options,
                NullLogger<PaperExecutionAdapter>.Instance);

            var riskProfile = new PositionRiskProfile(0.1m, 0.05m, new[] { new TakeProfitLevel(0.2m, 1m) });
            var buyRequest = new ExecutionRequest(
                TradingMode.Paper,
                TradeSide.Buy,
                "mint-1",
                "pool-1",
                DexKind.RaydiumV4,
                1m,
                new MarketDepthSnapshot(100m, 1000m, 30m, 100m, 100m, 15m),
                riskProfile,
                10_000m,
                100m,
                0.3m,
                new PriorityFeeSnapshot(10_000m, 20_000m, 30_000m, 40_000m),
                true,
                WalletAddress: "paper-wallet");

            var buyResult = await adapter.ExecuteAsync(buyRequest);

            Assert.True(buyResult.Success);
            Assert.NotNull(buyResult.PositionId);

            var sellRequest = buyRequest with
            {
                Side = TradeSide.Sell,
                InputAmount = buyResult.OutputAmount,
                MarketDepth = new MarketDepthSnapshot(160m, 800m, 30m, 100m, 100m, 15m),
                CorrelationId = "take_profit_1"
            };

            var sellResult = await adapter.ExecuteAsync(sellRequest);

            Assert.True(sellResult.Success);
            Assert.True(sellResult.RealizedPnlSol > 0m);
            Assert.Equal(2, await context.SolBotTrades.CountAsync());
            Assert.Equal(2, await context.SolBotFills.CountAsync());
            Assert.Equal(2, await context.SolBotMetrics.CountAsync());
            Assert.Equal(2, await context.SolBotBalanceSnapshots.CountAsync());

            var position = await context.SolBotPositions.SingleAsync();
            Assert.Equal("Closed", position.Status);
            Assert.Equal(0m, position.RemainingQuantityTokens);
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }
}
