using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgeControlWebApp.Models;
using EgeControlWebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace EgeControlWebApp.Areas.Admin.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public Dictionary<string, IList<string>> UserRoles { get; set; } = new Dictionary<string, IList<string>>();

        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            Users = await _userManager.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            foreach (var user in Users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                UserRoles[user.Id] = roles.ToList();
            }
        }

        public async Task<IActionResult> OnPostActivateAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = "Kullanıcı bulunamadı.";
                return RedirectToPage();
            }

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            StatusMessage = $"{user.FullName} aktifleştirildi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeactivateAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = "Kullanıcı bulunamadı.";
                return RedirectToPage();
            }

            // Admin kullanıcısını deaktive edemez
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                StatusMessage = "Admin kullanıcısı deaktive edilemez.";
                return RedirectToPage();
            }

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            StatusMessage = $"{user.FullName} deaktivleştirildi.";
            return RedirectToPage();
        }
    }
}
