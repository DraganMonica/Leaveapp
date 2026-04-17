using LeaveManagementSystem.Data;

namespace LeaveManagementSystem.Application.Services.PublicHolidays;

public interface IPublicHolidaysService
{
    Task<List<PublicHoliday>> GetHolidaysByYear(int year);
    Task CreateHoliday(string name, DateOnly date);
    Task DeleteHoliday(int id);
    Task<int> GetWorkingDays(DateOnly start, DateOnly end);
}
