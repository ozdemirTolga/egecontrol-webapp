using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EgeControlWebApp.Models;
using EgeControlWebApp.Modules.SolBot.Persistence;

namespace EgeControlWebApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; } = default!;
    public DbSet<Quote> Quotes { get; set; } = default!;
    public DbSet<QuoteItem> QuoteItems { get; set; } = default!;
    public DbSet<ContactMessage> ContactMessages { get; set; } = default!;
    public DbSet<GalleryItem> GalleryItems { get; set; } = default!;
    public DbSet<SiteSetting> SiteSettings { get; set; } = default!;
    public DbSet<VisitorLog> VisitorLogs { get; set; } = default!;
    public DbSet<SolBotTradeRecord> SolBotTrades { get; set; } = default!;
    public DbSet<SolBotFillRecord> SolBotFills { get; set; } = default!;
    public DbSet<SolBotPositionRecord> SolBotPositions { get; set; } = default!;
    public DbSet<SolBotBalanceSnapshotRecord> SolBotBalanceSnapshots { get; set; } = default!;
    public DbSet<SolBotMetricRecord> SolBotMetrics { get; set; } = default!;
    public DbSet<SolBotEventLogRecord> SolBotEventLogs { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Quote precision configuration
        modelBuilder.Entity<Quote>(entity =>
        {
            entity.Property(q => q.SubTotal)
                .HasPrecision(18, 2);

            entity.Property(q => q.VatAmount)
                .HasPrecision(18, 2);

            entity.Property(q => q.TotalAmount)
                .HasPrecision(18, 2);

            entity.Property(q => q.VatRate)
                .HasPrecision(5, 2);

            // User foreign key relationships
            entity.HasOne(q => q.CreatedByUser)
                .WithMany()
                .HasForeignKey(q => q.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(q => q.LastModifiedByUser)
                .WithMany()
                .HasForeignKey(q => q.LastModifiedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // QuoteItem precision configuration
        modelBuilder.Entity<QuoteItem>(entity =>
        {
            entity.Property(qi => qi.UnitPrice)
                .HasPrecision(18, 2);

            entity.Property(qi => qi.Quantity)
                .HasPrecision(18, 2);

            entity.Property(qi => qi.DiscountPercentage)
                .HasPrecision(5, 2);

            entity.Property(qi => qi.DiscountAmount)
                .HasPrecision(18, 2);

            entity.Property(qi => qi.Total)
                .HasPrecision(18, 2);
        });

        modelBuilder.Entity<SolBotTradeRecord>(entity =>
        {
            entity.HasIndex(trade => trade.TokenMint);
            entity.HasOne(trade => trade.Position)
                .WithMany(position => position.Trades)
                .HasForeignKey(trade => trade.PositionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SolBotFillRecord>(entity =>
        {
            entity.HasOne(fill => fill.Trade)
                .WithMany(trade => trade.Fills)
                .HasForeignKey(fill => fill.TradeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SolBotPositionRecord>(entity =>
        {
            entity.HasIndex(position => new { position.TokenMint, position.Status });
        });

        // SQL Server test için geçici olarak seed data kapatıldı
        // Seed data
        // modelBuilder.Entity<Customer>().HasData(
        //     new Customer
        //     {
        //         Id = 1,
        //         CompanyName = "ABC Teknoloji Ltd. Şti.",
        //         ContactPerson = "Ahmet Yılmaz",
        //         Email = "ahmet@abcteknoloji.com",
        //         Phone = "+90 212 555 0101",
        //         Address = "Ataşehir Mah. Mustafa Kemal Cad. No:123",
        //         City = "İstanbul",
        //         Country = "Türkiye",
        //         TaxNumber = "1234567890",
        //         TaxOffice = "Ataşehir Vergi Dairesi",
        //         CreatedAt = new DateTime(2024, 7, 10),
        //         UpdatedAt = new DateTime(2024, 7, 10),
        //         IsActive = true
        //     },
        //     new Customer
        //     {
        //         Id = 2,
        //         CompanyName = "XYZ Mühendislik A.Ş.",
        //         ContactPerson = "Fatma Demir",
        //         Email = "fatma@xyzmuhendislik.com",
        //         Phone = "+90 312 555 0202",
        //         Address = "Çankaya Mah. Atatürk Bulvarı No:456",
        //         City = "Ankara",
        //         Country = "Türkiye",
        //         TaxNumber = "0987654321",
        //         TaxOffice = "Çankaya Vergi Dairesi",
        //         CreatedAt = new DateTime(2024, 7, 25),
        //         UpdatedAt = new DateTime(2024, 7, 25),
        //         IsActive = true
        //     }
        // );
    }
}