using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EgeControlWebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace EgeControlWebApp.Areas.Admin.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CreateModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Ad alanı zorunludur.")]
            [Display(Name = "Ad")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Soyad alanı zorunludur.")]
            [Display(Name = "Soyad")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "E-posta adresi zorunludur.")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
            [Display(Name = "E-posta")]
            public string Email { get; set; }

            [Display(Name = "Departman")]
            public string Department { get; set; }

            [Display(Name = "Pozisyon")]
            public string Position { get; set; }

            [Required(ErrorMessage = "Şifre zorunludur.")]
            [StringLength(100, ErrorMessage = "Şifre en az {2} en fazla {1} karakter olmalıdır.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Şifre")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Şifre Tekrar")]
            [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Roller")]
            public List<string> SelectedRoles { get; set; } = new List<string>();
        }

        public Dictionary<string, string> AvailableRoles { get; set; } = new Dictionary<string, string>();

        public async Task OnGetAsync()
        {
            AvailableRoles = EgeControlWebApp.Models.UserRoles.RoleDescriptions;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            AvailableRoles = EgeControlWebApp.Models.UserRoles.RoleDescriptions;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Department = Input.Department,
                Position = Input.Position,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // Seçilen rolleri ata
                if (Input.SelectedRoles != null && Input.SelectedRoles.Any())
                {
                    foreach (var role in Input.SelectedRoles)
                    {
                        if (EgeControlWebApp.Models.UserRoles.AllRoles.Contains(role))
                        {
                            // Role yoksa oluştur
                            if (!await _roleManager.RoleExistsAsync(role))
                            {
                                await _roleManager.CreateAsync(new IdentityRole(role));
                            }
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                }

                StatusMessage = $"{user.FullName} başarıyla oluşturuldu.";
                return RedirectToPage("./Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
