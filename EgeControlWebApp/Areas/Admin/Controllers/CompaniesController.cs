using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EgeControlWebApp.Models;
using EgeControlWebApp.Services;

namespace EgeControlWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminPolicy")]
    public class CompaniesController : Controller
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        // GET: Admin/Companies
        public async Task<IActionResult> Index()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return View(companies);
        }

        // GET: Admin/Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _companyService.GetCompanyByIdAsync(id.Value);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Admin/Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Companies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,TaxNumber,TaxOffice,Website,Notes")] Company company)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _companyService.CreateCompanyAsync(company);
                    TempData["SuccessMessage"] = "Şirket başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Şirket oluşturulurken bir hata oluştu: " + ex.Message;
                }
            }
            return View(company);
        }

        // GET: Admin/Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _companyService.GetCompanyByIdAsync(id.Value);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Admin/Companies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,TaxNumber,TaxOffice,Website,Notes,IsActive,CreatedDate")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _companyService.UpdateCompanyAsync(company);
                    TempData["SuccessMessage"] = "Şirket başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Şirket güncellenirken bir hata oluştu: " + ex.Message;
                }
            }
            return View(company);
        }

        // GET: Admin/Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _companyService.GetCompanyByIdAsync(id.Value);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Admin/Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _companyService.DeleteCompanyAsync(id);
                TempData["SuccessMessage"] = "Şirket başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Şirket silinirken bir hata oluştu: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
