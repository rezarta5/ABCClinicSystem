using ABCClinicSystem.Data;
using ABCClinicSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ABCClinicSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
// ===================== DASHBOARD =====================

        public async Task<IActionResult> Dashboard()
        {
            // Get currently logged-in admin user
            var currentUser = await _userManager.GetUserAsync(User);

            // Pass full name to ViewBag
            ViewBag.FullName = currentUser != null ? currentUser.FullName : "Admin";
            var totalUsers = await _userManager.Users.CountAsync();
            var totalDoctors = (await _userManager.GetUsersInRoleAsync("Doctor")).Count;
            var totalPatients = (await _userManager.GetUsersInRoleAsync("Patient")).Count;
            var totalAppointments = await _context.Appointments.CountAsync();

            // ✅ Calculate bills info just once
            var totalBills = await _context.Bills.CountAsync();
            var totalRevenue = await _context.Bills
                .Where(b => b.Status == "Paid") // Only include Paid bills
                .SumAsync(b => b.Amount);

            // Load all appointments with Patient and Doctor included
            var allAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            // Load all bills
            var bills = await _context.Bills
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // Prepare model including total bills and revenue
            var model = new Admin
            {
                TotalUsers = totalUsers,
                TotalDoctors = totalDoctors,
                TotalPatients = totalPatients,
                TotalAppointments = totalAppointments,
                TotalBills = totalBills,
                TotalRevenue = totalRevenue
            };

            // Pass data to ViewBag for the view
            ViewBag.AllAppointments = allAppointments;
            ViewBag.Bills = bills;
            
            
            // Get Departments
            var departments = await _context.ServiceDepartments.ToListAsync();
            ViewBag.Departments = departments.Select(d => d.Name).ToList();

// Count appointments per department
            var appointmentsByDept = new List<int>();
            foreach (var dept in departments)
            {
                var count = await _context.Appointments
                    .Include(a => a.Service)
                    .CountAsync(a => a.Service.ServiceDepartmentId == dept.Id);
                appointmentsByDept.Add(count);
            }
            ViewBag.AppointmentsByDept = appointmentsByDept;

// Revenue per month (for chart)
            var months = Enumerable.Range(1, 12).Select(m => new DateTime(DateTime.Now.Year, m, 1).ToString("MMM")).ToList();
            ViewBag.Months = months;

            var revenuePerMonth = new List<decimal>();
            foreach (var m in Enumerable.Range(1, 12))
            {
                var revenue = await _context.Bills
                    .Where(b => b.Status == "Paid" && b.CreatedAt.Month == m && b.CreatedAt.Year == DateTime.Now.Year)
                    .SumAsync(b => b.Amount);
                revenuePerMonth.Add(revenue);
            }
            ViewBag.Revenue = revenuePerMonth;

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
        public async Task<IActionResult> UpdateAppointmentStatus(int id, string status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            // Only allow valid statuses
            var validStatuses = new List<string> { "Scheduled", "Completed", "Cancelled" };
            if (!validStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction(nameof(Dashboard));
            }

            appointment.Status = status;
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment status updated successfully!";
            return RedirectToAction(nameof(Dashboard));
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
        
        
        


        // View all bills
        // GET: Admin/Bills
        public async Task<IActionResult> Bills()
        {
            var bills = await _context.Bills
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bills); // /Views/Admin/Bills.cshtml
        }
        
        
        
        
        // GET: Admin/CreateBill
        [HttpGet]
        public async Task<IActionResult> CreateBill()
        {
            var patients = await _userManager.GetUsersInRoleAsync("Patient");
            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");

            ViewBag.Patients = new SelectList(patients, "Id", "FullName");
            ViewBag.Doctors = new SelectList(doctors, "Id", "FullName");

            ViewBag.StatusList = new SelectList(
                new List<string> { "Pending", "Paid", "Cancelled" },
                "Pending"
            );

            return View(new Bill());
        }

// POST: Admin/CreateBill
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBill(
            string PatientId,
            string DoctorId,
            decimal Amount,
            string Status,
            DateTime? DueDate,
            string Description)
        {
            // Validation
            if (string.IsNullOrEmpty(PatientId) || string.IsNullOrEmpty(DoctorId))
            {
                TempData["Error"] = "Please select both a Patient and a Doctor.";

                // Reload dropdowns with selected values
                var patients = await _userManager.GetUsersInRoleAsync("Patient");
                var doctors = await _userManager.GetUsersInRoleAsync("Doctor");

                ViewBag.Patients = new SelectList(patients, "Id", "FullName", PatientId);
                ViewBag.Doctors = new SelectList(doctors, "Id", "FullName", DoctorId);
                ViewBag.StatusList = new SelectList(new List<string> { "Pending", "Paid", "Cancelled" }, Status);

                return View();
            }

            // Create bill with UTC DueDate
            var bill = new Bill
            {
                PatientId = PatientId,
                DoctorId = DoctorId,
                Amount = Amount,
                Status = Status,
                DueDate = DueDate.HasValue ? DateTime.SpecifyKind(DueDate.Value, DateTimeKind.Utc) : (DateTime?)null,
                Description = Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bill added successfully!";
            return RedirectToAction(nameof(Bills));
        }


        // GET: Admin/EditBill/5
        [HttpGet]
        public async Task<IActionResult> EditBill(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null) return NotFound();

            // Load dropdowns
            var patients = await _userManager.GetUsersInRoleAsync("Patient");
            var doctors = await _userManager.GetUsersInRoleAsync("Doctor");

            ViewBag.Patients = new SelectList(patients, "Id", "FullName", bill.PatientId);
            ViewBag.Doctors = new SelectList(doctors, "Id", "FullName", bill.DoctorId);

            // Status options
            var statuses = new List<string> { "Pending", "Paid", "Cancelled" };
            ViewBag.StatusList = new SelectList(statuses, bill.Status);

            return View(bill);
        }

// POST: Admin/EditBill
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBill(
            int id,
            string patientId,
            string doctorId,
            decimal amount,
            string status,
            string description)
        {
            if (string.IsNullOrEmpty(status))
            {
                TempData["Error"] = "Please select a valid status.";
                return RedirectToAction(nameof(EditBill), new { id });
            }

            var bill = await _context.Bills.FindAsync(id);
            if (bill == null) return NotFound();

            // Update properties
            bill.PatientId = patientId;
            bill.DoctorId = doctorId;
            bill.Amount = amount;
            bill.Status = status;
            bill.Description = description;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Bill updated successfully!";
            return RedirectToAction(nameof(Bills));
        }


// POST: Admin/DeleteBill/5
        [HttpPost]
        public async Task<IActionResult> DeleteBill(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null) return NotFound();

            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bill deleted successfully!";
            return RedirectToAction(nameof(Bills));
        }
        
        
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBillStatus(int id, string status)
        {
            var bill = await _context.Bills.FindAsync(id);

            if (bill == null)
            {
                TempData["Error"] = "Bill not found.";
                return RedirectToAction(nameof(Bills));
            }

            var validStatuses = new List<string> { "Pending", "Paid", "Cancelled" };

            if (!validStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction(nameof(Bills));
            }

            bill.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bill status updated successfully!";
            return RedirectToAction(nameof(Bills));
        }
        
        

        public async Task<IActionResult> SeedDepartmentsAndServices()
        {
            if (!_context.ServiceDepartments.Any())
            {
                var cardiology = new ServiceDepartment { Name = "Cardiology" };
                var pediatrics = new ServiceDepartment { Name = "Pediatrics" };
                var dermatology = new ServiceDepartment { Name = "Dermatology" };

                _context.ServiceDepartments.AddRange(cardiology, pediatrics, dermatology);

                _context.Services.AddRange(
                    new Service { Name = "ECG Test", ServiceDepartment = cardiology },
                    new Service { Name = "Heart Checkup", ServiceDepartment = cardiology },
                    new Service { Name = "Vaccination", ServiceDepartment = pediatrics },
                    new Service { Name = "Growth Check", ServiceDepartment = pediatrics },
                    new Service { Name = "Routine Check-up", ServiceDepartment = pediatrics },
                    new Service { Name = "Skin Checkup", ServiceDepartment = dermatology },
                    new Service { Name = "Allergy Test", ServiceDepartment = dermatology }
                );

                await _context.SaveChangesAsync();
            }

            return Content("Departments and Services seeded successfully!");
        }
        
        
        

        // ===================== DEPARTMENTS & SERVICES MANAGEMENT =====================
        public async Task<IActionResult> Departments()
        {
            var departments = await _context.ServiceDepartments
                .Include(d => d.Services)
                .ToListAsync();
            return View(departments);
        }

// Add or Edit Department
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDepartment(ServiceDepartment model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Departments));

            if (model.Id == 0)
                _context.ServiceDepartments.Add(model);
            else
                _context.ServiceDepartments.Update(model);

            await _context.SaveChangesAsync();
            TempData["Success"] = model.Id == 0 ? "Department added!" : "Department updated!";
            return RedirectToAction(nameof(Departments));
        }

// Delete Department
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var dept = await _context.ServiceDepartments
                .Include(d => d.Services)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dept != null)
            {
                _context.Services.RemoveRange(dept.Services);
                _context.ServiceDepartments.Remove(dept);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Department and its services deleted!";
            }

            return RedirectToAction(nameof(Departments));
        }

// Add or Edit Service
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveService(Service model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Departments));

            if (model.Id == 0)
                _context.Services.Add(model);
            else
                _context.Services.Update(model);

            await _context.SaveChangesAsync();
            TempData["Success"] = model.Id == 0 ? "Service added!" : "Service updated!";
            return RedirectToAction(nameof(Departments));
        }

// Delete Service
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Service deleted!";
            }

            return RedirectToAction(nameof(Departments));
        }
    }
}
