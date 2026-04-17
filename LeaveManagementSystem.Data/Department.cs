namespace LeaveManagementSystem.Data;

public class Department : BaseEntity
{
    public string Name { get; set; }

    public string? ManagerId { get; set; }
    public ApplicationUser? Manager { get; set; }

    // Angajații din departament
    public List<ApplicationUser> Employees { get; set; }
}
