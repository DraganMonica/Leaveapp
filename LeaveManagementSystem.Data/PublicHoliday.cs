namespace LeaveManagementSystem.Data;

public class PublicHoliday : BaseEntity
{
    public string Name { get; set; }
    public DateOnly Date { get; set; }
    public int Year { get; set; }
}
