using AutoMapper;
using LeaveManagementSystem.Application.Models.LeaveAllocations;
using LeaveManagementSystem.Application.Services.Periods;
using LeaveManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Application.Services.LeaveAllocations;

public class LeaveAllocationsService(ApplicationDbContext _context,IUserService _userService, UserManager<ApplicationUser> _userManager, IMapper _mapper, IPeriodsService _periodsService) : ILeaveAllocationsService
{
    //cat am voie sa iau-
    public async Task AllocateLeave(string employeeId)
    {
        //Ia employee din Identity
        var employee = await _userManager.FindByIdAsync(employeeId);
        if (employee == null)
            throw new Exception("Employee not found");

        // Ia leave types + period
        var leaveTypes = await _context.LeaveTypes.ToListAsync();
        var period = await _periodsService.GetCurrentPeriod();

        // Determina startdate corect
        var startDate = employee.DateOfEmployment > period.StartDate
            ? employee.DateOfEmployment
            : period.StartDate;

        // Calculeaza luni corect 
        var monthsRemaining = ((period.EndDate.Year - startDate.Year) * 12)
                            + period.EndDate.Month - startDate.Month + 1;

        foreach (var leaveType in leaveTypes)
        {
            // Verifica daca exista deja
            bool exists = await AllocationExists(employeeId, period.Id, leaveType.Id);
            if (exists)
                continue;

            var accrualRate = decimal.Divide(leaveType.NumberOfDays, 12);

            var leaveAllocation = new LeaveAllocation
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveType.Id,
                PeriodId = period.Id,
                Days = (int)Math.Ceiling(accrualRate * monthsRemaining)
            };

            _context.Add(leaveAllocation);
        }
        await _context.SaveChangesAsync();
    }


    //- return view model with a lot more data than just a list
    public async Task<EmployeeAllocationVM> GetEmployeeAllocations(string? userId)
    {
        //daca nu i dau parametru(userId), il ia pe cel logat deja
        //daca da, mi l da pe cel cu userId ul dat
        var user = string.IsNullOrEmpty(userId) 
            ? await _userService.GetLoggedInUser()
            :await _userService.GetUserById(userId);

        var allocations = await GetAllocations(user.Id);
        var allocationVMList = _mapper.Map<List<LeaveAllocation>, List<LeaveAllocationVM>>(allocations);
        var leaveTypesCount = await _context.LeaveTypes.CountAsync();
        var employeeVM = new EmployeeAllocationVM
        {
            DateOfBirth = DateOnly.FromDateTime(user.DateOfBirth),
            Email =user.Email,
            FirstName=user.FirstName,
            LastName=user.LastName,
            Id =user.Id,
            LeaveAllocations = allocationVMList,
            IsCompletedAllocation = leaveTypesCount ==allocations.Count
        };

        return employeeVM;
    }

    public async Task<List<EmployeeListVM>> GetEmployees()
    {
        var users = await _userService.GetEmployees();
        //                         tipul lor e asta| de jos si transforma in  ce e jos
        var employees = _mapper.Map<List<ApplicationUser>, List<EmployeeListVM>>(users.ToList());
        return employees;
    }

    //pt o anumita alocare dupa id specific-cand vreau sa vad detaliile pt acea alocare
    public async Task<LeaveAllocationEditVM> GetEmployeeAllocation(int? allocationId)
    {
        var allocation=await _context.LeaveAllocations
            .Include(q => q.LeaveType)
            .Include(q => q.Employee)
            .FirstOrDefaultAsync(q => q.Id == allocationId);

        var model = _mapper.Map<LeaveAllocationEditVM>(allocation);
        return model;
    }

    public async Task EditAllocation(LeaveAllocationEditVM allocationEditVM)
    {
        var entity = await _context.LeaveAllocations
     .FirstOrDefaultAsync(e => e.Id == allocationEditVM.Id);

        if (entity is not null)
        {
            entity.Days = allocationEditVM.Days;
            await _context.SaveChangesAsync();
        }
    }

    //folosita pt cand user ul vrea sa faca un requests de concediu
    public async Task<LeaveAllocation> GetCurrentAllocation(int leaveTypeId, string employeeId)
    {
        var period = await _periodsService.GetCurrentPeriod();

        var allocation = await _context.LeaveAllocations
            .FirstAsync(q => q.LeaveTypeId == leaveTypeId
                && q.EmployeeId == employeeId
                && q.PeriodId == period.Id);

        return allocation;
    }
    private async Task<List<LeaveAllocation>> GetAllocations(string? userId)
    {
        //anul
        var period=await _periodsService.GetCurrentPeriod();
        var leaveAllocations = await _context.LeaveAllocations
            .Include(q => q.LeaveType)
            .Include(q => q.Period)
            .Where(q => q.EmployeeId == userId  && q.Period.Id==period.Id)
            .ToListAsync();
        return leaveAllocations;
    }

    private async Task<bool> AllocationExists(string userId, int periodId, int leaveTypeId) {
        var exists = await _context.LeaveAllocations.AnyAsync(q =>
            q.EmployeeId == userId &&
            q.LeaveTypeId == leaveTypeId &&
            q.PeriodId == periodId);

        return exists;
    }

   
}
