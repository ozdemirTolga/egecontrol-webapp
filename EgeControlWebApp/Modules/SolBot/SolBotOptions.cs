using System.ComponentModel.DataAnnotations;

namespace EgeControlWebApp.Modules.SolBot;

public sealed class SolBotOptions
{
    public const string SectionName = "SolBot";

    public bool Enabled { get; set; } = true;
    public TradingMode DefaultMode { get; set; } = TradingMode.Paper;
    public SolBotEndpointOptions Endpoints { get; set; } = new();
    public SolBotStrategyOptions Strategy { get; set; } = new();
    public SolBotFilterOptions Filters { get; set; } = new();
    public SolBotRiskOptions Risk { get; set; } = new();
    public SolBotExecutionOptions Execution { get; set; } = new();
    public SolBotPaperOptions Paper { get; set; } = new();
    public SolBotTokenOptions Token { get; set; } = new();
}

public sealed class SolBotEndpointOptions
{
    public string HeliusWebSocketUrl { get; set; } = "wss://mainnet.helius-rpc.com";
    public string QuickNodeWebSocketUrl { get; set; } = "wss://example.solana-mainnet.quiknode.pro";
    public string YellowstoneGrpcUrl { get; set; } = "https://grpc.solana.example";
    public string RugCheckBaseUrl { get; set; } = "https://api.rugcheck.xyz";
    public string JitoBlockEngineUrl { get; set; } = "https://mainnet.block-engine.jito.wtf/api/v1/bundles";
    public string HeliusPriorityFeeUrl { get; set; } = "https://mainnet.helius-rpc.com/?api-key=replace-me";
}

public sealed class SolBotStrategyOptions
{
    [Range(0.01, 1)]
    public decimal AllocationFraction { get; set; } = 0.1m;

    [Range(0.01, 10_000)]
    public decimal MaxAllocationSol { get; set; } = 1m;

    [Range(0.01, 10_000)]
    public decimal MinAllocationSol { get; set; } = 0.1m;

    public bool FirstBlockOnly { get; set; } = true;
}

public sealed class SolBotFilterOptions
{
    [Range(0, 10_000)]
    public decimal MinLiquiditySol { get; set; } = 25m;

    [Range(0, 100)]
    public decimal MaxTopHolderPercent { get; set; } = 18m;

    [Range(0, 100)]
    public decimal MaxTopTenHoldersPercent { get; set; } = 55m;

    [Range(0, 100)]
    public decimal MinSafetyScore { get; set; } = 70m;

    [Range(0, 100)]
    public decimal QuarantineScoreGrace { get; set; } = 5m;

    [Range(0, 100)]
    public decimal MaxWashTradingScore { get; set; } = 35m;

    public int MinUniqueHolders { get; set; } = 25;
    public bool RequireMintAuthorityRevoked { get; set; } = true;
    public bool RequireFreezeAuthorityRevoked { get; set; } = true;
}

public sealed class SolBotRiskOptions
{
    [Range(0.001, 1)]
    public decimal StopLossPercent { get; set; } = 0.12m;

    [Range(0.001, 1)]
    public decimal TrailingStopPercent { get; set; } = 0.08m;

    [Range(1, 10)]
    public int MaxConcurrentPositions { get; set; } = 3;

    [Range(0.001, 1)]
    public decimal DailyLossLimitPercent { get; set; } = 0.2m;

    public List<TakeProfitLevelOptions> TakeProfits { get; set; } =
    [
        new() { GainPercent = 0.2m, ExitFraction = 0.5m },
        new() { GainPercent = 0.5m, ExitFraction = 0.5m }
    ];
}

public sealed class TakeProfitLevelOptions
{
    [Range(0.001, 10)]
    public decimal GainPercent { get; set; } = 0.2m;

    [Range(0.001, 1)]
    public decimal ExitFraction { get; set; } = 0.5m;
}

public sealed class SolBotExecutionOptions
{
    [Range(0, 10_000)]
    public decimal BaseCommissionBps { get; set; } = 100m;

    [Range(0, 10_000)]
    public decimal MinimumPriorityFeeMicroLamports { get; set; } = 25_000m;

    [Range(0, 10_000_000)]
    public decimal MaximumPriorityFeeMicroLamports { get; set; } = 250_000m;

    [Range(0, 10_000_000)]
    public decimal MinimumJitoTipLamports { get; set; } = 25_000m;

    [Range(0, 10_000_000)]
    public decimal MaximumJitoTipLamports { get; set; } = 2_000_000m;

    [Range(1, 2_000_000)]
    public int DefaultComputeUnitLimit { get; set; } = 350_000;
}

public sealed class SolBotPaperOptions
{
    [Range(0, 1000)]
    public decimal DefaultLatencyBps { get; set; } = 15m;

    [Range(0.001, 10_000)]
    public decimal InitialVirtualBalanceSol { get; set; } = 100m;
}

public sealed class SolBotTokenOptions
{
    public string UtilityTokenMint { get; set; } = "SOLBOT";
    public List<SolBotTierOptions> Tiers { get; set; } =
    [
        new() { Tier = WalletTier.Basic, MinimumBalance = 0m, CommissionDiscountBps = 0m },
        new() { Tier = WalletTier.Advanced, MinimumBalance = 1_000m, CommissionDiscountBps = 500m },
        new() { Tier = WalletTier.Pro, MinimumBalance = 10_000m, CommissionDiscountBps = 1_500m },
        new() { Tier = WalletTier.Elite, MinimumBalance = 50_000m, CommissionDiscountBps = 2_500m }
    ];
}

public sealed class SolBotTierOptions
{
    public WalletTier Tier { get; set; } = WalletTier.Basic;
    public decimal MinimumBalance { get; set; }
    public decimal CommissionDiscountBps { get; set; }
}
