using LeaveManagementSystem.Application.Models.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveManagementSystem.Application.Services.Managers;

public interface IManagersService
{
    Task<ManagersListVM> GetAllManagers();
    Task<ManagerVM> GetManagerById(string id);
    Task CreateManager(CreateManagerVM model);
    Task CreateGeneralManager(CreateManagerVM model);
    Task UpdateManager(string id, EditManagerVM model);
    Task DeleteManager(string id);
    Task<List<ManagerVM>> GetManagersForGeneralManager(string generalManagerId);
    Task AssignManagerToGeneralManager(string generalManagerId, string managerId);
    Task RemoveManagerFromGeneralManager(string generalManagerId, string managerId);
    Task<List<ManagerVM>> GetMyManagers(string generalManagerId);
}
