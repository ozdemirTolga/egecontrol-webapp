using EgeControlWebApp.Data;
using EgeControlWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EgeControlWebApp.Services
{
    public interface IContactService
    {
        Task<List<Contact>> GetAllContactsAsync();
        Task<Contact?> GetContactByIdAsync(int id);
        Task<Contact> CreateContactAsync(Contact contact);
        Task<Contact> UpdateContactAsync(Contact contact);
        Task DeleteContactAsync(int id);
        Task<List<Contact>> GetContactsByCompanyIdAsync(int companyId);
        Task<List<Contact>> GetActiveContactsAsync();
        Task<bool> ContactExistsAsync(int id);
    }

    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _context;

        public ContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Contact>> GetAllContactsAsync()
        {
            return await _context.Contacts
                .Include(c => c.Company)
                .OrderBy(c => c.Company.Name)
                .ThenBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToListAsync();
        }

        public async Task<Contact?> GetContactByIdAsync(int id)
        {
            return await _context.Contacts
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Contact> CreateContactAsync(Contact contact)
        {
            contact.CreatedDate = DateTime.Now;
            contact.IsActive = true;
            
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return contact;
        }

        public async Task<Contact> UpdateContactAsync(Contact contact)
        {
            contact.UpdatedDate = DateTime.Now;
            
            _context.Entry(contact).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return contact;
        }

        public async Task DeleteContactAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                // Soft delete
                contact.IsActive = false;
                contact.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Contact>> GetContactsByCompanyIdAsync(int companyId)
        {
            return await _context.Contacts
                .Include(c => c.Company)
                .Where(c => c.CompanyId == companyId && c.IsActive)
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToListAsync();
        }

        public async Task<List<Contact>> GetActiveContactsAsync()
        {
            return await _context.Contacts
                .Include(c => c.Company)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Company.Name)
                .ThenBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToListAsync();
        }

        public async Task<bool> ContactExistsAsync(int id)
        {
            return await _context.Contacts.AnyAsync(c => c.Id == id && c.IsActive);
        }
    }
}
