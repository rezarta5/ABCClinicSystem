using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering; // <<< only this

namespace ABCClinicSystem.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required] [Display(Name = "Doctor")] public string DoctorId { get; set; }

        [Required] [Display(Name = "Patient")] public string PatientId { get; set; }

        [Required]
        [Display(Name = "Appointment Date & Time")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        [MaxLength(500)]
        [Display(Name = "Reason for Visit")]
        public string? Reason { get; set; }

        [MaxLength(100)]
        [Display(Name = "Appointment Type")]
        public string? AppointmentType { get; set; }

        [Display(Name = "Status")] public string Status { get; set; } = "Scheduled";

        // 🔹 Navigation properties — just add [ValidateNever]
        [ValidateNever] public ApplicationUser Doctor { get; set; }

        [ValidateNever] public ApplicationUser Patient { get; set; }

        // 🔹 Dropdowns for Razor forms (NotMapped)
        [NotMapped] public List<SelectListItem> Doctors { get; set; } = new();

        [NotMapped] public List<SelectListItem> Patients { get; set; } = new();

        [NotMapped]
        public List<SelectListItem> AppointmentTypes { get; set; } = new()
        {
            new SelectListItem { Value = "Check-up", Text = "Check-up" },
            new SelectListItem { Value = "Follow-up", Text = "Follow-up" },
            new SelectListItem { Value = "Consultation", Text = "Consultation" }
        };

        public int? DepartmentId { get; set; }
        [ValidateNever] public ServiceDepartment Department { get; set; }

        public int? ServiceId { get; set; }
        [ValidateNever] public Service Service { get; set; }

// Optional dropdowns for Razor forms
        [NotMapped] public List<SelectListItem> Departments { get; set; } = new();

        [NotMapped] public List<SelectListItem> Services { get; set; } = new();
    }
}

