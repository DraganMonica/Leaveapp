using AutoMapper;
using LeaveManagementSystem.Data;

using LeaveManagementSystem.Application.Models.LeaveAllocations;

using Microsoft.EntityFrameworkCore;
using LeaveManagementSystem.Application.Services;
using LeaveManagementSystem.Application.Services.Periods;

namespace LeaveManagementSystem.Application.Services.LeaveAllocations;

public class LeaveAllocationsService(ApplicationDbContext _context,IUserService _userService, IMapper _mapper, IPeriodsService _periodsService) : ILeaveAllocationsService
{
    public async Task AllocateLeave(string employeeId)
    {
        var leaveTypes = await _context.LeaveTypes
            .Where(q=>!q.LeaveAllocations.Any(x=> x.EmployeeId == employeeId))
            .ToListAsync();

        var currentDate = DateTime.Now;
        var period = await _periodsService.GetCurrentPeriod();

        var monthsRemaining = period.EndDate.Month - currentDate.Month;

        
        foreach (var leaveType in leaveTypes)
        {
            var accuralRate = decimal.Divide(leaveType.NumberOfDays, 12);

            var leaveAllocation = new LeaveAllocation
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveType.Id,
                PeriodId = period.Id,
                Days = (int)Math.Ceiling(accuralRate * monthsRemaining)
            };

            _context.Add(leaveAllocation);
        }
        await _context.SaveChangesAsync();
    }


    //- return view model with a lot more data than just a list
    public async Task<EmployeeAllocationVM> GetEmployeeAllocations(string? userId)
    {
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
        //var leaveAllocation=await GetEmployeeAllocation(allocationEditVM.Id);
        //if(leaveAllocation == null)
        //{
        //    throw new Exception("Leave allocation record does not exist.");
        //}
        //leaveAllocation.Days=allocationEditVM.Days;
        //_context.Update(leaveAllocation);
        //await _context.SaveChangesAsync();
        await _context.LeaveAllocations
            .Where(q => q.Id == allocationEditVM.Id)
            .ExecuteUpdateAsync(q => q.SetProperty(x => x.Days, allocationEditVM.Days));
    }
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
        var period=await _periodsService.GetCurrentPeriod();
        var leaveAllocations = await _context.LeaveAllocations
            // -join statement
            .Include(q => q.LeaveType)
            .Include(q => q.Period)
            .Where(q => q.EmployeeId == userId 
            && q.Period.Id==period.Id)
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
