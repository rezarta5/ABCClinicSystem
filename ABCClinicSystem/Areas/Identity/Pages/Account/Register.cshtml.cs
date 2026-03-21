using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ABCClinicSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ABCClinicSystem.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = null!; // ✅ Use C# 11 required handling

        public class InputModel
        {
            [Required] public required string FullName { get; set; }
            [Required] [EmailAddress] public required string Email { get; set; }
            [Required] public required string RoleType { get; set; } // Admin, Doctor, Manager, Patient
            [Required] [DataType(DataType.Password)] public required string Password { get; set; }
            [Required] [DataType(DataType.Password)] [Compare("Password")] public required string ConfirmPassword { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(Input.RoleType))
                await _roleManager.CreateAsync(new IdentityRole(Input.RoleType));

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FullName = Input.FullName,
                RoleType = Input.RoleType,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Input.RoleType);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect("~/");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }
    }
}