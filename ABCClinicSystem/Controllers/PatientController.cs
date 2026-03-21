using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ABCClinicSystem.Data;
using ABCClinicSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ABCClinicSystem.Controllers
{
    public class PatientController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public PatientController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Patient/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View(user);
        }

        // GET: Patient/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Patient/EditProfile
        [HttpPost]
        public async Task<IActionResult> EditProfile(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid) return View(model);

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.UserName;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // GET: Patient/Appointments
        public async Task<IActionResult> Appointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == user.Id)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // GET: Patient/BookAppointment
        public async Task<IActionResult> BookAppointment()
        {
            var doctors = await _userManager.Users
                .Where(u => u.RoleType == "Doctor")
                .ToListAsync();

            ViewBag.Doctors = doctors;
            return View();
        }

        // POST: Patient/BookAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(Appointment model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                var doctors = await _userManager.Users
                    .Where(u => u.RoleType == "Doctor")
                    .ToListAsync();
                ViewBag.Doctors = doctors;
                return View(model);
            }

            // Prevent double booking
            var exists = await _context.Appointments
                .AnyAsync(a => a.DoctorId == model.DoctorId && a.AppointmentDate == model.AppointmentDate);
            if (exists)
            {
                ModelState.AddModelError("", "This appointment slot is already booked.");
                var doctors = await _userManager.Users
                    .Where(u => u.RoleType == "Doctor")
                    .ToListAsync();
                ViewBag.Doctors = doctors;
                return View(model);
            }

            model.PatientId = user.Id;
            model.Status = "Scheduled";

            _context.Appointments.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment booked successfully!";
            return RedirectToAction(nameof(Appointments));
        }
    }
}