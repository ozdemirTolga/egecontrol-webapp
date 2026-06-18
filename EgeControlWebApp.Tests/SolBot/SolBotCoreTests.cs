using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using EgeControlWebApp.Modules.SolBot;

namespace EgeControlWebApp.Tests.SolBot;

public class SolBotCoreTests
{
    private static SolBotOptions CreateOptions()
        => new();

    [Fact]
    public void TokenFilterEngine_RejectsUnsafeToken()
    {
        var engine = new TokenFilterEngine(Options.Create(CreateOptions()), NullLogger<TokenFilterEngine>.Instance);
        var snapshot = new TokenSecuritySnapshot(
            "mint-1",
            RugCheckScore: 82m,
            MintAuthorityEnabled: true,
            FreezeAuthorityEnabled: true,
            LiquiditySol: 5m,
            TopHolderPercent: 35m,
            TopTenHolderPercent: 80m,
            UniqueHolders: 10,
            WashTradingScore: 50m);

        var decision = engine.Evaluate(snapshot);

        Assert.Equal(FilterStatus.Reject, decision.Status);
        Assert.Contains("mint_authority_enabled", decision.Reasons);
        Assert.Contains("wash_trading_detected", decision.Reasons);
    }

    [Fact]
    public void SniperStrategyEngine_CreatesSignal_ForApprovedFirstBlockEvent()
    {
        var engine = new SniperStrategyEngine(Options.Create(CreateOptions()));
        var marketEvent = new NormalizedSolanaEvent(
            "event-1",
            "helius",
            123,
            DateTimeOffset.UtcNow,
            DexKind.RaydiumV4,
            SolBotEventType.PoolCreated,
            "mint-1",
            "pool-1",
            100m,
            "{}");

        var signal = engine.TryCreateSignal(marketEvent, new FilterDecision(FilterStatus.Allow, 90m, Array.Empty<string>()), 10m, 0);

        Assert.NotNull(signal);
        Assert.Equal(1m, signal!.AllocationSol);
        Assert.True(signal.FirstBlockOpportunity);
    }

    [Fact]
    public void RiskEngine_UsesTakeProfit_ThenTrailingStop()
    {
        var engine = new RiskEngine();
        var profile = new PositionRiskProfile(
            0.1m,
            0.05m,
            new[]
            {
                new TakeProfitLevel(0.2m, 0.5m),
                new TakeProfitLevel(0.5m, 0.5m)
            });

        var position = new PositionState(Guid.NewGuid(), "mint-1", 10m, 1m, 1m, 10m, new HashSet<int>());
        var takeProfit = engine.Evaluate(position, profile, 1.25m);
        var trailing = engine.Evaluate(position with { HighestPriceSeenSol = 1.5m, RemainingQuantityTokens = 5m, TriggeredTakeProfitIndexes = new HashSet<int> { 0 } }, profile, 1.40m);

        Assert.Equal(RiskTriggerType.TakeProfit, takeProfit.Trigger);
        Assert.Equal(5m, takeProfit.QuantityToExitTokens);
        Assert.Equal(RiskTriggerType.TrailingStop, trailing.Trigger);
        Assert.Equal(5m, trailing.QuantityToExitTokens);
    }
}
