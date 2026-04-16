using Microsoft.AspNetCore.Mvc;
using ABCClinicSystem.Data;
using ABCClinicSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Identity;

namespace ABCClinicSystem.Controllers
{
    [Authorize]
    public class MedicalRecordController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MedicalRecordController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ---------------- Index ----------------
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            IQueryable<MedicalRecord> records = _context.MedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .OrderByDescending(r => r.CreatedAt);

            if (User.IsInRole("Doctor"))
            {
                // Doctor sees only their own records
                records = records.Where(r => r.DoctorId == currentUserId);
            }
            else if (User.IsInRole("Patient"))
            {
                // Patient sees only their own records
                records = records.Where(r => r.PatientId == currentUserId);
            }
            // Admin sees all records

            return View(await records.ToListAsync());
        }
        
        

        // ---------------- Create (GET) ----------------
        [Authorize(Roles = "Doctor,Admin")]
        
        public async Task<IActionResult> Create()
        {
            var model = new MedicalRecord { CreatedAt = DateTime.UtcNow };

            // Populate patients for all users
            var patients = await _userManager.GetUsersInRoleAsync("Patient");
            ViewBag.Patients = new SelectList(patients, "Id", "FullName");

            // Populate doctors only for Admin
            if (User.IsInRole("Admin"))
            {
                var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
                ViewBag.Doctors = new SelectList(doctors, "Id", "FullName");
            }

            return View(model);
            
            
        }

        // ---------------- Create (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Create(MedicalRecord record)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            // Assign Doctor only if the user is a Doctor
            if (User.IsInRole("Doctor"))
            {
                record.DoctorId = currentUser.Id;
            }

            // Remove navigation property validation
            ModelState.Remove("DoctorId");
            ModelState.Remove("Doctor");
            ModelState.Remove("Patient");

            // Validate Patient manually
            if (string.IsNullOrEmpty(record.PatientId))
            {
                ModelState.AddModelError("PatientId", "Patient is required.");
            }

            // Ensure UTC
            record.CreatedAt = DateTime.SpecifyKind(record.CreatedAt, DateTimeKind.Utc);

            // Repopulate dropdowns if validation fails
            if (!ModelState.IsValid)
            {
                var patients = await _userManager.GetUsersInRoleAsync("Patient");
                ViewBag.Patients = new SelectList(patients, "Id", "FullName", record.PatientId);

                if (User.IsInRole("Admin"))
                {
                    var doctors = await _userManager.GetUsersInRoleAsync("Doctor");
                    ViewBag.Doctors = new SelectList(doctors, "Id", "FullName", record.DoctorId);
                }

                return View(record);
            }

            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Medical record created successfully!";
            return RedirectToAction(nameof(Index));
        }
        
        
        

        // ---------------- Edit (GET) ----------------
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null) return NotFound();

            await PopulateDropdownsAsync();
            return View(record);
        }

        // ---------------- Edit (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Edit(int id, MedicalRecord record)
        {
            if (id != record.Id) return BadRequest();

            // ✅ Remove ALL problematic validation
            ModelState.Remove(nameof(MedicalRecord.DoctorId));
            ModelState.Remove("Doctor");   // 🔥 ADD THIS
            ModelState.Remove("Patient");  // 🔥 ADD THIS

            // ✅ Auto-assign doctor
            if (User.IsInRole("Doctor"))
            {
                var doctorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(doctorId))
                {
                    record.DoctorId = doctorId;
                }
            }

            // ✅ Validate Patient manually
            if (string.IsNullOrEmpty(record.PatientId))
            {
                ModelState.AddModelError("PatientId", "Patient is required.");
            }

            // ✅ Ensure UTC
            record.CreatedAt = DateTime.SpecifyKind(record.CreatedAt, DateTimeKind.Utc);

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(record);
            }

            _context.MedicalRecords.Update(record);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Medical record updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ---------------- Delete (GET) ----------------
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.MedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (record == null) return NotFound();

            return View(record);
        }

        // ---------------- Delete (POST) ----------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null) return NotFound();

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Medical record deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ---------------- Helper ----------------
        private async Task PopulateDropdownsAsync()
        {
            ViewBag.Patients = new SelectList(await _context.Users
                .Where(u => u.RoleType == "Patient")
                .ToListAsync(), "Id", "FullName");

            // Only Admin (not Doctor) sees doctor dropdown
            if (User.IsInRole("Admin") && !User.IsInRole("Doctor"))
            {
                ViewBag.Doctors = new SelectList(await _context.Users
                    .Where(u => u.RoleType == "Doctor")
                    .ToListAsync(), "Id", "FullName");
            }
        }
    }
}