namespace LeaveManagementSystem.Data
{
    public class LeaveRequest:BaseEntity
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }


        //am nevoie de o relatie intre leave request si leave type
        public LeaveType? LeaveType { get; set; }
        public int LeaveTypeId { get; set; }


        public LeaveRequestStatus? LeaveRequestStatus { get; set; } 
        public int LeaveRequestStatusId { get; set; }

        public ApplicationUser? Employee { get; set; }
        public string EmployeeId { get; set; } = default!;

        public ApplicationUser? Reviwer { get; set; }
        public string? ReviwerId { get; set; }

        public string? RequestComments { get; set; }
    }
}