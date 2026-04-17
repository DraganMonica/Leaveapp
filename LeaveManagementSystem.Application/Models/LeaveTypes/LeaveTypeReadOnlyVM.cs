
namespace LeaveManagementSystem.Application.Models.LeaveTypes
{
    public class LeaveTypeReadOnlyVM: BaseLeaveTypeVM
    {
        public string Name { get; set; }=string.Empty;
        [Display(Name = "Maximum Number of Days")]
        public int NumberOfDays { get; set; }
    }

}
