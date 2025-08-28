using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgeControlWebApp.Models;

namespace EgeControlWebApp.Areas.Admin.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class RolesModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public ApplicationUser? AppUser { get; set; }
        public IList<string> UserRolesList { get; set; } = new List<string>();
        public Dictionary<string, string> AvailableRoles { get; set; } = new Dictionary<string, string>();

        [BindProperty]
        public List<string> SelectedRoles { get; set; } = new List<string>();

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            AppUser = await _userManager.FindByIdAsync(id);
            if (AppUser == null)
            {
                return NotFound();
            }

            UserRolesList = await _userManager.GetRolesAsync(AppUser);
            AvailableRoles = EgeControlWebApp.Models.UserRoles.RoleDescriptions;
            SelectedRoles = UserRolesList.ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            AppUser = await _userManager.FindByIdAsync(id);
            if (AppUser == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(AppUser);
            
            // Kaldırılacak roller
            var rolesToRemove = currentRoles.Except(SelectedRoles).ToList();
            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(AppUser, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return await OnGetAsync(id);
                }
            }

            // Eklenecek roller
            var rolesToAdd = SelectedRoles.Except(currentRoles).ToList();
            if (rolesToAdd.Any())
            {
                foreach (var role in rolesToAdd)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                var addResult = await _userManager.AddToRolesAsync(AppUser, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return await OnGetAsync(id);
                }
            }

            StatusMessage = $"{AppUser.FullName} kullanıcısının rolleri güncellendi.";
            return RedirectToPage("./Index");
        }
    }
}
