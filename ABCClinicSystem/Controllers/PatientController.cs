using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ABCClinicSystem.Data;
using ABCClinicSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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

        public async Task<IActionResult> Bills()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var bills = await _context.Bills
                .Include(b => b.Doctor)
                .Where(b => b.PatientId == user.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bills);
        }
       public async Task<IActionResult> Dashboard()
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return NotFound();

    // GET ALL APPOINTMENTS
    var allAppointments = await _context.Appointments
        .Include(a => a.Doctor)
        .Where(a => a.PatientId == user.Id)
        .ToListAsync();

    // UPCOMING APPOINTMENTS (use local time consistently)
    var now = DateTime.Now;

    var upcomingAppointments = allAppointments
        .Where(a => a.AppointmentDate >= now)
        .OrderBy(a => a.AppointmentDate)
        .ToList();

    // GROUP BY MONTH (PROPER ORDER + SORT INSIDE GROUP)
    var groupedByMonth = allAppointments
        .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
        .OrderByDescending(g => g.Key.Year)
        .ThenByDescending(g => g.Key.Month)
        .Select(g => new
        {
            Month = g.Key.Month,
            Year = g.Key.Year,
            Appointments = g
                .OrderBy(x => x.AppointmentDate)
                .ToList()
        })
        .ToList();

    ViewBag.AppointmentsByMonth = groupedByMonth;

    // MEDICAL RECORDS
    var medicalRecords = await _context.MedicalRecords
        .Where(m => m.PatientId == user.Id)
        .OrderByDescending(m => m.CreatedAt)
        .ToListAsync();

    // BASIC INFO
    ViewBag.FullName = user.FullName;
    ViewBag.UpcomingAppointments = upcomingAppointments;
    ViewBag.MedicalRecords = medicalRecords;

    // CHART DATA (THIS MONTH ONLY)
    // CHART DATA (THIS MONTH ONLY - FIXED)
    var appointmentsThisMonth = allAppointments
        .Where(a => a.AppointmentDate.Month == DateTime.Now.Month
                    && a.AppointmentDate.Year == DateTime.Now.Year)
        .GroupBy(a => a.AppointmentDate.Day)
        .OrderBy(g => g.Key)
        .Select(g => new
        {
            Day = g.Key,
            Count = g.Count()
        })
        .ToList();

    ViewBag.AppointmentsDays = appointmentsThisMonth.Select(a => a.Day).ToList();
    ViewBag.AppointmentsCount = appointmentsThisMonth.Select(a => a.Count).ToList();

    // REVENUE (PLACEHOLDER)
    var paymentsThisYear = Enumerable.Range(1, 12)
        .Select(m => new { Month = m, Amount = 100 * m })
        .ToList();

    ViewBag.RevenueMonths = paymentsThisYear.Select(p => p.Month).ToList();
    ViewBag.RevenueValues = paymentsThisYear.Select(p => p.Amount).ToList();

    return View();
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
            return View("~/Views/Patient/Appointments/Book.cshtml");
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
                return View(Dashboard); // <-- explicit path
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
                return View("~/Views/Patient/Appointments/Book.cshtml", model); // <-- explicit path
            }

            model.PatientId = user.Id;
            model.Status = "Scheduled";

            _context.Appointments.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment booked successfully!";
            return RedirectToAction(nameof(Appointments));
        }
        
        [HttpPost]
        [Authorize(Roles = "Patient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayBill(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var bill = await _context.Bills
                .FirstOrDefaultAsync(x => x.Id == id && x.PatientId == user.Id);

            if (bill == null)
                return Json(new { success = false, message = "Bill not found" });

            if (bill.Status == "Paid")
                return Json(new { success = false, message = "Already paid" });

            bill.Status = "Paid";
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Payment successful" });
        }
    }
}
