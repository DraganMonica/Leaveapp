using LeaveManagementSystem.Application.Services;
using LeaveManagementSystem.Common.Static;
using LeaveManagementSystem.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LeaveManagementSystem.Application.Services
{
    public class UserService(UserManager<ApplicationUser> _userManager, IHttpContextAccessor _httpContextAccessor, ILogger<UserService> _logger) : IUserService
    {

        //returneaza utilizatorul autentificat curent(din sesiunea curenta) din contextul htttp
        public async Task<ApplicationUser> GetLoggedInUser()
        {
            //
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);

            if (user == null)
            {
                _logger.LogWarning("User not found in db for current HttpContext");
            }
            else
            {
                _logger.LogInformation("Logged-in user with id: {UserId}", user.Id);
            }

            return user;
        }


        //returneaza un user pe baza id ului
        public async Task<ApplicationUser> GetUserById(string userId)
        {
            _logger.LogInformation("Fetching user by ID: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
            }
            else
            {
                _logger.LogInformation("User found: {UserId}", user.Id);
            }

            return user;
        }


        //returneaza lista de anajati in functie de rolul user ului curent(Manager, GeenralManager, Admin)
        public async Task<List<ApplicationUser>> GetEmployees()
        {
            _logger.LogInformation("Fetching employees based on current user role");

            var currentUser = await GetLoggedInUser();
            var roles = await _userManager.GetRolesAsync(currentUser);

            _logger.LogInformation("User {UserId} has roles: {Roles}", currentUser.Id, string.Join(",", roles));
            // Manager - doar employees din departament 
            if (roles.Contains(Roles.Manager))
            {
                var employees = await _userManager.GetUsersInRoleAsync(Roles.Employee);

                //iau toti employees
                return employees
                    .Where(e =>
                    //dar sa fie din acelasi departament cu el
                        e.DepartmentId == currentUser.DepartmentId &&
                        // exclude sa fie el insusi introdus in lista
                        e.Id != currentUser.Id &&
                        // exclude managerii care aveau si rol employee, respectiv generalmanager si manager
                        !_userManager.IsInRoleAsync(e, Roles.Manager).Result &&
                        !_userManager.IsInRoleAsync(e, Roles.GeneralManager).Result)
                    .ToList();
            }

            //  General Manager - doar manageri
            if (roles.Contains(Roles.GeneralManager))
            {
                var managers = await _userManager.GetUsersInRoleAsync(Roles.Manager);

                return managers
                    .Where(m => m.Id != currentUser.Id)
                    .ToList();
            }

            //  Admin - vede toti angajatii
            if (roles.Contains(Roles.Administrator))
            {
                var employees = await _userManager.GetUsersInRoleAsync(Roles.Employee);
                var managers = await _userManager.GetUsersInRoleAsync(Roles.Manager);
                var generalManagers = await _userManager.GetUsersInRoleAsync(Roles.GeneralManager);

                return employees
                    .Concat(managers)
                    .Concat(generalManagers)
                    .Where(u => u.Id != currentUser.Id)
                    .DistinctBy(u => u.Id)
                    .ToList();
            }

            return new List<ApplicationUser>();
        }
    }
}
