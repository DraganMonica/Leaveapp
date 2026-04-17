using LeaveManagementSystem.Data;

namespace LeaveManagementSystem.Application.Services.Departments;

public interface IDepartmentsService
{
    Task<List<Department>> GetAllDepartments();
    Task<Department> GetDepartmentById(int id);
    Task CreateDepartment(string name);
    Task UpdateDepartment(int id, string name, string? managerId);
    Task DeleteDepartment(int id);
    Task AssignManager(int departmentId, string managerId);
    Task AssignEmployeeToDepartment(string employeeId, int? departmentId);
    Task<List<ApplicationUser>> GetUnassignedEmployees();
}
