namespace LeaveManagementSystem.Data;

public class GeneralManagerManager
{
    public string GeneralManagerId { get; set; }
    public ApplicationUser GeneralManager { get; set; }

    public string ManagerId { get; set; }
    public ApplicationUser Manager { get; set; }
}
