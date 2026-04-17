using LeaveManagementSystem.Application.Services.LeaveTypes;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LeaveManagementSystem.Web.Controllers;

[Authorize]
public class LeaveRequestsController(ILeaveTypesService _leaveTypesService, ILeaveRequestsService _leaveRequestsService) : Controller
{
    // employee view requests
    public async Task<IActionResult> Index()
    {
        var model = await _leaveRequestsService.GetEmployeeLeaveRequests();
        return View(model);
    }

    public async Task<IActionResult> Create(int? leaveTypeId)
    {
        var leaveTypes=await _leaveTypesService.GetAll();
        var leaveTypesList=new SelectList(leaveTypes, "Id", "Name",leaveTypeId);
        var model=new LeaveRequestCreateVM
        {
            StartDate=DateOnly.FromDateTime(DateTime.Now),
            EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            LeaveTypes =leaveTypesList
        };
        return View(model);
    }

    // employee create request
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveRequestCreateVM model)
    {
        if (await _leaveRequestsService.HasOverlappingRequest(model))
        {
            ModelState.AddModelError(string.Empty, "You already have a leave request for this period.");
        }

        if (await _leaveRequestsService.RequestHasNoWorkingDays(model))
        {
            ModelState.AddModelError(string.Empty,
                "The selected period does not contain any working days (weekends or public holidays).");
        }

        if (await _leaveRequestsService.RequestDatesExceedAllocation(model))
        {
            ModelState.AddModelError(string.Empty, "You have exceeded your allocation");
            ModelState.AddModelError(nameof(model.EndDate), "The number of requested days exceeds your available balance.");
        }

        if (ModelState.IsValid)
        {
            await _leaveRequestsService.CreateLeaveRequest(model);
            return RedirectToAction(nameof(Index));
        }

        var leaveTypes = await _leaveTypesService.GetAll();
        model.LeaveTypes = new SelectList(leaveTypes, "Id", "Name");
        return View(model);
    }

    // employee cancel request
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _leaveRequestsService.CancelLeaveRequest(id);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }


    [Authorize(Roles = $"{Roles.Administrator}, {Roles.Manager}, {Roles.GeneralManager}")]
    public async Task<IActionResult> ListRequests()
    {
        var model= await _leaveRequestsService.AdminGetAllLeaveRequests();
        return View(model);
    }

    
    public async Task<IActionResult> Review(int id)
    {
        var model= await _leaveRequestsService.GetLeaveRequestForReview(id);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, bool approved)
    {
        await _leaveRequestsService.ReviewLeaveRequest(id, approved);
        return RedirectToAction(nameof(ListRequests));
    }
}

