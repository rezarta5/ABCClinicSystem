using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ABCClinicSystem.Data;
using ABCClinicSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

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

            ViewBag.FullName = doctor.FullName;

            // Appointments
            ViewBag.TotalAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id)
                .CountAsync();

            ViewBag.UpcomingAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate >= DateTime.UtcNow)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            // Medical Records
            ViewBag.RecentMedicalRecords = await _context.MedicalRecords
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == doctor.Id)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Bills
            var bills = await _context.Bills
                .Include(b => b.Patient)
                .Where(b => b.DoctorId == doctor.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            ViewBag.Bills = bills;
            ViewBag.PendingBills = bills.Count(b => b.Status == "Pending");

            return View(); // <-- closing brace fixed here
        }

        // GET: Doctor/Appointments
        public async Task<IActionResult> Appointments()
        {
            var doctor = await _userManager.GetUserAsync(User);
            if (doctor == null) return NotFound();

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.DoctorId == doctor.Id)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // POST: Doctor/UpdateAppointmentStatus
        [HttpPost]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, string status)
        {
            var doctor = await _userManager.GetUserAsync(User); // current logged-in doctor
            if (doctor == null) return Unauthorized();

            // Load the appointment and ensure it belongs to this doctor
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctor.Id);

            if (appointment == null)
                return NotFound(); // either doesn't exist or not the doctor's appointment

            // Only allow certain statuses
            if (status != "Scheduled" && status != "Completed" && status != "Cancelled")
                return BadRequest("Invalid status.");

            appointment.Status = status;
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment status updated successfully!";
            return RedirectToAction("Dashboard");
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


        // POST: Doctor/UpdateBillStatus
        [HttpPost]
        public async Task<IActionResult> UpdateBillStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                TempData["Error"] = "Please select a valid status.";
                return RedirectToAction(nameof(Dashboard));
            }

            var bill = await _context.Bills.FindAsync(id);
            if (bill == null) return NotFound();

            bill.Status = status;
            _context.Update(bill);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bill status updated successfully!";
            return RedirectToAction(nameof(Dashboard));
        }

        // GET: Doctor/CreateBill
        public async Task<IActionResult> CreateBill()
        {
            var doctor = await _userManager.GetUserAsync(User);
            if (doctor == null) return NotFound();

            // Get all users in "Patient" role
            var patients = await _userManager.GetUsersInRoleAsync("Patient");
            ViewBag.Patients = new SelectList(patients, "Id", "FullName");

            // Load all bills for this doctor
            var bills = await _context.Bills
                .Include(b => b.Patient)
                .Where(b => b.DoctorId == doctor.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            ViewBag.Bills = bills;

            return View();
        }

// POST: Doctor/CreateBill
        [HttpPost]
        public async Task<IActionResult> CreateBill(Bill bill)
        {
            var doctor = await _userManager.GetUserAsync(User);
            if (doctor == null) return NotFound();

            bill.DoctorId = doctor.Id;
            bill.Status = "Pending";
            bill.CreatedAt = DateTime.UtcNow;

            // Convert DueDate to UTC if it has a value
            if (bill.DueDate.HasValue)
            {
                bill.DueDate = DateTime.SpecifyKind(bill.DueDate.Value, DateTimeKind.Utc);
            }

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bill created successfully!";
            return RedirectToAction(nameof(Dashboard));
        }
        
        // GET: Doctor/AllBills
        public async Task<IActionResult> AllBills()
        {
            var doctor = await _userManager.GetUserAsync(User);
            if (doctor == null) return NotFound();

            var bills = await _context.Bills
                .Include(b => b.Patient)
                .Where(b => b.DoctorId == doctor.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bills); // Create /Views/Doctor/AllBills.cshtml
        }
        
        
        // POST: Doctor/DeleteBill
        [HttpPost]
        public async Task<IActionResult> DeleteBill(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null) return NotFound();

            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bill deleted successfully!";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}