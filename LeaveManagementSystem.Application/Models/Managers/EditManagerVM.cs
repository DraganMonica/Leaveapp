namespace LeaveManagementSystem.Application.Models.Managers
{
    public class EditManagerVM
    {
        public string Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly DateOfEmployment { get; set; }
    }
}
