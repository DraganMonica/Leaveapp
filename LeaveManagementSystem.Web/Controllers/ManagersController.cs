using LeaveManagementSystem.Application.Models.Managers;
using LeaveManagementSystem.Application.Services.Managers;
using LeaveManagementSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LeaveManagementSystem.Web.Controllers;

[Authorize]
public class ManagersController(IManagersService _managersService,
    UserManager<ApplicationUser> _userManager) : Controller
{
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Index()
    {
        var model = await _managersService.GetAllManagers();
        return View(model);
    }

    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var model = new EditManagerVM
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DateOfBirth = DateOnly.FromDateTime(user.DateOfBirth),
            DateOfEmployment = user.DateOfEmployment
        };
        ViewBag.UserId = id;
        return View(model);
    }


    [Authorize(Roles = Roles.Administrator)]
    public IActionResult CreateManager() => View(new CreateManagerVM());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> CreateManager(CreateManagerVM model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await _managersService.CreateManager(model);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [Authorize(Roles = Roles.Administrator)]
    public IActionResult CreateGeneralManager() => View(new CreateManagerVM());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> CreateGeneralManager(CreateManagerVM model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await _managersService.CreateGeneralManager(model);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditManagerVM model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.UserId = id;
            return View(model);
        }
        await _managersService.UpdateManager(id, model);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Delete(string id)
    {
        await _managersService.DeleteManager(id);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> ManageManagers(string id)
    {
        var gm = await _userManager.FindByIdAsync(id);
        if (gm == null) return NotFound();

        var assignedManagers = await _managersService.GetManagersForGeneralManager(id);
        var allManagers = await _userManager.GetUsersInRoleAsync(Roles.Manager);
        var assignedIds = assignedManagers.Select(m => m.Id).ToList();
        var unassigned = allManagers.Where(m => !assignedIds.Contains(m.Id)).ToList();

        ViewBag.GeneralManagerId = id;
        ViewBag.GeneralManagerName = $"{gm.FirstName} {gm.LastName}";
        ViewBag.AssignedManagers = assignedManagers;
        ViewBag.UnassignedManagers = new SelectList(unassigned, "Id", "Email");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> AddManagerToGM(string generalManagerId, string managerId)
    {
        await _managersService.AssignManagerToGeneralManager(generalManagerId, managerId);
        return RedirectToAction(nameof(ManageManagers), new { id = generalManagerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> RemoveManagerFromGM(string generalManagerId, string managerId)
    {
        await _managersService.RemoveManagerFromGeneralManager(generalManagerId, managerId);
        return RedirectToAction(nameof(ManageManagers), new { id = generalManagerId });
    }

    [Authorize(Roles = $"{Roles.Administrator}, {Roles.GeneralManager}")]
    public async Task<IActionResult> MyManagers()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var managers = await _managersService.GetMyManagers(currentUser.Id);
        return View(managers);
    }
}
