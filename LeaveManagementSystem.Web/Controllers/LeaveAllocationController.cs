using LeaveManagementSystem.Common.Static;
using LeaveManagementSystem.Application.Services.LeaveAllocations;
using LeaveManagementSystem.Application.Services.LeaveTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.Application.Models.LeaveAllocations;

namespace LeaveManagementSystem.Web.Controllers
{
    [Authorize]
    public class LeaveAllocationController (ILeaveAllocationsService _leaveAllocationsService, ILeaveTypesService _leaveTypesService): Controller
    {
        [Authorize(Roles=Roles.Administrator)]
        public async Task<IActionResult> Index()
        {
            var employees = await _leaveAllocationsService.GetEmployees();
            return View(employees);
        }

        [Authorize(Roles = Roles.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AllocateLeave(string? id)
        {
            await _leaveAllocationsService.AllocateLeave(id);
            return RedirectToAction(nameof(Details), new {userId=id});
        }

        [Authorize(Roles = Roles.Administrator)]
        public async Task<IActionResult> EditAllocation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allocation = await _leaveAllocationsService.GetEmployeeAllocation(id);
            if (allocation == null)
            {
                return NotFound();
            }
            return View(allocation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAllocation(LeaveAllocationEditVM allocationEditVM)
        {
            //P1. Validate that the number of days does not exceed the maximum allowed for the selected leave type
            if (await _leaveTypesService.DaysExceedMaximum(allocationEditVM.LeaveType.Id, allocationEditVM.Days)) {
                ModelState.AddModelError("Days", "Number of days exceeds maximum for this leave type.");  
            }
            //P2. Validate that the employee does not already have an allocation for the same leave type in the same period
            if (!ModelState.IsValid)
            {
                await _leaveAllocationsService.EditAllocation(allocationEditVM);
                return RedirectToAction(nameof(Details), new { userId = allocationEditVM.Employee.Id });
            }
            var days=allocationEditVM.Days;
            //reload all the data from the db, asta cu tot cu employee information
            allocationEditVM=await _leaveAllocationsService.GetEmployeeAllocation(allocationEditVM.Id);
            allocationEditVM.Days=days;
            return View(allocationEditVM);
        }

        public async Task<IActionResult> Details(string? userId)
        {
            
            var employeesVM = await _leaveAllocationsService.GetEmployeeAllocations(userId);
            return View(employeesVM);
        }
    }
}
