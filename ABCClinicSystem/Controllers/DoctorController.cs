using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ABCClinicSystem.Data;
using ABCClinicSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ABCClinicSystem.Controllers
{
    public class DoctorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DoctorController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Doctor/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var doctor = await _userManager.GetUserAsync(User);
            if (doctor == null) return NotFound();

            // Set FullName for view
            ViewBag.FullName = doctor.FullName;

            // Total appointments
            var totalAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id)
                .CountAsync();

            // Upcoming appointments
            var upcomingAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate >= DateTime.UtcNow)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.UpcomingAppointments = upcomingAppointments;

            return View();
        }

        // GET: Doctor/Appointments
        public async Task<IActionResult> Appointments()
        {
            var doctor = await _userManager.GetUserAsync(User);
            if (doctor == null) return NotFound();

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)       // Include doctor info if needed
                .Where(a => a.DoctorId == doctor.Id)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // POST: Doctor/UpdateAppointmentStatus
        [HttpPost]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, string status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.Status = status;
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Appointments));
        }

        // GET: Doctor/MedicalRecords
        public async Task<IActionResult> MedicalRecords()
        {
            var doctor = await _userManager.GetUserAsync(User);
            if (doctor == null) return NotFound();

            var records = await _context.MedicalRecords
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == doctor.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(records);
        }

        // GET: Doctor/Profile
        public async Task<IActionResult> Profile()
        {
            var doctor = await _userManager.GetUserAsync(User);
            if (doctor == null) return NotFound();
            return View(doctor);
        }

        // GET: Doctor/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var doctor = await _userManager.GetUserAsync(User);
            if (doctor == null) return NotFound();
            return View(doctor);
        }

        // POST: Doctor/EditProfile
        [HttpPost]
        public async Task<IActionResult> EditProfile(ApplicationUser model)
        {
            var doctor = await _userManager.GetUserAsync(User);
            if (doctor == null) return NotFound();
            if (!ModelState.IsValid) return View(model);

            doctor.FullName = model.FullName;
            doctor.Email = model.Email;
            doctor.UserName = model.UserName;

            var result = await _userManager.UpdateAsync(doctor);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
    }
}