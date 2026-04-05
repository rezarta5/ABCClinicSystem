// Models/Admin.cs
namespace ABCClinicSystem.Models
{
    public class Admin
    {
        // Existing dashboard stats
        public int TotalUsers { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }

        // New bill-related stats
        public int TotalBills { get; set; }          // Total bills created
        public int PendingBills { get; set; }        // Bills with Status = "Pending"
        public decimal TotalRevenue { get; set; }    // Sum of Amount for Paid bills
    }
}