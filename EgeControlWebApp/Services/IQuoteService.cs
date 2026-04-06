using EgeControlWebApp.Models;

namespace EgeControlWebApp.Services
{
    public interface IQuoteService
    {
        Task<IEnumerable<Quote>> GetAllQuotesAsync();
        Task<IEnumerable<Quote>> GetQuotesByUserIdAsync(string userId);
        Task<Quote?> GetQuoteByIdAsync(int id);
        Task<Quote> CreateQuoteAsync(Quote quote);
        Task<Quote> UpdateQuoteAsync(Quote quote, string? userId = null, string? userName = null);
        Task<bool> DeleteQuoteAsync(int id);
        Task<bool> QuoteExistsAsync(int id);
        Task<string> GenerateQuoteNumberAsync();
        Task<IEnumerable<Quote>> GetQuotesByCustomerIdAsync(int customerId);
        Task<IEnumerable<Quote>> SearchQuotesAsync(string searchTerm);
        Task<IEnumerable<Quote>> SearchQuotesByUserIdAsync(string searchTerm, string userId);
    }
}
