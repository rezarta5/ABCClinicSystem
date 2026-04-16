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
                User.IsInRole("Admin") ? "Admin" : "Manager";

            ViewBag.UserRole = role;

            // Start with all appointments, include Service and Department
            IQueryable<Appointment> appointments = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .ThenInclude(s => s.ServiceDepartment); // 🔹 include Department
           
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
                Doctors = (User.IsInRole("Admin") || User.IsInRole("Patient"))
                    ? _context.Users
                        .Where(u => u.RoleType == "Doctor")
                        .Select(u => new SelectListItem
                        {
                            Value = u.Id,
                            Text = u.FullName
                        }).ToList()
                    : null, // Doctors themselves won't see it

                Patients = _context.Users
                    .Where(u => u.RoleType == "Patient")
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.FullName,
                        Selected = u.Id == currentUserId
                    }).ToList(),

                AppointmentDate = DateTime.Now
            };
            
            
            // Populate Department dropdown
            appointment.Departments = _context.ServiceDepartments
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                })
                .ToList();

// Populate Service dropdown
            appointment.Services = _context.Services
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToList();
            
            
            // Pre-select default AppointmentType
            foreach (var item in appointment.AppointmentTypes)
            {
                if (item.Value == appointment.AppointmentType)
                    item.Selected = true;
            }
            
            ViewBag.Departments = _context.ServiceDepartments
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
                .ToList();

            ViewBag.Services = _context.Services
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToList();

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
            
            appointment.DepartmentId = model.DepartmentId;
            appointment.ServiceId = model.ServiceId;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment booked successfully!";

            // Redirect to proper page
            return RedirectToAction("Index", "Appointment");
        }
        
        
        
        
        
        // GET: Appointment/GetServicesByDepartment/5
        [HttpGet]
        public async Task<IActionResult> GetServicesByDepartment(int departmentId)
        {
            var services = await _context.Services
                .Where(s => s.ServiceDepartmentId == departmentId)
                .Select(s => new { id = s.Id, name = s.Name })
                .ToListAsync();

            return Json(services);
        }


// GET: Appointment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Department)
                .Include(a => a.Service)
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
            
            // Departments dropdown
            appointment.Departments = _context.ServiceDepartments
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name,
                    Selected = d.Id == appointment.DepartmentId
                })
                .ToList();

// Services dropdown
            appointment.Services = _context.Services
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = s.Id == appointment.ServiceId
                })
                .ToList();

            // Mark current AppointmentType selection
            foreach (var item in appointment.AppointmentTypes)
            {
                item.Selected = item.Value == appointment.AppointmentType;
            }

            return View(appointment);
        }

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Appointment model)
{
    if (id != model.Id) return NotFound();
    
    if (!ModelState.IsValid)
    {
        var errors = string.Join("; ", ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));
        TempData["Errors"] = errors;
    }

    var appointment = await _context.Appointments.FindAsync(id);
    if (appointment == null) return NotFound();

    if (ModelState.IsValid)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (User.IsInRole("Patient") && appointment.PatientId == currentUserId)
        {
            // Patients can only edit Date and Reason
            appointment.AppointmentDate = DateTime.SpecifyKind(model.AppointmentDate, DateTimeKind.Utc);
            appointment.Reason = model.Reason;
        }
        else
        {
            // Admins/Doctors can edit everything
            appointment.DoctorId = model.DoctorId;
            appointment.PatientId = model.PatientId;
            appointment.DepartmentId = model.DepartmentId;
            appointment.ServiceId = model.ServiceId;
            appointment.AppointmentType = model.AppointmentType;
            appointment.Reason = model.Reason;
            appointment.Status = model.Status;
            appointment.AppointmentDate = DateTime.SpecifyKind(model.AppointmentDate, DateTimeKind.Utc);
        }

        try
        {
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

    // 🔹 IMPORTANT: Repopulate navigation properties for the view
    var appointmentEntity = await _context.Appointments
        .Include(a => a.Doctor)
        .Include(a => a.Patient)
        .Include(a => a.Department)
        .Include(a => a.Service)
        .FirstOrDefaultAsync(a => a.Id == model.Id);

    if (appointmentEntity != null)
    {
        model.Doctor = appointmentEntity.Doctor;
        model.Patient = appointmentEntity.Patient;
        model.Department = appointmentEntity.Department;
        model.Service = appointmentEntity.Service;
    }

    // Repopulate dropdowns
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

    model.Departments = _context.ServiceDepartments
        .Select(d => new SelectListItem
        {
            Value = d.Id.ToString(),
            Text = d.Name,
            Selected = d.Id == model.DepartmentId
        }).ToList();

    model.Services = _context.Services
        .Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = s.Name,
            Selected = s.Id == model.ServiceId
        }).ToList();

    foreach (var item in model.AppointmentTypes)
        item.Selected = item.Value == model.AppointmentType;

    return View(model);
}    
        
        
[HttpPost]
public async Task<IActionResult> UpdateAppointmentStatus(int id, string status)
{
    var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    Appointment appointment;

    // ✅ PATIENT: only allowed to CANCEL
    if (User.IsInRole("Patient"))
    {
        if (status != "Cancelled")
        {
            TempData["Error"] = "You can only cancel appointments.";
            return RedirectToAction(nameof(Index));
        }

        appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == currentUserId);

        if (appointment == null) return NotFound();

        appointment.Status = "Cancelled";
    }
    // ✅ DOCTOR / ADMIN: full control
    else if (User.IsInRole("Doctor") || User.IsInRole("Admin"))
    {
        appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null) return NotFound();

        var validStatuses = new List<string> { "Scheduled", "Completed", "Cancelled" };

        if (!validStatuses.Contains(status))
        {
            TempData["Error"] = "Invalid status.";
            return RedirectToAction(nameof(Index));
        }

        appointment.Status = status;
    }
    else
    {
        return Forbid();
    }

    await _context.SaveChangesAsync();

    TempData["Success"] = "Appointment status updated successfully!";
    return RedirectToAction(nameof(Index));
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


