using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EgeControlWebApp.Data;
using EgeControlWebApp.Modules.SolBot.Persistence;

namespace EgeControlWebApp.Modules.SolBot;

public sealed class HeliusPriorityFeeEstimator : IPriorityFeeEstimator
{
    private readonly SolBotExecutionOptions _options;

    public HeliusPriorityFeeEstimator(IOptions<SolBotOptions> options)
    {
        _options = options.Value.Execution;
    }

    public PriorityFeeQuote Estimate(PriorityFeeSnapshot snapshot, bool urgent)
    {
        var chosen = urgent ? snapshot.High : snapshot.Medium;
        chosen = Math.Clamp(chosen, _options.MinimumPriorityFeeMicroLamports, _options.MaximumPriorityFeeMicroLamports);
        return new PriorityFeeQuote(chosen, _options.DefaultComputeUnitLimit);
    }
}

public sealed class JitoTipCalculator : IJitoTipCalculator
{
    private readonly SolBotExecutionOptions _options;

    public JitoTipCalculator(IOptions<SolBotOptions> options)
    {
        _options = options.Value.Execution;
    }

    public long CalculateLamports(decimal expectedEdgeSol, double congestionLevel, bool urgent)
    {
        var floor = _options.MinimumJitoTipLamports;
        var multiplier = 1m + (decimal)Math.Max(0, congestionLevel);
        if (urgent)
        {
            multiplier += 0.5m;
        }

        multiplier += Math.Min(expectedEdgeSol, 5m) * 0.1m;
        var lamports = floor * multiplier;
        lamports = Math.Clamp(lamports, floor, _options.MaximumJitoTipLamports);
        return decimal.ToInt64(decimal.Round(lamports, 0, MidpointRounding.AwayFromZero));
    }
}

public sealed class SlippagePricingEngine : ISlippagePricingEngine
{
    private readonly IWalletTierService _walletTierService;

    public SlippagePricingEngine(IWalletTierService walletTierService)
    {
        _walletTierService = walletTierService;
    }

    public PricingSimulationResult Simulate(ExecutionRequest request, decimal priorityFeeMicroLamports, long jitoTipLamports, decimal commissionBps)
    {
        var marketDepth = request.MarketDepth;
        var poolFeeFraction = marketDepth.PoolFeeBps / 10_000m;
        var latencyFraction = marketDepth.LatencyBps / 10_000m;
        var tierDiscount = _walletTierService.ResolveCommissionDiscountBps(request.WalletSolBotBalance) / 10_000m;
        var effectiveCommissionBps = commissionBps * (1 - tierDiscount);

        decimal inputAmount = request.InputAmount;
        decimal outputAmount;
        decimal executionPrice;
        decimal slippageBps;
        decimal poolFeeSol;
        decimal tokenTaxSol;

        if (request.Side == TradeSide.Buy)
        {
            var effectiveInput = inputAmount * (1 - poolFeeFraction);
            var k = marketDepth.BaseReserveSol * marketDepth.QuoteReserveTokens;
            var newBaseReserve = marketDepth.BaseReserveSol + effectiveInput;
            var newQuoteReserve = k / newBaseReserve;
            outputAmount = Math.Max(0m, marketDepth.QuoteReserveTokens - newQuoteReserve);
            var taxFraction = marketDepth.BuyTaxBps / 10_000m;
            outputAmount *= 1 - taxFraction;
            executionPrice = outputAmount == 0 ? 0 : inputAmount / outputAmount;
            var quotedPrice = marketDepth.QuoteReserveTokens == 0 ? 0 : marketDepth.BaseReserveSol / marketDepth.QuoteReserveTokens;
            slippageBps = quotedPrice == 0 ? 0 : ((executionPrice / quotedPrice) - 1) * 10_000m + marketDepth.LatencyBps;
            poolFeeSol = inputAmount - effectiveInput;
            tokenTaxSol = inputAmount * taxFraction;
        }
        else
        {
            var effectiveInput = inputAmount * (1 - (marketDepth.SellTaxBps / 10_000m));
            var k = marketDepth.BaseReserveSol * marketDepth.QuoteReserveTokens;
            var newQuoteReserve = marketDepth.QuoteReserveTokens + effectiveInput;
            var newBaseReserve = k / newQuoteReserve;
            outputAmount = Math.Max(0m, marketDepth.BaseReserveSol - newBaseReserve);
            var poolFee = outputAmount * poolFeeFraction;
            outputAmount -= poolFee;
            executionPrice = inputAmount == 0 ? 0 : outputAmount / inputAmount;
            var quotedPrice = marketDepth.QuoteReserveTokens == 0 ? 0 : marketDepth.BaseReserveSol / marketDepth.QuoteReserveTokens;
            slippageBps = quotedPrice == 0 ? 0 : (1 - (executionPrice / quotedPrice)) * 10_000m + marketDepth.LatencyBps;
            poolFeeSol = poolFee;
            tokenTaxSol = inputAmount * (marketDepth.SellTaxBps / 10_000m) * quotedPrice;
        }

        var networkFeeSol = priorityFeeMicroLamports / 1_000_000_000_000m;
        var jitoTipSol = jitoTipLamports / 1_000_000_000m;
        var commissionSol = (request.Side == TradeSide.Buy ? inputAmount : outputAmount) * (effectiveCommissionBps / 10_000m);
        slippageBps += latencyFraction * 10_000m;

        return new PricingSimulationResult(inputAmount, outputAmount, executionPrice, Math.Max(0m, slippageBps), poolFeeSol, tokenTaxSol, networkFeeSol, jitoTipSol, commissionSol);
    }
}

