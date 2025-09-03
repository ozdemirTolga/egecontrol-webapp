using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EgeControlWebApp.Models;

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
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(q => q.LastModifiedByUser)
                .WithMany()
                .HasForeignKey(q => q.LastModifiedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // QuoteItem precision configuration
        modelBuilder.Entity<QuoteItem>(entity =>
        {
            entity.Property(qi => qi.UnitPrice)
                .HasPrecision(18, 2);

            entity.Property(qi => qi.DiscountPercentage)
                .HasPrecision(5, 2);

            entity.Property(qi => qi.DiscountAmount)
                .HasPrecision(18, 2);

            entity.Property(qi => qi.Total)
                .HasPrecision(18, 2);
        });

        // Seed data
        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                Id = 1,
                CompanyName = "ABC Teknoloji Ltd. Şti.",
                ContactPerson = "Ahmet Yılmaz",
                Email = "ahmet@abcteknoloji.com",
                Phone = "+90 212 555 0101",
                Address = "Ataşehir Mah. Mustafa Kemal Cad. No:123",
                City = "İstanbul",
                Country = "Türkiye",
                TaxNumber = "1234567890",
                TaxOffice = "Ataşehir Vergi Dairesi",
                CreatedAt = new DateTime(2024, 7, 10),
                UpdatedAt = new DateTime(2024, 7, 10),
                IsActive = true
            },
            new Customer
            {
                Id = 2,
                CompanyName = "XYZ Mühendislik A.Ş.",
                ContactPerson = "Fatma Demir",
                Email = "fatma@xyzmuhendislik.com",
                Phone = "+90 312 555 0202",
                Address = "Çankaya Mah. Atatürk Bulvarı No:456",
                City = "Ankara",
                Country = "Türkiye",
                TaxNumber = "0987654321",
                TaxOffice = "Çankaya Vergi Dairesi",
                CreatedAt = new DateTime(2024, 7, 25),
                UpdatedAt = new DateTime(2024, 7, 25),
                IsActive = true
            }
        );
    }
}