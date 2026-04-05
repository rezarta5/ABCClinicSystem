namespace ABCClinicSystem.Models;

public class ServiceDepartment
{
    public int Id { get; set; }
    public string Name { get; set; }  
    public string? Description { get; set; }

    // Navigation for services
    public List<Service> Services { get; set; } = new();
}