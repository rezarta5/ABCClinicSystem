using Microsoft.AspNetCore.Identity;

namespace ABCClinicSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        public string RoleType { get; set; }  // rename Role → RoleType
    }
}