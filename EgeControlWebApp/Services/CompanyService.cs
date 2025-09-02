using EgeControlWebApp.Data;
using EgeControlWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EgeControlWebApp.Services
{
    public interface ICompanyService
    {
        Task<List<Company>> GetAllCompaniesAsync();
        Task<Company?> GetCompanyByIdAsync(int id);
        Task<Company> CreateCompanyAsync(Company company);
        Task<Company> UpdateCompanyAsync(Company company);
        Task DeleteCompanyAsync(int id);
        Task<List<Company>> GetActiveCompaniesAsync();
        Task<bool> CompanyExistsAsync(int id);
    }

    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDbContext _context;

        public CompanyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Company>> GetAllCompaniesAsync()
        {
            return await _context.Companies
                .Include(c => c.Contacts.Where(x => x.IsActive))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Company?> GetCompanyByIdAsync(int id)
        {
            return await _context.Companies
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Company> CreateCompanyAsync(Company company)
        {
            company.CreatedDate = DateTime.Now;
            company.IsActive = true;
            
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task<Company> UpdateCompanyAsync(Company company)
        {
            company.UpdatedDate = DateTime.Now;
            
            _context.Entry(company).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task DeleteCompanyAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                // Soft delete
                company.IsActive = false;
                company.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Company>> GetActiveCompaniesAsync()
        {
            return await _context.Companies
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> CompanyExistsAsync(int id)
        {
            return await _context.Companies.AnyAsync(c => c.Id == id && c.IsActive);
        }
    }
}
