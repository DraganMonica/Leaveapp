using LeaveManagementSystem.Application.Models.LeaveTypes;

namespace LeaveManagementSystem.Application.Services.LeaveTypes;

public interface ILeaveTypesService
{
    
    Task Create(LeaveTypeCreateVM model);
    Task Edit(LeaveTypeEditVM model);
    Task<T?> Get<T>(int id) where T : class;
    Task<List<LeaveTypeReadOnlyVM>> GetAll();
    Task Remove(int id);
    bool LeaveTypeExists(int id);
    Task<bool> CheckIfLeaveTypeNameExists(string name);
    Task<bool> CheckIfLeaveTypeNameExistsForEdit(LeaveTypeEditVM leaveTypeEdit);
    Task<bool> DaysExceedMaximum(int leaveTypeId, int days);
    
}