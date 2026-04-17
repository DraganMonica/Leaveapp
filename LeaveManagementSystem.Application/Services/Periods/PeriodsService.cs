using LeaveManagementSystem.Data;
using Microsoft.EntityFrameworkCore;


namespace LeaveManagementSystem.Application.Services.Periods
{
    public class PeriodsService(ApplicationDbContext _context) : IPeriodsService
    {
        // imi da perioada curenta in functie de data curenta  - efectiv imi anul curent
        public async Task<Period> GetCurrentPeriod()
        {
            var currentDate=DateTime.Now;
            var period=await _context.Periods.SingleAsync(q=>q.EndDate.Year == currentDate.Year);
            return period;
        }
    }
}
