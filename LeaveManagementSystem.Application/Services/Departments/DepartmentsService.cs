using LeaveManagementSystem.Common.Static;
using LeaveManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeaveManagementSystem.Application.Services.Departments;

public class DepartmentsService(ApplicationDbContext _context, UserManager<ApplicationUser> _userManager, ILogger<DepartmentsService> _logger) : IDepartmentsService
{
    public async Task<List<Department>> GetAllDepartments()
    {
        _logger.LogInformation("Fetching all departments");

        return await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.Employees)
            .ToListAsync();
    }

    public async Task<Department> GetDepartmentById(int id)
    {
        _logger.LogInformation("Fetching department with ID: {Id}", id);

        var department = await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
        {
            _logger.LogWarning("Department not found: {Id}", id);
        }

        return department;
    }

    public async Task CreateDepartment(string name)
    {
        _logger.LogInformation("Creating department with name: {Name}", name);

        var department = new Department { Name = name };
        _context.Add(department);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Department created successfully with name: {Name}", name);
    }


    //adaug manager ul ulterior, nu in creearea departamentului
    public async Task UpdateDepartment(int id, string name, string? managerId)
    {
        _logger.LogInformation("Updating department ID: {Id}", id);

        var department = await _context.Departments.FindAsync(id);
        if (department == null)
        {
            _logger.LogWarning("Department not found for update: {Id}", id);
            return;
        }

        department.Name = name;
        department.ManagerId = managerId;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Department updated successfully: {Id}", id);
    }

    public async Task DeleteDepartment(int id)
    {
        _logger.LogInformation("Deleting department ID: {Id}", id);

        var department = await _context.Departments.FindAsync(id);
        if (department == null)
        {
            _logger.LogWarning("Department not found for deletion: {Id}", id);
            return;
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Department deleted successfully: {Id}", id);
    }

    public async Task AssignManager(int departmentId, string managerId)
    {
        var department = await _context.Departments.FindAsync(departmentId);
        if (department == null) return;

        department.ManagerId = managerId;

        var manager = await _userManager.FindByIdAsync(managerId);
        if (manager != null)
        {
            manager.DepartmentId = departmentId;
            await _userManager.UpdateAsync(manager);

            _logger.LogInformation("Manager {ManagerId} assigned to department {DepartmentId}",
                managerId, departmentId);
        }
        else
        {
            _logger.LogWarning("Manager not found: {ManagerId}", managerId);
        }

        await _context.SaveChangesAsync();
    }



    //face legatura dintre employee si department
    public async Task AssignEmployeeToDepartment(string employeeId, int? departmentId)
    {
        _logger.LogInformation("Assigning employee {EmployeeId} to department {DepartmentId}",
            employeeId, departmentId);

        var user = await _userManager.FindByIdAsync(employeeId);

        if (user == null)
        {
            _logger.LogWarning("Employee not found: {EmployeeId}", employeeId);
            return;
        }


        //previn asignarea unui user la mai multe departamente
        if (user.DepartmentId != null && departmentId != null)
        {
            _logger.LogWarning("Employee {EmployeeId} already assigned to department {DepartmentId}",
                 employeeId, user.DepartmentId);

            throw new Exception("Employee is already assigned to a department.");
        }

        user.DepartmentId = departmentId;
        await _userManager.UpdateAsync(user);
    }

    public async Task<List<ApplicationUser>> GetUnassignedEmployees()
    {
        _logger.LogInformation("Fetching unassigned employees");
        var employees = await _userManager.GetUsersInRoleAsync(Roles.Employee);
        return employees.Where(e => e.DepartmentId == null).ToList();
    }
}
