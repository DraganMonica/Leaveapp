using LeaveManagementSystem.Application.Models.LeaveAllocations;
using LeaveManagementSystem.Data;

namespace LeaveManagementSystem.Application.Services.LeaveAllocations
{
    public interface ILeaveAllocationsService
    {
        Task AllocateLeave(string employeeId);
        //Task<List<LeaveAllocation>> GetAllocations(string? userId);
        Task<EmployeeAllocationVM> GetEmployeeAllocations(string? userId);
        Task<LeaveAllocationEditVM> GetEmployeeAllocation(int? allocationId);
        Task<List<EmployeeListVM>> GetEmployees();
        Task<LeaveAllocation> GetCurrentAllocation(int leaveTypeId, string employeeId);
        Task EditAllocation(LeaveAllocationEditVM allocationEditVM);
    }
}