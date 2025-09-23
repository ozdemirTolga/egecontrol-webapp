using Microsoft.EntityFrameworkCore;
using EgeControlWebApp.Data;
using EgeControlWebApp.Models;
using Microsoft.Data.Sqlite;
using System.Data;

namespace EgeControlWebApp.Services
{
    public class DataMigrationService
    {
        private readonly ILogger<DataMigrationService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DataMigrationService(ILogger<DataMigrationService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task MigrateFromSqliteToSqlServer(string sqlitePath, string sqlServerConnectionString)
        {
            try
            {
                _logger.LogInformation("SQLite'dan SQL Server'a veri geçişi başlatılıyor...");

                // SQLite'dan verileri oku
                var customers = await ReadCustomersFromSqlite(sqlitePath);
                var quotes = await ReadQuotesFromSqlite(sqlitePath);
                var quoteItems = await ReadQuoteItemsFromSqlite(sqlitePath);
                var contactMessages = await ReadContactMessagesFromSqlite(sqlitePath);

                // SQL Server'a yazı
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Mevcut verileri temizle (opsiyonel)
                // await context.Database.ExecuteSqlRawAsync("DELETE FROM ContactMessages; DELETE FROM QuoteItems; DELETE FROM Quotes; DELETE FROM Customers;");

                // Customers
                if (customers.Any())
                {
                    await context.Customers.AddRangeAsync(customers);
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"{customers.Count} müşteri aktarıldı.");
                }

                // Quotes
                if (quotes.Any())
                {
                    await context.Quotes.AddRangeAsync(quotes);
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"{quotes.Count} teklif aktarıldı.");
                }

                // Quote Items
                if (quoteItems.Any())
                {
                    await context.QuoteItems.AddRangeAsync(quoteItems);
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"{quoteItems.Count} teklif kalemi aktarıldı.");
                }

                // Contact Messages
                if (contactMessages.Any())
                {
                    await context.ContactMessages.AddRangeAsync(contactMessages);
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"{contactMessages.Count} iletişim mesajı aktarıldı.");
                }

                _logger.LogInformation("Veri geçişi başarıyla tamamlandı!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Veri geçişi sırasında hata oluştu");
                throw;
            }
        }

        private async Task<List<Customer>> ReadCustomersFromSqlite(string sqlitePath)
        {
            var customers = new List<Customer>();
            using var connection = new SqliteConnection($"Data Source={sqlitePath}");
            await connection.OpenAsync();

            var command = new SqliteCommand("SELECT * FROM Customers", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                customers.Add(new Customer
                {
                    Id = reader.GetInt32("Id"),
                    CompanyName = reader.GetString("CompanyName"),
                    ContactPerson = reader.IsDBNull("ContactPerson") ? null : reader.GetString("ContactPerson"),
                    Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                    Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
                    Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                    City = reader.IsDBNull("City") ? null : reader.GetString("City"),
                    Country = reader.IsDBNull("Country") ? null : reader.GetString("Country"),
                    TaxNumber = reader.IsDBNull("TaxNumber") ? null : reader.GetString("TaxNumber"),
                    TaxOffice = reader.IsDBNull("TaxOffice") ? null : reader.GetString("TaxOffice"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    UpdatedAt = reader.GetDateTime("UpdatedAt"),
                    IsActive = reader.GetBoolean("IsActive")
                });
            }

            return customers;
        }

        private async Task<List<Quote>> ReadQuotesFromSqlite(string sqlitePath)
        {
            var quotes = new List<Quote>();
            using var connection = new SqliteConnection($"Data Source={sqlitePath}");
            await connection.OpenAsync();

            var command = new SqliteCommand("SELECT * FROM Quotes", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                quotes.Add(new Quote
                {
                    Id = reader.GetInt32("Id"),
                    QuoteNumber = reader.GetString("QuoteNumber"),
                    CustomerId = reader.GetInt32("CustomerId"),
                    QuoteDate = reader.GetDateTime("QuoteDate"),
                    ValidUntil = reader.GetDateTime("ValidUntil"),
                    SubTotal = reader.GetDecimal("SubTotal"),
                    VatRate = reader.GetDecimal("VatRate"),
                    VatAmount = reader.GetDecimal("VatAmount"),
                    TotalAmount = reader.GetDecimal("TotalAmount"),
                    Currency = reader.IsDBNull("Currency") ? "TRY" : reader.GetString("Currency"),
                    Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    UpdatedAt = reader.GetDateTime("UpdatedAt"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedByUserId = reader.IsDBNull("CreatedByUserId") ? null : reader.GetString("CreatedByUserId"),
                    LastModifiedByUserId = reader.IsDBNull("LastModifiedByUserId") ? null : reader.GetString("LastModifiedByUserId")
                });
            }

            return quotes;
        }

        private async Task<List<QuoteItem>> ReadQuoteItemsFromSqlite(string sqlitePath)
        {
            var quoteItems = new List<QuoteItem>();
            using var connection = new SqliteConnection($"Data Source={sqlitePath}");
            await connection.OpenAsync();

            var command = new SqliteCommand("SELECT * FROM QuoteItems", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                quoteItems.Add(new QuoteItem
                {
                    Id = reader.GetInt32("Id"),
                    QuoteId = reader.GetInt32("QuoteId"),
                    ProductName = reader.GetString("ProductName"),
                    Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                    Quantity = reader.GetDecimal("Quantity"),
                    UnitPrice = reader.GetDecimal("UnitPrice"),
                    DiscountPercentage = reader.GetDecimal("DiscountPercentage"),
                    DiscountAmount = reader.GetDecimal("DiscountAmount"),
                    Total = reader.GetDecimal("Total")
                });
            }

            return quoteItems;
        }

        private async Task<List<ContactMessage>> ReadContactMessagesFromSqlite(string sqlitePath)
        {
            var messages = new List<ContactMessage>();
            using var connection = new SqliteConnection($"Data Source={sqlitePath}");
            await connection.OpenAsync();

            var command = new SqliteCommand("SELECT * FROM ContactMessages", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                messages.Add(new ContactMessage
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    Email = reader.GetString("Email"),
                    Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
                    Subject = reader.GetString("Subject"),
                    Message = reader.GetString("Message"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    IsRead = reader.GetBoolean("IsRead"),
                    IsReplied = reader.GetBoolean("IsReplied")
                });
            }

            return messages;
        }
    }
}
