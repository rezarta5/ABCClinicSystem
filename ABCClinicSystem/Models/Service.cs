using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ABCClinicSystem.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }  
        public string? Description { get; set; }

        // Foreign key
        public int ServiceDepartmentId { get; set; }

        // Navigation property
        [ValidateNever]
        public ServiceDepartment ServiceDepartment { get; set; }
    }
}
