using ABCClinicSystem.Data;
using ABCClinicSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ABCClinicSystem.Controllers
{
    [Authorize] // Only logged-in users
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Appointment
        public async Task<IActionResult> Index()
        {
            // Detect role
            string role = User.IsInRole("Doctor") ? "Doctor" :
                User.IsInRole("Patient") ? "Patient" :
                User.IsInRole("Admin") ? "Admin" :
                "Manager";

            ViewBag.UserRole = role;

            // Start with all appointments
            IQueryable<Appointment> appointments = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient);

            // Get current user ID
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (role == "Doctor")
                appointments = appointments.Where(a => a.DoctorId == currentUserId);
            else if (role == "Patient")
                appointments = appointments.Where(a => a.PatientId == currentUserId);

            return View(await appointments.ToListAsync());
        }

        // GET: Appointment/Book
        public IActionResult Book()
        {
            // Get logged-in patient ID (optional)
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var appointment = new Appointment
            {
                // Populate Doctor dropdown
                Doctors = _context.Users
                    .Where(u => u.RoleType == "Doctor")
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id, // Must match DoctorId type (string)
                        Text = u.FullName
                    })
                    .ToList(),

                // Populate Patient dropdown
                Patients = _context.Users
                    .Where(u => u.RoleType == "Patient")
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id, // Must match PatientId type (string)
                        Text = u.FullName,
                        Selected = u.Id == currentUserId // Pre-select logged-in patient
                    })
                    .ToList(),

                // AppointmentTypes already have defaults in model constructor
            };

            // Pre-select default AppointmentType
            foreach (var item in appointment.AppointmentTypes)
            {
                if (item.Value == appointment.AppointmentType)
                    item.Selected = true;
            }

            // Pre-fill AppointmentDate with current time
            appointment.AppointmentDate = DateTime.Now;

            return View(appointment);
        }

// POST: Appointment/Book

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(Appointment model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate dropdowns if needed
                return View(model);
            }

            // Get current logged-in user ID
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var appointment = new Appointment
            {
                DoctorId = User.IsInRole("Doctor") ? currentUserId : model.DoctorId,
                PatientId = User.IsInRole("Patient") ? currentUserId : model.PatientId,
                AppointmentDate = DateTime.SpecifyKind(model.AppointmentDate, DateTimeKind.Utc),
                AppointmentType = model.AppointmentType,
                Reason = model.Reason,
                Status = "Scheduled"
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment booked successfully!";

            // Redirect to proper page
            if (User.IsInRole("Doctor"))
                return RedirectToAction("Dashboard", "Doctor");
            else
                return RedirectToAction("Index", "Appointment");
        }


// GET: Appointment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            // Get logged-in user ID
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Populate Doctor dropdown
            appointment.Doctors = _context.Users
                .Where(u => u.RoleType == "Doctor")
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FullName,
                    Selected = u.Id == appointment.DoctorId || u.Id == currentUserId && User.IsInRole("Doctor")
                })
                .ToList();

            // Populate Patient dropdown
            appointment.Patients = _context.Users
                .Where(u => u.RoleType == "Patient")
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FullName,
                    Selected = u.Id == appointment.PatientId || u.Id == currentUserId && User.IsInRole("Patient")
                })
                .ToList();

            // Mark current AppointmentType selection
            foreach (var item in appointment.AppointmentTypes)
            {
                item.Selected = item.Value == appointment.AppointmentType;
            }

            return View(appointment);
        }

// POST: Appointment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the existing appointment
                    var appointment = await _context.Appointments.FindAsync(id);
                    if (appointment == null) return NotFound();

                    // Update fields
                    appointment.DoctorId = model.DoctorId;
                    appointment.PatientId = model.PatientId;
                    appointment.AppointmentType = model.AppointmentType;
                    appointment.Reason = model.Reason;
                    appointment.Status = model.Status;

                    // Convert AppointmentDate to UTC
                    appointment.AppointmentDate = DateTime.SpecifyKind(model.AppointmentDate, DateTimeKind.Utc);

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Appointment updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Appointments.Any(a => a.Id == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Repopulate dropdowns if validation fails
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            model.Doctors = _context.Users
                .Where(u => u.RoleType == "Doctor")
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FullName,
                    Selected = u.Id == model.DoctorId
                }).ToList();

            model.Patients = _context.Users
                .Where(u => u.RoleType == "Patient")
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FullName,
                    Selected = u.Id == model.PatientId
                }).ToList();

            foreach (var item in model.AppointmentTypes)
                item.Selected = item.Value == model.AppointmentType;

            return View(model);
        }



// GET: Appointment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

// POST: Appointment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                // Set success message
                TempData["Success"] = "Appointment deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}


