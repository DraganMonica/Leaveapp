
using AutoMapper;
using LeaveManagementSystem.Application.Models.LeaveAllocations;
using LeaveManagementSystem.Application.Models.LeaveRequests;
using LeaveManagementSystem.Application.Services.LeaveAllocations;
using LeaveManagementSystem.Data;

using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Application.Services.LeaveRequests
{
    public class LeaveRequestsService(IMapper _mapper, IUserService _userService, ApplicationDbContext _context, ILeaveAllocationsService _leaveAllocationsService) : ILeaveRequestsService
    {
        public async Task CreateLeaveRequest(LeaveRequestCreateVM model)
        {
            //map data to leave request data model
            var leaveRequest=_mapper.Map<LeaveRequest>(model);

            //get logged in employee id
            var user=await _userService.GetLoggedInUser();
            leaveRequest.EmployeeId=user.Id;

            //set LeaveRequestStatusId to pending
            leaveRequest.LeaveRequestStatusId=(int)LeaveRequestStatusEnum.Pending;

            //save leave request
            _context.Add(leaveRequest);

            await UpdateAllocationDays(leaveRequest, true);
            await _context.SaveChangesAsync();
        }

        public async Task CancelLeaveRequest(int leaveRequestId)
        {
            var leaveRequest=await _context.LeaveRequests.FindAsync(leaveRequestId);
            leaveRequest.LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Cancelled;

            await UpdateAllocationDays(leaveRequest, false);
            await _context.SaveChangesAsync();
        }

       

        public async Task<EmployeeLeaveRequestListVM> AdminGetAllLeaveRequests()
        {
            var leaveRequests = await _context.LeaveRequests
               .Include(q => q.LeaveType)
               .ToListAsync();
            var approvedLeaveRequestsCount = leaveRequests.Count(q => q.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Approved);

            var pendingLeaveRequestsCount = leaveRequests.Count(q => q.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Pending);

            var rejectedLeaveRequestsCount = leaveRequests.Count(q => q.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Rejected);

            var leaveRequestModels = leaveRequests.Select(q => new LeaveRequestListVM
            {
                StartDate = q.StartDate,
                EndDate = q.EndDate,
                Id = q.Id,
                LeaveType = q.LeaveType.Name,
                LeaveRequestStatus = (LeaveRequestStatusEnum)q.LeaveRequestStatusId,
                NumberOfDays = q.EndDate.DayNumber - q.StartDate.DayNumber
            }).ToList();


            var model = new EmployeeLeaveRequestListVM
            {
                ApprovedRequests = approvedLeaveRequestsCount,
                PendingRequests= pendingLeaveRequestsCount,
                RejectedRequests= rejectedLeaveRequestsCount,
                TotalRequests=leaveRequests.Count,
                LeaveRequests = leaveRequestModels
            };

            return model;
        }
        public async Task<List<LeaveRequestListVM>> GetEmployeeLeaveRequests()
        {
            var user=await _userService.GetLoggedInUser();
            var leaveRequests=await _context.LeaveRequests
                .Include(q=>q.LeaveType)
                .Where(q=>q.EmployeeId==user.Id)
                .ToListAsync();

            var model=leaveRequests.Select(q => new LeaveRequestListVM
            {
                StartDate=q.StartDate,
                EndDate=q.EndDate,
                Id=q.Id,
                LeaveType =q.LeaveType.Name,
                LeaveRequestStatus =(LeaveRequestStatusEnum)q.LeaveRequestStatusId,
                NumberOfDays=q.EndDate.DayNumber - q.StartDate.DayNumber
            }).ToList();

            return model;
        }

        public async Task ReviewLeaveRequest(int leaveRequestId, bool approved)
        {
            var user = await _userService.GetLoggedInUser();
            var leaveRequest=await _context.LeaveRequests.FindAsync(leaveRequestId);
            leaveRequest.LeaveRequestStatusId = approved 
                ? (int)LeaveRequestStatusEnum.Approved
                : (int)LeaveRequestStatusEnum.Rejected;

            leaveRequest.ReviwerId = user.Id;

            if (!approved)
            {
                await UpdateAllocationDays(leaveRequest, false);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RequestDatesExceedAllocation(LeaveRequestCreateVM model)
        {
            var user = await _userService.GetLoggedInUser();
            var currentDate = DateTime.Now;
            var period = await _context.Periods.SingleAsync(q => q.EndDate.Year == currentDate.Year);
            var numberOfDays= model.EndDate.DayNumber - model.StartDate.DayNumber;
            var allocation = await _context.LeaveAllocations
                .FirstAsync(q => q.LeaveTypeId == model.LeaveTypeId 
                && q.EmployeeId == user.Id
                && q.PeriodId == period.Id);

            return allocation.Days < numberOfDays;
        }

        public async Task<ReviewLeaveRequestVM> GetLeaveRequestForReview(int id)
        {
            var leaveRequest=await _context.LeaveRequests
                .Include(q => q.LeaveType)
                .FirstAsync(q => q.Id== id);

            var user=await _userService.GetUserById(leaveRequest.EmployeeId);

            var model = new ReviewLeaveRequestVM
            {
                StartDate = leaveRequest.StartDate,
                EndDate = leaveRequest.EndDate,
                NumberOfDays = leaveRequest.EndDate.DayNumber - leaveRequest.StartDate.DayNumber,
                LeaveRequestStatus = (LeaveRequestStatusEnum)leaveRequest.LeaveRequestStatusId,
                Id = leaveRequest.Id,
                LeaveType = leaveRequest.LeaveType.Name,
                RequestComments=leaveRequest.RequestComments,
                Employee=new EmployeeListVM
                {
                    Id=leaveRequest.EmployeeId,
                    Email=user.Email,
                    FirstName=user.FirstName,
                    LastName=user.LastName
                }
            };
            return model;
        }

        private async Task UpdateAllocationDays(LeaveRequest leaveRequest, bool deductDays)
        {
            var allocation = await _leaveAllocationsService.GetCurrentAllocation(
                leaveRequest.LeaveTypeId, leaveRequest.EmployeeId);

            var numberOfDays = CalculateDays(leaveRequest.StartDate, leaveRequest.EndDate);

            if (deductDays)
            {
                allocation.Days -= numberOfDays;
            }
            else
            {
                allocation.Days += numberOfDays;
            }

            _context.Entry(allocation).State = EntityState.Modified;
        }

        private int CalculateDays(DateOnly start, DateOnly end) 
        { 
            return end.DayNumber-start.DayNumber;   
        }
    }
}
