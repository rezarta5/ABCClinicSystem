using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering; 
using ABCClinicSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ABCClinicSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ApplicationUserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationUserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // ---------------- List all users ----------------
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // ---------------- Create User (GET) ----------------
        public IActionResult Create()
        {
            ViewBag.Roles = new[] { "Admin", "Doctor", "Patient" };
            return View();
        }

        // ---------------- Create User (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser user, string password)
        {
            ViewBag.Roles = new[] { "Admin", "Doctor", "Patient" }; // ✅ populate roles here too

            if (!ModelState.IsValid)
                return View(user);

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(user); // now ViewBag.Roles is populated
            }

            if (!string.IsNullOrEmpty(user.RoleType))
                await _userManager.AddToRoleAsync(user, user.RoleType);

            TempData["Success"] = $"User {user.FullName} created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ---------------- Edit User (GET) ----------------
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // ✅ Convert roles to SelectList for asp-items
            var roles = new[] { "Admin", "Doctor", "Patient" };
            ViewBag.Roles = new SelectList(roles, user.RoleType); // selected value = user's current role

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser user)
        {
            var existingUser = await _userManager.FindByIdAsync(user.Id);
            if (existingUser == null) return NotFound();

            // Set UserName (required by Identity)
            existingUser.UserName = user.Email; // ✅ must not be empty, can only contain letters/digits

            // Update other fields
            existingUser.Email = user.Email;
            existingUser.FullName = user.FullName;

            // Remove old roles
            var currentRoles = await _userManager.GetRolesAsync(existingUser);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);

            // Assign new role
            if (!string.IsNullOrEmpty(user.RoleType))
            {
                existingUser.RoleType = user.RoleType;
                await _userManager.AddToRoleAsync(existingUser, user.RoleType);
            }

            var result = await _userManager.UpdateAsync(existingUser);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                var roles = new[] { "Admin", "Doctor", "Patient" };
                ViewBag.Roles = new SelectList(roles, user.RoleType);

                return View(user);
            }

            TempData["Success"] = $"User {user.FullName} updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        
        
        // ---------------- Delete User ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            TempData["Success"] = result.Succeeded 
                ? $"User {user.FullName} deleted successfully!" 
                : "Failed to delete user.";

            return RedirectToAction(nameof(Index));
        }
    }
}