using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABCClinicSystem.Data;
using Microsoft.AspNetCore.Authorization;

namespace ABCClinicSystem.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GlobalSearch(string query, string type, string status)
        {
            query = query?.Trim()?.ToLower();

            if (string.IsNullOrWhiteSpace(type))
                return Json(new List<object>());

            // ================= ALL SEARCH =================
            if (type == "all")
            {
                var result = new List<object>();

                // DOCTORS
                var doctors = _context.Users
                    .Where(x => x.RoleType == "Doctor");

                if (!string.IsNullOrWhiteSpace(query))
                {
                    doctors = doctors.Where(x =>
                        x.FullName.ToLower().Contains(query) ||
                        x.Email.ToLower().Contains(query));
                }

                result.AddRange(await doctors
                    .Select(x => new
                    {
                        col1 = x.FullName + " (Doctor)",
                        col2 = x.Email,
                        col3 = "",
                        col4 = ""
                    })
                    .ToListAsync());

                // DEPARTMENTS
                var departments = _context.ServiceDepartments
                    .Include(d => d.Services)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    departments = departments.Where(x =>
                        x.Name.ToLower().Contains(query) ||
                        x.Description.ToLower().Contains(query) ||
                        x.Services.Any(s => s.Name.ToLower().Contains(query)));
                }

                result.AddRange(await departments
                    .Select(x => new
                    {
                        col1 = x.Name + " (Department)",
                        col2 = x.Description,
                        col3 = "",
                        col4 = ""
                    })
                    .ToListAsync());

                // SERVICES
                var services = _context.Services.AsQueryable();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    services = services.Where(x =>
                        x.Name.ToLower().Contains(query) ||
                        x.Description.ToLower().Contains(query));
                }

                result.AddRange(await services
                    .Select(x => new
                    {
                        col1 = x.Name + " (Service)",
                        col2 = x.Description,
                        col3 = "",
                        col4 = ""
                    })
                    .ToListAsync());

                return Json(result);
            }

            // ================= DOCTORS =================
            if (type == "doctors")
            {
                var data = _context.Users
                    .Where(x => x.RoleType == "Doctor");

                if (!string.IsNullOrWhiteSpace(query))
                {
                    data = data.Where(x =>
                        x.FullName.ToLower().Contains(query) ||
                        x.Email.ToLower().Contains(query));
                }

                var result = await data.Select(x => new
                {
                    col1 = x.FullName,
                    col2 = x.Email,
                    col3 = x.PhoneNumber,
                    col4 = ""
                }).ToListAsync();

                return Json(result);
            }

            // ================= DEPARTMENTS =================
            if (type == "departments")
            {
                var data = _context.ServiceDepartments
                    .Include(d => d.Services)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    data = data.Where(x =>
                        x.Name.ToLower().Contains(query) ||
                        x.Description.ToLower().Contains(query));
                }

                var result = await data.Select(x => new
                {
                    col1 = x.Name,
                    col2 = x.Description,
                    col3 = "",   // ⚠️ avoid string.Join in EF
                    col4 = ""
                }).ToListAsync();

                return Json(result);
            }

            // ================= SERVICES =================
            if (type == "services")
            {
                var data = _context.Services.AsQueryable();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    data = data.Where(x =>
                        x.Name.ToLower().Contains(query) ||
                        x.Description.ToLower().Contains(query));
                }

                var result = await data.Select(x => new
                {
                    col1 = x.Name,
                    col2 = x.Description,
                    col3 = "",
                    col4 = ""
                }).ToListAsync();

                return Json(result);
            }

            return Json(new List<object>());
        }
    }
}