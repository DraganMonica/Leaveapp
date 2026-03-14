using LeaveManagementSystem.Application.Models.LeaveRequests;

namespace LeaveManagementSystem.Application.Services.LeaveRequests
{
    public interface ILeaveRequestsService
    {
        Task CreateLeaveRequest(LeaveRequestCreateVM model);
        //le permite  angajatilor sa vada doar cererile lor (managerii si adminii pot vedea toate cererile)
        Task<List<LeaveRequestListVM>> GetEmployeeLeaveRequests();
        Task<EmployeeLeaveRequestListVM> AdminGetAllLeaveRequests();
        Task CancelLeaveRequest(int id);
        Task ReviewLeaveRequest(int leaveRequestId, bool approved);
        Task <bool> RequestDatesExceedAllocation(LeaveRequestCreateVM model);
        Task<ReviewLeaveRequestVM> GetLeaveRequestForReview(int id);
    }
}