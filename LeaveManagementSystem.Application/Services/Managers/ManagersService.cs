using LeaveManagementSystem.Application.Models.Managers;
using LeaveManagementSystem.Application.Services.LeaveAllocations;
using LeaveManagementSystem.Common.Static;
using LeaveManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeaveManagementSystem.Application.Services.Managers;

public class ManagersService(
        UserManager<ApplicationUser> _userManager,
        ILeaveAllocationsService _leaveAllocationsService,
        ApplicationDbContext _context, ILogger<ManagersService> _logger) : IManagersService
        
        
{
    public async Task<ManagersListVM> GetAllManagers()
    {
        _logger.LogInformation("Fetching all managers and general managers");
        
        var managers = await _userManager.GetUsersInRoleAsync(Roles.Manager);
        var generalManagers = await _userManager.GetUsersInRoleAsync(Roles.GeneralManager);

        _logger.LogInformation("Found {ManagersCount} managers and {GeneralManagersCount} general managers",
    managers.Count, generalManagers.Count);
        //transform in vm pt UI
        return new ManagersListVM
        {
            Managers = managers.Select(m => new ManagerVM
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Email = m.Email
            }).ToList(),

            GeneralManagers = generalManagers.Select(m => new ManagerVM
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Email = m.Email
            }).ToList()
        };
    }

    //cauta manager dupa id
    public async Task<ManagerVM> GetManagerById(string id)
    {
        _logger.LogInformation("Fetching manager with ID: {Id}", id);
        
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Manager not found with ID: {Id}", id);
            return null;
        }
        return new ManagerVM
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };
    }

    // manager ul si generalmanager ul sunt introduse(create) de catre admin, nu las oricui accesul sa se inregistreze cum vrea
    public async Task CreateManager(CreateManagerVM model)
    {
        _logger.LogInformation("Creating manager with email: {Email}", model.Email);

        var user = BuildUser(model);
        //verifica daca parola e valida, o transforma intr un hash si apoi salveaza user ul în db
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));

            _logger.LogError("Failed to create manager with email {Email}. Errors: {Errors}",
                model.Email, errors);

            throw new Exception(errors);
        }

        _logger.LogInformation("Manager created successfully with email: {Email}", model.Email);

        await _userManager.AddToRolesAsync(user, new[] { Roles.Employee, Roles.Manager });

        _logger.LogInformation("Roles assigned to manager: {Email}", model.Email);

        var userId = await _userManager.GetUserIdAsync(user);

        await _leaveAllocationsService.AllocateLeave(userId);

        _logger.LogInformation("Leave allocated for manager ID: {UserId}", userId);
    }

    public async Task CreateGeneralManager(CreateManagerVM model)
    {
        var user = BuildUser(model);
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));

            _logger.LogError("Failed to create general manager {Email}. Errors: {Errors}",
                model.Email, errors);

            throw new Exception(errors);
        }

        await _userManager.AddToRoleAsync(user, Roles.GeneralManager);

        _logger.LogInformation("General manager created successfully: {Email}", model.Email);
    }

    //datele de profil le setez eu, datele de autentificare le seteaza Identity (ApplicationUser inherits Identity)
    private ApplicationUser BuildUser(CreateManagerVM model) => new ApplicationUser
    {
        UserName = model.Email,
        NormalizedUserName = model.Email.ToUpper(),
        Email = model.Email,
        NormalizedEmail = model.Email.ToUpper(),
        FirstName = model.FirstName,
        LastName = model.LastName,
        DateOfBirth = model.DateOfBirth.ToDateTime(TimeOnly.MinValue),
        DateOfEmployment = model.DateOfEmployment,
        EmailConfirmed = true
    };

    public async Task UpdateManager(string id, EditManagerVM model)
    {
        _logger.LogInformation("Updating manager with ID: {Id}", id);
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Manager not found for update: {Id}", id);
            return;
        }

        user.UserName = model.Email;
        user.NormalizedUserName = model.Email.ToUpper();
        user.Email = model.Email;
        user.NormalizedEmail = model.Email.ToUpper();
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.DateOfBirth = model.DateOfBirth.ToDateTime(TimeOnly.MinValue);
        user.DateOfEmployment = model.DateOfEmployment;

        await _userManager.UpdateAsync(user);
        _logger.LogInformation("Manager updated successfully: {Id}", id);
    }


    public async Task DeleteManager(string id)
    {
        _logger.LogInformation("Deleting manager with ID: {Id}", id);

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            _logger.LogWarning("Manager not found for deletion: {Id}", id);
            return;
        }

        //sterg legatura cu general manager, pentru a nu da eroare 
        var assignments = _context.GeneralManagerManagers
            .Where(x => x.ManagerId == id);

        _context.GeneralManagerManagers.RemoveRange(assignments);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Removed manager relations for ID: {Id}", id);

        //sterg managerul
        await _userManager.DeleteAsync(user);
        _logger.LogInformation("Manager deleted successfully: {Id}", id);
    }


    public async Task<List<ManagerVM>> GetManagersForGeneralManager(string generalManagerId)
    {
        var assignments = await _context.GeneralManagerManagers
            .Where(x => x.GeneralManagerId == generalManagerId)
            .Include(x => x.Manager)
            .ToListAsync();

        return assignments.Select(x => new ManagerVM
        {
            Id = x.Manager.Id,
            FirstName = x.Manager.FirstName,
            LastName = x.Manager.LastName,
            Email = x.Manager.Email
        }).ToList();
    }

    public async Task AssignManagerToGeneralManager(string generalManagerId, string managerId)
    {
        _logger.LogInformation("Assigning manager {ManagerId} to general manager {GeneralManagerId}",
            managerId, generalManagerId);

        // verific daca  manager si generalmanager nu sunt null sau empty
        if (string.IsNullOrEmpty(managerId) || string.IsNullOrEmpty(generalManagerId))
            return;

        var exists = await _context.GeneralManagerManagers
            .AnyAsync(x => x.GeneralManagerId == generalManagerId && x.ManagerId == managerId);

        //daca relatia exista, nu o mai adauga, altfel mai jos o creeaza
        if (exists)
        {
            _logger.LogWarning("Assignment already exists for Manager {ManagerId}", managerId);
            return;
        }

        _context.GeneralManagerManagers.Add(new GeneralManagerManager
        {
            GeneralManagerId = generalManagerId,
            ManagerId = managerId
        });
        await _context.SaveChangesAsync();

        _logger.LogInformation("Assignment created successfully");
    }

    public async Task RemoveManagerFromGeneralManager(string generalManagerId, string managerId)
    {
        _logger.LogInformation("Removing manager {ManagerId} from general manager {GeneralManagerId}",
            managerId, generalManagerId);

        var entry = await _context.GeneralManagerManagers
            .FirstOrDefaultAsync(x => x.GeneralManagerId == generalManagerId && x.ManagerId == managerId);

        if (entry == null)
        {
            _logger.LogWarning("Assignment not found");
            return;
        }

        _context.GeneralManagerManagers.Remove(entry);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Assignment removed successfully");
    }

    
    //cauta manager dupa id
    public async Task<List<ManagerVM>> GetMyManagers(string generalManagerId)
    {
        _logger.LogInformation("Fetching managers for current general manager: {GeneralManagerId}",
            generalManagerId);
        return await _context.GeneralManagerManagers
            .Where(x => x.GeneralManagerId == generalManagerId)
            .Include(x => x.Manager)
            .Select(x => new ManagerVM
            {
                Id = x.Manager.Id,
                FirstName = x.Manager.FirstName,
                LastName = x.Manager.LastName,
                Email = x.Manager.Email
            })
            .ToListAsync();
    }
}
