using System;
using System.ComponentModel.DataAnnotations;

namespace ABCClinicSystem.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }

        // Patient (required)
        [Required(ErrorMessage = "Patient field is required.")]
        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }

        // Doctor (required)
        [Required(ErrorMessage = "Doctor field is required.")]
        public string DoctorId { get; set; }
        public ApplicationUser Doctor { get; set; }

        // Diagnosis (required)
        [Required(ErrorMessage = "Diagnosis field is required.")]
        [StringLength(500, ErrorMessage = "Diagnosis cannot exceed 500 characters.")]
        public string Diagnosis { get; set; }

        // Optional notes
        [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters.")]
        public string Notes { get; set; }

        // Record creation date (UTC)
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}