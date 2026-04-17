namespace LeaveManagementSystem.Data
{
    public class ApplicationUser:IdentityUser
    {
        public string FirstName{ get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateOnly DateOfEmployment { get; set; }
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}
