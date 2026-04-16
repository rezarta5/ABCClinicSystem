using System;
using System.ComponentModel.DataAnnotations;

namespace ABCClinicSystem.Models
{
    public class Bill
    {
        public int Id { get; set; }

        [Required]
        public string DoctorId { get; set; }  // Foreign key to ApplicationUser

        [Required]
        public string PatientId { get; set; }   

        [Required]
        [DataType(DataType.Currency)]
        [Range(0.01, 100000, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }    // Pending / Paid / Cancelled

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        
        [StringLength(250)]
        public string Description { get; set; }

        // Navigation properties
        public ApplicationUser Doctor { get; set; }
        public ApplicationUser Patient { get; set; }
   
    }
}