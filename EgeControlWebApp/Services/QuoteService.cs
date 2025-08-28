using Microsoft.EntityFrameworkCore;
using EgeControlWebApp.Data;
using EgeControlWebApp.Models;

namespace EgeControlWebApp.Services
{
    public class QuoteService : IQuoteService
    {
        private readonly ApplicationDbContext _context;

        public QuoteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Quote>> GetAllQuotesAsync()
        {
            return await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteItems)
                .Include(q => q.CreatedByUser)
                .Include(q => q.LastModifiedByUser)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<Quote?> GetQuoteByIdAsync(int id)
        {
            return await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteItems.OrderBy(qi => qi.SortOrder))
                .Include(q => q.CreatedByUser)
                .Include(q => q.LastModifiedByUser)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Quote> CreateQuoteAsync(Quote quote)
        {
            quote.CreatedAt = DateTime.Now;
            quote.QuoteNumber = await GenerateQuoteNumberAsync();
            
            // Her kalemin toplamını güvenilir şekilde hesapla
            foreach (var item in quote.QuoteItems)
            {
                var lineSubtotal = item.Quantity * item.UnitPrice;
                item.DiscountAmount = lineSubtotal * (item.DiscountPercentage / 100);
                item.Total = lineSubtotal - item.DiscountAmount;
            }

            // Teklif toplamlarını hesapla
            quote.SubTotal = quote.QuoteItems.Sum(qi => qi.Total);
            quote.VatAmount = quote.SubTotal * (quote.VatRate / 100);
            quote.TotalAmount = quote.SubTotal + quote.VatAmount;
            
            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();
            
            return quote;
        }

        public async Task<Quote> UpdateQuoteAsync(Quote incoming, string? userId = null, string? userName = null)
        {
            // Load existing tracked aggregate
            var existing = await _context.Quotes
                .Include(q => q.QuoteItems)
                .FirstOrDefaultAsync(q => q.Id == incoming.Id);

            if (existing == null)
            {
                throw new KeyNotFoundException($"Quote {incoming.Id} not found");
            }

            // Update scalar fields
            existing.Title = incoming.Title;
            existing.Description = incoming.Description;
            existing.VatRate = incoming.VatRate;
            existing.QuoteDate = incoming.QuoteDate;
            existing.ValidUntil = incoming.ValidUntil;
            existing.Notes = incoming.Notes;
            existing.Status = incoming.Status;
            existing.CustomerId = incoming.CustomerId;
            existing.Currency = incoming.Currency; // Para birimi güncellemesi eklendi
            existing.UpdatedAt = DateTime.Now;
            
            // Update last modified information
            if (!string.IsNullOrEmpty(userId))
            {
                existing.LastModifiedByUserId = userId;
            }
            if (!string.IsNullOrEmpty(userName))
            {
                existing.LastModifiedBy = userName;
            }

            // Prepare clean incoming items with computed totals
            var cleanIncomingItems = (incoming.QuoteItems ?? new List<QuoteItem>()).Select(item =>
            {
                var lineSubtotal = item.Quantity * item.UnitPrice;
                var discountAmount = lineSubtotal * (item.DiscountPercentage / 100);
                var total = lineSubtotal - discountAmount;
                return new QuoteItem
                {
                    Id = item.Id,
                    ItemName = item.ItemName,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Unit = item.Unit,
                    DiscountPercentage = item.DiscountPercentage,
                    DiscountAmount = discountAmount,
                    Total = total,
                    SortOrder = item.SortOrder
                };
            }).ToList();

            // Replace all items to avoid tracking conflicts
            if (existing.QuoteItems.Any())
            {
                _context.QuoteItems.RemoveRange(existing.QuoteItems);
            }

            foreach (var ci in cleanIncomingItems)
            {
                ci.Id = 0; // ensure new insert
                ci.QuoteId = existing.Id;
                _context.QuoteItems.Add(ci);
            }

            // Compute totals from the incoming target set
            existing.SubTotal = cleanIncomingItems.Sum(i => i.Total);
            existing.VatAmount = existing.SubTotal * (existing.VatRate / 100);
            existing.TotalAmount = existing.SubTotal + existing.VatAmount;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteQuoteAsync(int id)
        {
            var quote = await _context.Quotes
                .Include(q => q.QuoteItems)
                .FirstOrDefaultAsync(q => q.Id == id);
                
            if (quote == null)
            {
                return false;
            }

            _context.QuoteItems.RemoveRange(quote.QuoteItems);
            _context.Quotes.Remove(quote);
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> QuoteExistsAsync(int id)
        {
            return await _context.Quotes.AnyAsync(q => q.Id == id);
        }

        public async Task<string> GenerateQuoteNumberAsync()
        {
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;
            
            var lastQuote = await _context.Quotes
                .Where(q => q.CreatedAt.Year == currentYear && q.CreatedAt.Month == currentMonth)
                .OrderByDescending(q => q.CreatedAt)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            
            if (lastQuote != null)
            {
                // Son teklif numarasından sıradaki numarayı al
                var parts = lastQuote.QuoteNumber.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"EGE-{currentYear:yyyy}{currentMonth:00}-{nextNumber:000}";
        }

        public async Task<IEnumerable<Quote>> GetQuotesByCustomerIdAsync(int customerId)
        {
            return await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteItems)
                .Where(q => q.CustomerId == customerId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quote>> SearchQuotesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllQuotesAsync();
            }

            searchTerm = searchTerm.ToLower();

            return await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteItems)
                .Where(q => q.QuoteNumber.ToLower().Contains(searchTerm) ||
                           q.Title.ToLower().Contains(searchTerm) ||
                           (q.Customer != null && q.Customer.CompanyName.ToLower().Contains(searchTerm)))
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }
    }
}
