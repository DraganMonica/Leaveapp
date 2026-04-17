using LeaveManagementSystem.Application.Services.Departments;
using LeaveManagementSystem.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LeaveManagementSystem.Web.Controllers;

[Authorize(Roles = Roles.Administrator)]
public class DepartmentsController(
        IDepartmentsService _departmentsService,
        UserManager<ApplicationUser> _userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var departments = await _departmentsService.GetAllDepartments();
        return View(departments);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateManagersList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? managerId)
    {
        if (string.IsNullOrEmpty(name))
        {
            ModelState.AddModelError("name", "Name is required.");
            await PopulateManagersList();
            return View();
        }
        await _departmentsService.CreateDepartment(name);
        if (!string.IsNullOrEmpty(managerId))
        {
            var dept = (await _departmentsService.GetAllDepartments())
                .OrderByDescending(d => d.Id).First();
            await _departmentsService.AssignManager(dept.Id, managerId);
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dept = await _departmentsService.GetDepartmentById(id);
        if (dept == null) return NotFound();
        await PopulateManagersList(dept.ManagerId);
        return View(dept);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string name, string? managerId)
    {
        await _departmentsService.UpdateDepartment(id, name, managerId);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var dept = await _departmentsService.GetDepartmentById(id);
        if (dept == null) return NotFound();
        return View(dept);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _departmentsService.DeleteDepartment(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateManagersList(string? selectedId = null)
    {
        var managers = await _userManager.GetUsersInRoleAsync(Roles.Manager);
        ViewBag.Managers = new SelectList(managers, "Id", "Email", selectedId);
    }

    public async Task<IActionResult> ManageEmployees(int id)
    {
        var dept = await _departmentsService.GetDepartmentById(id);
        if (dept == null) return NotFound();

        var unassigned = await _departmentsService.GetUnassignedEmployees();
        ViewBag.UnassignedEmployees = new SelectList(unassigned, "Id", "Email");
        return View(dept);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEmployee(int departmentId, string employeeId)
    {
        await _departmentsService.AssignEmployeeToDepartment(employeeId, departmentId);
        return RedirectToAction(nameof(ManageEmployees), new { id = departmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveEmployee(int departmentId, string employeeId)
    {
        await _departmentsService.AssignEmployeeToDepartment(employeeId, null);
        return RedirectToAction(nameof(ManageEmployees), new { id = departmentId });
    }
}
