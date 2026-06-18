using Microsoft.Extensions.Options;
using EgeControlWebApp.Modules.SolBot;

namespace EgeControlWebApp.Tests.SolBot;

public class SolBotPricingTests
{
    private readonly SolBotOptions _options = new();

    [Fact]
    public void SlippagePricingEngine_AccountsForSlippageFeesAndDiscounts()
    {
        var walletTier = new WalletTierService(Options.Create(_options));
        var engine = new SlippagePricingEngine(walletTier);
        var request = new ExecutionRequest(
            TradingMode.Paper,
            TradeSide.Buy,
            "mint-1",
            "pool-1",
            DexKind.RaydiumV4,
            1m,
            new MarketDepthSnapshot(100m, 1_000m, 30m, 200m, 300m, 15m),
            new PositionRiskProfile(0.1m, 0.05m, new[] { new TakeProfitLevel(0.2m, 1m) }),
            10_000m,
            100m,
            0.4m,
            new PriorityFeeSnapshot(10_000m, 20_000m, 40_000m, 80_000m),
            true);

        var result = engine.Simulate(request, 40_000m, 50_000L, 100m);

        Assert.True(result.OutputAmount > 0m);
        Assert.True(result.SlippageBps > 0m);
        Assert.True(result.CommissionSol < 0.01m); // pro tier discount lowers commission below base 1%
    }

    [Fact]
    public void FeeEstimators_RespectBoundsAndUrgency()
    {
        var priority = new HeliusPriorityFeeEstimator(Options.Create(_options));
        var jito = new JitoTipCalculator(Options.Create(_options));

        var fee = priority.Estimate(new PriorityFeeSnapshot(1m, 2m, 999_999m, 2_000_000m), urgent: true);
        var tip = jito.CalculateLamports(5m, 1.0, urgent: true);

        Assert.Equal(_options.Execution.MaximumPriorityFeeMicroLamports, fee.MicroLamports);
        Assert.InRange(tip, (long)_options.Execution.MinimumJitoTipLamports, (long)_options.Execution.MaximumJitoTipLamports);
    }

    [Fact]
    public void WalletTierService_And_RevenueShare_ApplyDiscounts()
    {
        var walletTier = new WalletTierService(Options.Create(_options));
        var revenueShare = new RevenueShareService(walletTier);

        var accrual = revenueShare.CreateAccrual("wallet-1", 0.5m, 50_000m);

        Assert.Equal(WalletTier.Elite, accrual.Tier);
        Assert.True(accrual.DiscountedCommissionSol < accrual.GrossCommissionSol);
        Assert.True(accrual.RevenueShareAccruedSol > 0m);
    }
}
