using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using EgeControlWebApp.Models;
using EgeControlWebApp.Services;

namespace EgeControlWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class ContactsController : Controller
    {
        private readonly IContactService _contactService;
        private readonly ICompanyService _companyService;

        public ContactsController(IContactService contactService, ICompanyService companyService)
        {
            _contactService = contactService;
            _companyService = companyService;
        }

        // GET: Admin/Contacts
        public async Task<IActionResult> Index()
        {
            var contacts = await _contactService.GetAllContactsAsync();
            return View(contacts);
        }

        // GET: Admin/Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _contactService.GetContactByIdAsync(id.Value);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Admin/Contacts/Create
        public async Task<IActionResult> Create()
        {
            await PopulateCompaniesDropDown();
            return View();
        }

        // POST: Admin/Contacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CompanyId,FirstName,LastName,Title,Email,Phone,Extension,MobilePhone,Department,Notes")] Contact contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _contactService.CreateContactAsync(contact);
                    TempData["SuccessMessage"] = "İletişim kişisi başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "İletişim kişisi oluşturulurken bir hata oluştu: " + ex.Message;
                }
            }
            await PopulateCompaniesDropDown(contact.CompanyId);
            return View(contact);
        }

        // GET: Admin/Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _contactService.GetContactByIdAsync(id.Value);
            if (contact == null)
            {
                return NotFound();
            }
            await PopulateCompaniesDropDown(contact.CompanyId);
            return View(contact);
        }

        // POST: Admin/Contacts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,FirstName,LastName,Title,Email,Phone,Extension,MobilePhone,Department,Notes,IsActive,CreatedDate")] Contact contact)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _contactService.UpdateContactAsync(contact);
                    TempData["SuccessMessage"] = "İletişim kişisi başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "İletişim kişisi güncellenirken bir hata oluştu: " + ex.Message;
                }
            }
            await PopulateCompaniesDropDown(contact.CompanyId);
            return View(contact);
        }

        // GET: Admin/Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _contactService.GetContactByIdAsync(id.Value);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Admin/Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _contactService.DeleteContactAsync(id);
                TempData["SuccessMessage"] = "İletişim kişisi başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "İletişim kişisi silinirken bir hata oluştu: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Get contacts by company ID
        [HttpGet]
        public async Task<IActionResult> GetContactsByCompany(int companyId)
        {
            var contacts = await _contactService.GetContactsByCompanyIdAsync(companyId);
            return Json(contacts.Select(c => new 
            { 
                id = c.Id, 
                fullName = c.FullName,
                fullNameWithTitle = c.FullNameWithTitle,
                email = c.Email,
                phone = c.Phone
            }));
        }

        private async Task PopulateCompaniesDropDown(int? selectedCompanyId = null)
        {
            var companies = await _companyService.GetActiveCompaniesAsync();
            ViewData["CompanyId"] = new SelectList(companies, "Id", "Name", selectedCompanyId);
        }
    }
}