public sealed class EncryptedPrivateKeyStore : IEncryptedPrivateKeyStore
{
    private readonly IDataProtector _protector;

    public EncryptedPrivateKeyStore(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("SolBot.PrivateKeys.v1");
    }

    public string Protect(string privateKey)
    {
        if (string.IsNullOrWhiteSpace(privateKey))
        {
            throw new ArgumentException("Private key is required.", nameof(privateKey));
        }

        return _protector.Protect(privateKey);
    }

    public string Unprotect(string encryptedPrivateKey)
    {
        if (string.IsNullOrWhiteSpace(encryptedPrivateKey))
        {
            throw new ArgumentException("Encrypted private key is required.", nameof(encryptedPrivateKey));
        }

        return _protector.Unprotect(encryptedPrivateKey);
    }
}

public sealed class RugCheckClient : IRugCheckClient
{
    private readonly HttpClient _httpClient;

    public RugCheckClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RugCheckAssessment> GetAssessmentAsync(string tokenMint, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"/v1/tokens/{tokenMint}/report/summary", cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        var score = root.TryGetProperty("score", out var scoreProp) && scoreProp.TryGetDecimal(out var value) ? value : 0m;
        var warnings = new List<string>();
        if (root.TryGetProperty("warnings", out var warningsProp) && warningsProp.ValueKind == JsonValueKind.Array)
        {
            warnings.AddRange(warningsProp.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString()!).Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        return new RugCheckAssessment(tokenMint, score, warnings);
    }
}

public sealed class HttpJitoBundleSubmitter : IJitoBundleSubmitter
{
    private readonly HttpClient _httpClient;

