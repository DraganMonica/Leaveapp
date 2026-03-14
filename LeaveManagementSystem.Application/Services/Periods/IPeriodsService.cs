using LeaveManagementSystem.Data;

namespace LeaveManagementSystem.Application.Services.Periods
{
    public interface IPeriodsService
    {
        Task<Period> GetCurrentPeriod();
    }
}
