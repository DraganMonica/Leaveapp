using AutoMapper;
using LeaveManagementSystem.Application.Models.LeaveAllocations;
using LeaveManagementSystem.Application.Models.LeaveRequests;
using LeaveManagementSystem.Application.Services.LeaveAllocations;
using LeaveManagementSystem.Application.Services.Managers;
using LeaveManagementSystem.Application.Services.PublicHolidays;
using LeaveManagementSystem.Common.Static;
using LeaveManagementSystem.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeaveManagementSystem.Application.Services.LeaveRequests
{
    public class LeaveRequestsService(IMapper _mapper, IUserService _userService,
        ApplicationDbContext _context, ILeaveAllocationsService _leaveAllocationsService,
        IPublicHolidaysService _publicHolidaysService, UserManager<ApplicationUser> _userManager,
        IManagersService _managersService, ILogger<LeaveRequestsService> _logger) : ILeaveRequestsService
    {


        public async Task CreateLeaveRequest(LeaveRequestCreateVM model)
        {
            _logger.LogInformation("Creating leave request");
            var leaveRequest = _mapper.Map<LeaveRequest>(model);
            var user = await _userService.GetLoggedInUser();
            _logger.LogInformation("Leave request initiated by user {UserId}", user.Id);


            //seteaza cine face cererea
            leaveRequest.EmployeeId = user.Id;
            leaveRequest.LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Pending;//1-valoarea numerica din enum

            _context.Add(leaveRequest);

            await UpdateAllocationDays(leaveRequest, true);
            await _context.SaveChangesAsync();
        }


        //returneaza statistici(cate cereri sunt approved, pending, etc)+ lista de cereri de concediu ale angajatilor celui logat
        /*
        Manager — vede doar cererile angajatilor din departamentul lui
        General Manager — vede doar cererile managerilor lui
        */
        public async Task<EmployeeLeaveRequestListVM> AdminGetAllLeaveRequests()
        {
            var currentUser = await _userService.GetLoggedInUser();

            //toate cererile care nu sunt ale lui
            IQueryable<LeaveRequest> query = _context.LeaveRequests
                .Include(q => q.LeaveType)
                .Where(q => q.EmployeeId != currentUser.Id);

            //verificarea rolului current user ului & filtrarea in functie de rol
            if (await _userManager.IsInRoleAsync(currentUser, Roles.Manager))
            {
                var employees = await _userService.GetEmployees();
                var employeeIds = employees.Select(e => e.Id).ToList();
                query = query.Where(r => employeeIds.Contains(r.EmployeeId));
            }
            else if (await _userManager.IsInRoleAsync(currentUser, Roles.GeneralManager))
            {
                var myManagers = await _managersService.GetMyManagers(currentUser.Id);
                var managerIds = myManagers.Select(m => m.Id).ToList();
                query = query.Where(r => managerIds.Contains(r.EmployeeId));
            }

            var leaveRequests = await query.ToListAsync();

            var leaveRequestVMs = new List<LeaveRequestListVM>();
            foreach (var q in leaveRequests)
            {
                leaveRequestVMs.Add(new LeaveRequestListVM
                {
                    StartDate = q.StartDate,
                    EndDate = q.EndDate,
                    Id = q.Id,
                    LeaveType = q.LeaveType.Name,
                    LeaveRequestStatus = (LeaveRequestStatusEnum)q.LeaveRequestStatusId,
                    NumberOfDays = await _publicHolidaysService.GetWorkingDays(q.StartDate, q.EndDate)
                });
            }
            //fiecare linie numara cate cereri din lista deja filtrata au un anumit status 
            var model = new EmployeeLeaveRequestListVM
            {
                ApprovedRequests = leaveRequests.Count(q => q.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Approved),
                PendingRequests = leaveRequests.Count(q => q.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Pending),
                RejectedRequests = leaveRequests.Count(q => q.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Rejected),
                TotalRequests = leaveRequests.Count,
                LeaveRequests = leaveRequestVMs
            };
            _logger.LogInformation("Stats: Total={Total}, Approved={Approved}, Pending={Pending}, Rejected={Rejected}",
                model.TotalRequests, model.ApprovedRequests, model.PendingRequests, model.RejectedRequests);
            return model;



        }


        //user ul isi vede cererile si poate anula cu 3 zile maximum inaitne sa inceapa concediul
        public async Task<List<LeaveRequestListVM>> GetEmployeeLeaveRequests()
        {
            var user = await _userService.GetLoggedInUser();

            //toate cererile angajatului logat
            var leaveRequests = await _context.LeaveRequests
                .Include(q => q.LeaveType)
                .Where(q => q.EmployeeId == user.Id)
                .ToListAsync();
            //le transforma in format pt UI
            var model = new List<LeaveRequestListVM>();
            foreach (var q in leaveRequests)
            {
                model.Add(new LeaveRequestListVM
                {
                    StartDate = q.StartDate,
                    EndDate = q.EndDate,
                    Id = q.Id,
                    LeaveType = q.LeaveType.Name,
                    LeaveRequestStatus = (LeaveRequestStatusEnum)q.LeaveRequestStatusId,
                    NumberOfDays = await _publicHolidaysService.GetWorkingDays(q.StartDate, q.EndDate),
                    // decide daca poate anula
                    CanCancel = await CanCancelRequest(q)
                });
            }

            _logger.LogInformation("Employee {UserId} has {Count} leave requests",
                user.Id, model.Count);

            return model;
        }


        //user ul apasa pe cancel(verifica daca are voie, seteaza cancelled+reda zilele inapoi)
        public async Task CancelLeaveRequest(int leaveRequestId)
        {
            _logger.LogInformation("Attempting to cancel leave request {RequestId}", leaveRequestId);
            var leaveRequest = await _context.LeaveRequests.FindAsync(leaveRequestId);

            if (!await CanCancelRequest(leaveRequest))
                throw new InvalidOperationException(
                    "You can no longer cancel this request. Cancellation is only allowed at least 3 working days in advance.");

            leaveRequest.LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Cancelled;
            await UpdateAllocationDays(leaveRequest, false);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Leave request {RequestId} cancelled successfully", leaveRequestId);
        }


        //managerul/genralmanager vede cererea, apasa approve/reject, se seteaza statusul, daca e reject se redau zilele inapoi
        public async Task ReviewLeaveRequest(int leaveRequestId, bool approved)
        {
            _logger.LogInformation("Reviewing leave request {RequestId}", leaveRequestId);
            var user = await _userService.GetLoggedInUser();
            var leaveRequest=await _context.LeaveRequests.FindAsync(leaveRequestId);

            // ca manager nu ti aproba singur
            if (leaveRequest.EmployeeId == user.Id)
            {
                _logger.LogWarning("User {UserId} tried to approve own request", user.Id);
                throw new InvalidOperationException("You can't approve your own leave request.");
            }

            leaveRequest.LeaveRequestStatusId = approved 
                ? (int)LeaveRequestStatusEnum.Approved
                : (int)LeaveRequestStatusEnum.Rejected;

            leaveRequest.ReviwerId = user.Id;

            if (!approved)
            {
                await UpdateAllocationDays(leaveRequest, false);
            }
            await _context.SaveChangesAsync();

            _logger.LogInformation("Leave request {RequestId} reviewed. Approved={Approved}",
                leaveRequestId, approved);
        }


        //daca zilele cerute depasesc zilele ramase in alocare
        public async Task<bool> RequestDatesExceedAllocation(LeaveRequestCreateVM model)
        {
            var user = await _userService.GetLoggedInUser();

            var currentDate = DateTime.Now;

            var period = await _context.Periods.SingleAsync(q => q.EndDate.Year == currentDate.Year);

            //cate zile a cerut
            var numberOfDays= CalculateDays(model.StartDate, model.EndDate);

            _logger.LogInformation("Checking allocation for leave request");
            
            var allocation = await _context.LeaveAllocations
                .FirstAsync(q => q.LeaveTypeId == model.LeaveTypeId 
                && q.EmployeeId == user.Id
                && q.PeriodId == period.Id);

            return allocation.Days < numberOfDays;
        }

        //daca working days = 0=> true(adica a ales o perioada care nu are zile lucratoare)
        //daca working days > 0=> false(adica a ales o perioada care are zile lucratoare)
        public async Task<bool> RequestHasNoWorkingDays(LeaveRequestCreateVM model)
        {
            _logger.LogInformation("Checking if request has working days");
            var workingDays = await _publicHolidaysService.GetWorkingDays(
                model.StartDate, model.EndDate);
            return workingDays == 0;
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

            // doar working days, NU calendaristice
            var numberOfDays = await _publicHolidaysService.GetWorkingDays(
                leaveRequest.StartDate, leaveRequest.EndDate);

            //true-scad zilele
            //false-adaug zilele inapoi (in caz de cancel sau reject)
            if (deductDays)
                allocation.Days -= numberOfDays;
            else
                allocation.Days += numberOfDays;

            //EF stie ca trebuie update la db
            _context.Entry(allocation).State = EntityState.Modified;

            _logger.LogInformation("Allocation updated. DeductDays={Deduct}", deductDays);
        }


        // daca exista cerere=> true
        // daca nu exista cerere=> false
        public async Task<bool> HasOverlappingRequest(LeaveRequestCreateVM model)
        {
            var user = await _userService.GetLoggedInUser();

            return await _context.LeaveRequests
                .Where(q => q.EmployeeId == user.Id
                    //ma intereseaza cele care  u sunt rejected sau cancelled
                    && (q.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Pending
                     || q.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Approved)
                    //verific daca se suprapun perioadele
                    && q.StartDate <= model.EndDate
                    && q.EndDate >= model.StartDate)
                .AnyAsync();
        }

        
        private int CalculateDays(DateOnly start, DateOnly end) 
        { 
            return end.DayNumber-start.DayNumber+1;   
        }


        //ma intereseaza sa pot anula doar daca mai am 3 zile lucratoare inainte de startul concediului
        private async Task<bool> CanCancelRequest(LeaveRequest leaveRequest)
        {
            // Nu pot da cancel, daca e deja cancelled sau rejected
            if (leaveRequest.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Cancelled ||
                leaveRequest.LeaveRequestStatusId == (int)LeaveRequestStatusEnum.Rejected)
                return false;

            var today = DateOnly.FromDateTime(DateTime.Now);

            // Daca a inceput deja concediul, nu mai pot da cancel ca user
            if (leaveRequest.StartDate <= today)
                return false;

            // Numara zilele lucratoare ramase pana la start,
            var workingDaysUntilStart = await _publicHolidaysService.GetWorkingDays(today, leaveRequest.StartDate);

            return workingDaysUntilStart > 3;
        }
    }
}