    public HttpJitoBundleSubmitter(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BundleSubmissionReceipt> SubmitAsync(BundleSubmissionRequest request, CancellationToken cancellationToken = default)
    {
        var rpcPayload = new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "sendBundle",
            @params = new object[]
            {
                new[] { request.UnsignedTransaction },
                new
                {
                    tip = request.JitoTipLamports,
                    computeUnitPriceMicroLamports = request.PriorityFeeMicroLamports,
                    computeUnitLimit = request.ComputeUnitLimit
                }
            }
        };

        var payloadJson = JsonSerializer.Serialize(rpcPayload);
        using var response = await _httpClient.PostAsync("", new StringContent(payloadJson, Encoding.UTF8, "application/json"), cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        string bundleId;
        try
        {
            using var document = JsonDocument.Parse(responseJson);
            bundleId = document.RootElement.TryGetProperty("result", out var resultElement)
                ? resultElement.GetString() ?? $"bundle-{Guid.NewGuid():N}"
                : $"bundle-{Guid.NewGuid():N}";
        }
        catch (JsonException)
        {
            bundleId = $"bundle-{Guid.NewGuid():N}";
        }

        return new BundleSubmissionReceipt(bundleId, true, _httpClient.BaseAddress?.ToString() ?? string.Empty, payloadJson);
    }
}

public sealed class PaperExecutionAdapter : IExecutionAdapter
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ISlippagePricingEngine _pricingEngine;
    private readonly IPriorityFeeEstimator _priorityFeeEstimator;
    private readonly IJitoTipCalculator _jitoTipCalculator;
    private readonly IRevenueShareService _revenueShareService;
    private readonly SolBotOptions _options;
    private readonly ILogger<PaperExecutionAdapter> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public PaperExecutionAdapter(
        ApplicationDbContext dbContext,
        ISlippagePricingEngine pricingEngine,
        IPriorityFeeEstimator priorityFeeEstimator,
        IJitoTipCalculator jitoTipCalculator,
        IRevenueShareService revenueShareService,
        IOptions<SolBotOptions> options,
        ILogger<PaperExecutionAdapter> logger)
    {
        _dbContext = dbContext;
        _pricingEngine = pricingEngine;
        _priorityFeeEstimator = priorityFeeEstimator;
        _jitoTipCalculator = jitoTipCalculator;
        _revenueShareService = revenueShareService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ExecutionResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default)
    {
        var feeQuote = _priorityFeeEstimator.Estimate(request.PriorityFeeSnapshot, request.FirstBlockOpportunity);
        var tipLamports = _jitoTipCalculator.CalculateLamports(request.ExpectedEdgeSol, request.FirstBlockOpportunity ? 1.0 : 0.25, request.FirstBlockOpportunity);
        var pricing = _pricingEngine.Simulate(request, feeQuote.MicroLamports, tipLamports, _options.Execution.BaseCommissionBps);
        var now = DateTimeOffset.UtcNow;
        decimal realizedPnl = 0m;
        Guid? positionId;

        if (request.Side == TradeSide.Buy)
        {
            if (request.AvailableCashSol < request.InputAmount + pricing.NetworkFeeSol + pricing.JitoTipSol + pricing.CommissionSol)
            {
                throw new InvalidOperationException("Insufficient virtual SOL balance for paper execution.");
            }

            var position = await _dbContext.SolBotPositions
                .SingleOrDefaultAsync(item => item.TokenMint == request.TokenMint && item.Status == "Open", cancellationToken);

            if (position is null)
            {
                position = new SolBotPositionRecord
                {
                    TokenMint = request.TokenMint,
                    PoolAddress = request.PoolAddress,
                    Dex = request.Dex.ToString(),
                    QuantityTokens = pricing.OutputAmount,
                    RemainingQuantityTokens = pricing.OutputAmount,
                    AverageEntryPriceSol = pricing.ExecutionPriceSol,
                    HighestObservedPriceSol = pricing.ExecutionPriceSol,
                    StopLossPercent = request.RiskProfile.StopLossPercent,
                    TrailingStopPercent = request.RiskProfile.TrailingStopPercent,
                    TakeProfitPlanJson = JsonSerializer.Serialize(request.RiskProfile.TakeProfits, JsonOptions),
                    TriggeredTakeProfitsJson = JsonSerializer.Serialize(Array.Empty<int>(), JsonOptions),
                    OpenedAt = now,
                    Status = "Open"
                };
                _dbContext.SolBotPositions.Add(position);
            }
            else
            {
                var existingCost = position.AverageEntryPriceSol * position.QuantityTokens;
                var newCost = pricing.ExecutionPriceSol * pricing.OutputAmount;
                position.QuantityTokens += pricing.OutputAmount;
                position.RemainingQuantityTokens += pricing.OutputAmount;
                position.AverageEntryPriceSol = position.QuantityTokens == 0 ? 0 : (existingCost + newCost) / position.QuantityTokens;
                position.HighestObservedPriceSol = Math.Max(position.HighestObservedPriceSol, pricing.ExecutionPriceSol);
            }

            positionId = position.Id;
        }
        else
        {
            var position = await _dbContext.SolBotPositions
                .SingleOrDefaultAsync(item => item.TokenMint == request.TokenMint && item.Status == "Open", cancellationToken)
                ?? throw new InvalidOperationException("Open paper position not found for sell execution.");

            if (position.RemainingQuantityTokens < request.InputAmount)
            {
                throw new InvalidOperationException("Cannot sell more tokens than the open paper position holds.");
            }

            position.RemainingQuantityTokens -= request.InputAmount;
            position.HighestObservedPriceSol = Math.Max(position.HighestObservedPriceSol, pricing.ExecutionPriceSol);
            position.LastExitTrigger = request.CorrelationId ?? "manual";
            if (position.RemainingQuantityTokens == 0)
            {
                position.Status = "Closed";
                position.ClosedAt = now;
            }

            realizedPnl = pricing.OutputAmount - (request.InputAmount * position.AverageEntryPriceSol) - pricing.FeesTotal();
            positionId = position.Id;
        }

        var trade = new SolBotTradeRecord
        {
            CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString("N"),
            Mode = request.Mode.ToString(),
            Side = request.Side.ToString(),
            TokenMint = request.TokenMint,
            PoolAddress = request.PoolAddress,
            Dex = request.Dex.ToString(),
            InputAmount = pricing.InputAmount,
            OutputAmount = pricing.OutputAmount,
            ExecutionPriceSol = pricing.ExecutionPriceSol,
            FeesSol = pricing.FeesTotal(),
            RealizedPnlSol = realizedPnl,
            PositionId = positionId,
            Status = "Filled",
            CreatedAt = now,
            RevenueShareAccrualSol = _revenueShareService.CreateAccrual(request.WalletAddress ?? "paper-wallet", pricing.CommissionSol, request.WalletSolBotBalance).RevenueShareAccruedSol
        };

        _dbContext.SolBotTrades.Add(trade);
        _dbContext.SolBotFills.Add(new SolBotFillRecord
        {
            Trade = trade,
            FillType = request.Side.ToString(),
            Quantity = request.Side == TradeSide.Buy ? pricing.OutputAmount : pricing.InputAmount,
            PriceSol = pricing.ExecutionPriceSol,
            SlippageBps = pricing.SlippageBps,
            PoolFeeSol = pricing.PoolFeeSol,
            NetworkFeeSol = pricing.NetworkFeeSol,
            JitoTipSol = pricing.JitoTipSol,
            CommissionSol = pricing.CommissionSol,
            TokenTaxSol = pricing.TokenTaxSol,
            CreatedAt = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await PersistSnapshotAsync(request.TokenMint, now, cancellationToken);
        _dbContext.SolBotMetrics.Add(new SolBotMetricRecord
        {
            Name = request.Side == TradeSide.Buy ? "paper_buy" : "paper_sell",
            Value = request.Side == TradeSide.Buy ? pricing.InputAmount : pricing.OutputAmount,
            TagsJson = JsonSerializer.Serialize(new { request.TokenMint, request.Dex, request.Mode }, JsonOptions),
            RecordedAt = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        SolBotMetrics.ExecutedTradesCounter.Add(1);
        SolBotMetrics.SlippageHistogram.Record((double)pricing.SlippageBps);
        SolBotMetrics.RealizedPnlHistogram.Record((double)realizedPnl);
        _logger.LogInformation("Executed paper {Side} for {TokenMint} at {ExecutionPriceSol} SOL", request.Side, request.TokenMint, pricing.ExecutionPriceSol);

        return new ExecutionResult(true, request.Side, request.TokenMint, pricing.InputAmount, pricing.OutputAmount, pricing.ExecutionPriceSol, pricing.FeesTotal(), realizedPnl, positionId, request.Mode.ToString(), "paper_filled", trade.CorrelationId);
    }

    private async Task PersistSnapshotAsync(string tokenMint, DateTimeOffset timestamp, CancellationToken cancellationToken)
    {
        var openPositions = await _dbContext.SolBotPositions.Where(item => item.Status == "Open").ToListAsync(cancellationToken);
        var unrealized = openPositions.Sum(position => position.RemainingQuantityTokens * position.AverageEntryPriceSol);
        var realized = await _dbContext.SolBotTrades.SumAsync(item => item.RealizedPnlSol, cancellationToken);
        var totalFees = await _dbContext.SolBotTrades.SumAsync(item => item.FeesSol, cancellationToken);
        var buys = await _dbContext.SolBotTrades.Where(item => item.Side == nameof(TradeSide.Buy)).SumAsync(item => item.InputAmount, cancellationToken);
        var sells = await _dbContext.SolBotTrades.Where(item => item.Side == nameof(TradeSide.Sell)).SumAsync(item => item.OutputAmount, cancellationToken);
        var cashBalance = _options.Paper.InitialVirtualBalanceSol - buys + sells - totalFees;

        _dbContext.SolBotBalanceSnapshots.Add(new SolBotBalanceSnapshotRecord
        {
            TokenMint = tokenMint,
            CashBalanceSol = cashBalance,
            UnrealizedPnlSol = unrealized,
            RealizedPnlSol = realized,
            EquitySol = cashBalance + unrealized,
            CreatedAt = timestamp
        });
    }
}

public sealed class LiveExecutionAdapter : IExecutionAdapter
{
    private readonly IPriorityFeeEstimator _priorityFeeEstimator;
    private readonly IJitoTipCalculator _jitoTipCalculator;
    private readonly IJitoBundleSubmitter _bundleSubmitter;
    private readonly IEncryptedPrivateKeyStore _privateKeyStore;

    public LiveExecutionAdapter(
        IPriorityFeeEstimator priorityFeeEstimator,
        IJitoTipCalculator jitoTipCalculator,
        IJitoBundleSubmitter bundleSubmitter,
        IEncryptedPrivateKeyStore privateKeyStore)
    {
        _priorityFeeEstimator = priorityFeeEstimator;
        _jitoTipCalculator = jitoTipCalculator;
        _bundleSubmitter = bundleSubmitter;
        _privateKeyStore = privateKeyStore;
    }

    public async Task<ExecutionResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UnsignedTransaction))
        {
            throw new InvalidOperationException("Live execution requires an unsigned transaction payload.");
        }

        if (string.IsNullOrWhiteSpace(request.EncryptedPrivateKey))
        {
            throw new InvalidOperationException("Live execution requires an encrypted private key.");
        }

        _ = SHA256.HashData(Encoding.UTF8.GetBytes(_privateKeyStore.Unprotect(request.EncryptedPrivateKey)));

        var feeQuote = _priorityFeeEstimator.Estimate(request.PriorityFeeSnapshot, request.FirstBlockOpportunity);
        var tipLamports = _jitoTipCalculator.CalculateLamports(request.ExpectedEdgeSol, request.FirstBlockOpportunity ? 1.0 : 0.25, request.FirstBlockOpportunity);
        var receipt = await _bundleSubmitter.SubmitAsync(
            new BundleSubmissionRequest(request.UnsignedTransaction, tipLamports, feeQuote.MicroLamports, feeQuote.ComputeUnitLimit),
            cancellationToken);

        return new ExecutionResult(true, request.Side, request.TokenMint, request.InputAmount, 0m, 0m, (feeQuote.MicroLamports / 1_000_000_000_000m) + (tipLamports / 1_000_000_000m), 0m, null, request.Mode.ToString(), "bundle_submitted", receipt.BundleId);
    }
}

internal static class PricingSimulationResultExtensions
{
    public static decimal FeesTotal(this PricingSimulationResult pricing)
        => pricing.PoolFeeSol + pricing.TokenTaxSol + pricing.NetworkFeeSol + pricing.JitoTipSol + pricing.CommissionSol;
}
