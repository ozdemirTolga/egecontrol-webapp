using System.Net;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using EgeControlWebApp.Modules.SolBot;

namespace EgeControlWebApp.Tests.SolBot;

public class SolBotIntegrationTests
{
    [Fact]
    public void SolanaEventNormalizer_Normalizes_Helius_And_Yellowstone_Payloads()
    {
        var normalizer = new SolanaEventNormalizer();

        var helius = normalizer.Normalize("helius", """
        {
          "source": "helius",
          "eventId": "abc123",
          "slot": 321,
          "dex": "raydium",
          "eventType": "pool_create",
          "tokenMint": "mint-1",
          "poolAddress": "pool-1",
          "liquiditySol": 42.5,
          "observedAt": "2026-06-18T09:15:19Z"
        }
        """);

        var yellowstone = normalizer.Normalize("yellowstone", """
        {
          "source": "yellowstone",
          "slot": 654,
          "signature": "sig-1",
          "observedAt": "2026-06-18T09:15:20Z",
          "instruction": {
            "programId": "6EF8rrecthR5Dkzon8Nwu78hRvfCKubJ14M5uBEwF6P",
            "eventType": "pool_create",
            "accounts": {
              "tokenMint": "mint-2",
              "poolAddress": "pool-2",
              "liquiditySol": 55.1
            }
          }
        }
        """);

        Assert.Equal(DexKind.RaydiumV4, helius.Dex);
        Assert.Equal(SolBotEventType.PoolCreated, helius.EventType);
        Assert.Equal(DexKind.PumpFun, yellowstone.Dex);
        Assert.Equal("pool-2", yellowstone.PoolAddress);
    }

    [Fact]
    public async Task LiveExecutionAdapter_SubmitsBundle_WithEncryptedKey()
    {
        var options = Options.Create(new SolBotOptions());
        var priority = new HeliusPriorityFeeEstimator(options);
        var jito = new JitoTipCalculator(options);
        var submitter = new FakeBundleSubmitter();
        var protector = DataProtectionProvider.Create(new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"solbot-tests-{Guid.NewGuid():N}")));
        var keyStore = new EncryptedPrivateKeyStore(protector);
        var adapter = new LiveExecutionAdapter(priority, jito, submitter, keyStore);

        var result = await adapter.ExecuteAsync(new ExecutionRequest(
            TradingMode.Live,
            TradeSide.Buy,
            "mint-1",
            "pool-1",
            DexKind.Meteora,
            1m,
            new MarketDepthSnapshot(100m, 1000m, 30m, 0m, 0m, 15m),
            new PositionRiskProfile(0.1m, 0.05m, new[] { new TakeProfitLevel(0.2m, 1m) }),
            0m,
            0m,
            0.5m,
            new PriorityFeeSnapshot(10_000m, 20_000m, 30_000m, 40_000m),
            true,
            WalletAddress: "wallet-1",
            EncryptedPrivateKey: keyStore.Protect("secret-key"),
            UnsignedTransaction: Convert.ToBase64String(Encoding.UTF8.GetBytes("tx")),
            CorrelationId: "corr-1"));

        Assert.True(result.Success);
        Assert.Equal("bundle-123", result.ExternalReference);
        Assert.Equal(1, submitter.Calls);
        Assert.Equal(350000, submitter.LastRequest!.ComputeUnitLimit);
    }

    private sealed class FakeBundleSubmitter : IJitoBundleSubmitter
    {
        public int Calls { get; private set; }
        public BundleSubmissionRequest? LastRequest { get; private set; }

        public Task<BundleSubmissionReceipt> SubmitAsync(BundleSubmissionRequest request, CancellationToken cancellationToken = default)
        {
            Calls++;
            LastRequest = request;
            return Task.FromResult(new BundleSubmissionReceipt("bundle-123", true, "http://localhost", "{}"));
        }
    }
}
