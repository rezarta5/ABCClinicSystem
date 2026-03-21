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
    [Authorize(Roles = "Doctor,Admin")]
    public class MedicalRecordController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MedicalRecordController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /MedicalRecord
        public async Task<IActionResult> Index()
        {
            var records = await _context.MedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(records);
        }

        // ---------------- Create ----------------

        // GET: /MedicalRecord/Create
        public async Task<IActionResult> Create()
        {
            var model = new MedicalRecord
            {
                CreatedAt = DateTime.UtcNow
            };

            await PopulateDropdownsAsync();
            return View(model);
        }

// POST: /MedicalRecord/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalRecord record)
        {
            // Assign DoctorId if logged in user is Doctor
            if (!User.IsInRole("Admin"))
            {
                var doctorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(doctorId))
                {
                    ModelState.AddModelError("", "Unable to identify logged-in doctor.");
                }
                else
                {
                    record.DoctorId = doctorId;
                    ModelState.Remove("DoctorId"); // remove error since we assigned it
                }
            }

            // Ensure CreatedAt is set
            if (record.CreatedAt == default)
                record.CreatedAt = DateTime.UtcNow;

            // Make sure PatientId is not null
            if (string.IsNullOrEmpty(record.PatientId))
                ModelState.AddModelError("PatientId", "Please select a patient.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(record);
            }

            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Medical record created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ---------------- Edit ----------------

        // GET: /MedicalRecord/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null) return NotFound();

            await PopulateDropdownsAsync();
            return View(record);
        }

        // POST: /MedicalRecord/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicalRecord record)
        {
            if (id != record.Id) return BadRequest();

            // Assign DoctorId if user is not Admin
            if (!User.IsInRole("Admin"))
            {
                var doctorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(doctorId))
                    ModelState.AddModelError("", "Unable to identify logged-in doctor.");
                else
                    record.DoctorId = doctorId;
            }

            // Ensure CreatedAt is UTC
            record.CreatedAt = record.CreatedAt.ToUniversalTime();

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(record);
            }

            _context.MedicalRecords.Update(record);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Medical record updated successfully!";
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Error updating database: " + ex.Message);
                await PopulateDropdownsAsync();
                return View(record);
            }

            return RedirectToAction(nameof(Index));
        }

        // ---------------- Delete ----------------

        // GET: /MedicalRecord/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.MedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (record == null) return NotFound();

            return View(record);
        }

        // POST: /MedicalRecord/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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

        // Populate dropdowns for Patients and Doctors
        private async Task PopulateDropdownsAsync()
        {
            ViewBag.Patients = new SelectList(await _context.Users
                .Where(u => u.RoleType == "Patient")
                .ToListAsync(), "Id", "FullName");

            if (User.IsInRole("Admin"))
            {
                ViewBag.Doctors = new SelectList(await _context.Users
                    .Where(u => u.RoleType == "Doctor")
                    .ToListAsync(), "Id", "FullName");
            }
        }
    }
}