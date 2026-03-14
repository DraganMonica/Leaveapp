
namespace LeaveManagementSystem.Application.Models.LeaveTypes
{
    public class LeaveTypeEditVM: BaseLeaveTypeVM
    {
        
        [Required]
        [Length(3, 25, ErrorMessage = " Name must be between 4 and 25 characters!")]
        public string Name { get; set; } = string.Empty;
        [Required]
        [Range(1, 126)]
        [Display(Name = "Maximum Number of Days")]
        public int NumberOfDays { get; set; }
    }


}
