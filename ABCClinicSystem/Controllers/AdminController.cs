using ABCClinicSystem.Data;
using ABCClinicSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABCClinicSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ===================== DASHBOARD =====================
        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalDoctors = (await _userManager.GetUsersInRoleAsync("Doctor")).Count;
            var totalPatients = (await _userManager.GetUsersInRoleAsync("Patient")).Count;
            var totalAppointments = await _context.Appointments.CountAsync();

            var model = new Admin
            {
                TotalUsers = totalUsers,
                TotalDoctors = totalDoctors,
                TotalPatients = totalPatients,
                TotalAppointments = totalAppointments
            };

            return View(model);
        }

        // ===================== MANAGE USERS =====================
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users); // Use your Users.cshtml table view
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return RedirectToAction(nameof(Users));
        }

        // ===================== MANAGE APPOINTMENTS =====================
        public async Task<IActionResult> Appointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .ToListAsync();

            return View(appointments); // Use your ManageAppointments.cshtml
        }

        [HttpGet]
        public async Task<IActionResult> EditAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null) return NotFound();

            // Dropdown lists
            ViewData["Doctors"] = _userManager.Users.Where(u => u.RoleType == "Doctor").ToList();
            ViewData["Patients"] = _userManager.Users.Where(u => u.RoleType == "Patient").ToList();

            return View(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> EditAppointment(Appointment model)
        {
            if (!ModelState.IsValid) return View(model);

            var appointment = await _context.Appointments.FindAsync(model.Id);
            if (appointment == null) return NotFound();

            appointment.DoctorId = model.DoctorId;
            appointment.PatientId = model.PatientId;
            appointment.AppointmentDate = model.AppointmentDate;
            appointment.Reason = model.Reason;
            appointment.AppointmentType = model.AppointmentType;
            appointment.Status = model.Status;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Appointments));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Appointments));
        }

        // ===================== MANAGE MEDICAL RECORDS =====================
        public async Task<IActionResult> MedicalRecords()
        {
            var records = await _context.MedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .ToListAsync();

            return View(records); // Use your ManageMedicalRecords.cshtml
        }

        [HttpGet]
        public async Task<IActionResult> EditMedicalRecord(int id)
        {
            var record = await _context.MedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (record == null) return NotFound();

            ViewData["Doctors"] = _userManager.Users.Where(u => u.RoleType == "Doctor").ToList();
            ViewData["Patients"] = _userManager.Users.Where(u => u.RoleType == "Patient").ToList();

            return View(record);
        }

        [HttpPost]
        public async Task<IActionResult> EditMedicalRecord(MedicalRecord model)
        {
            if (!ModelState.IsValid) return View(model);

            var record = await _context.MedicalRecords.FindAsync(model.Id);
            if (record == null) return NotFound();

            record.DoctorId = model.DoctorId;
            record.PatientId = model.PatientId;
            record.Diagnosis = model.Diagnosis;
            record.Notes = model.Notes;
            record.CreatedAt = model.CreatedAt;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MedicalRecords));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMedicalRecord(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null) return NotFound();

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MedicalRecords));
        }
    }
}