using Microsoft.EntityFrameworkCore;
using EgeControlWebApp.Data;
using EgeControlWebApp.Models;

namespace EgeControlWebApp.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Quotes)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            customer.CreatedAt = DateTime.UtcNow;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.IsActive = true;
            
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            
            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            customer.UpdatedAt = DateTime.UtcNow;
            
            _context.Entry(customer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return customer;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return false;
            }

            // Soft delete - sadece IsActive'i false yapıyoruz
            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CustomerExistsAsync(int id)
        {
            return await _context.Customers.AnyAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllCustomersAsync();
            }

            searchTerm = searchTerm.ToLower();

            return await _context.Customers
                .Where(c => c.IsActive &&
                    (c.CompanyName.ToLower().Contains(searchTerm) ||
                     c.ContactPerson.ToLower().Contains(searchTerm) ||
                     c.Email.ToLower().Contains(searchTerm) ||
                     c.Phone.Contains(searchTerm)))
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quote>> GetCustomerQuotesAsync(int customerId)
        {
            return await _context.Quotes
                .Where(q => q.CustomerId == customerId)
                .Include(q => q.QuoteItems)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }
    }
}
