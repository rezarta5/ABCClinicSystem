using System;
using System.ComponentModel.DataAnnotations;

namespace ABCClinicSystem.Models
{
    public class DoctorSchedule
    {
        public int Id { get; set; }

        [Required]
        public string DoctorId { get; set; } = null!; 

        public ApplicationUser Doctor { get; set; } = null!;

        public DateTime AvailableDate { get; set; }

        public bool IsBooked { get; set; } = false;

        public string Status => IsBooked ? "Booked" : "Available";
    }
}