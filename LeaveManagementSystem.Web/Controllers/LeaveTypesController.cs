using Microsoft.EntityFrameworkCore;
using LeaveManagementSystem.Application.Models.LeaveTypes;
using LeaveManagementSystem.Application.Services.LeaveTypes;
using LeaveManagementSystem.Common.Static;


namespace LeaveManagementSystem.Web.Controllers
{
    [Authorize(Roles=Roles.Administrator)]
    public class LeaveTypesController(ILeaveTypesService _leaveTypesService, ILogger<LeaveTypesController> _logger) : Controller
    {
        
        private const string NameExistsValidationMessage="This leave types already exists in the database";
        

        // GET: LeaveTypes
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Loading Leave Types");
            var viewData =await _leaveTypesService.GetAll();
            // return viewmodel to the view
            return View(viewData);
        }

        // GET: LeaveTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveType = await _leaveTypesService.Get<LeaveTypeReadOnlyVM>(id.Value);
            if (leaveType == null)
            {
                return NotFound();
            }
            
            return View(leaveType);
        }

        // GET: LeaveTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LeaveTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveTypeCreateVM leaveTypeCreate)
        {
            if(await _leaveTypesService.CheckIfLeaveTypeNameExists(leaveTypeCreate.Name))
            {
                ModelState.AddModelError(nameof(leaveTypeCreate.Name), NameExistsValidationMessage);
            }
            if (ModelState.IsValid)
            {
                _logger.LogWarning("Leave Type attempt failed due to invalidity");
                await _leaveTypesService.Create(leaveTypeCreate);
                return RedirectToAction(nameof(Index));
            }
            return View(leaveTypeCreate);
        }

        

        // GET: LeaveTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveType = await _leaveTypesService.Get<LeaveTypeEditVM>(id.Value);
            if (leaveType == null)
            {
                return NotFound();
            }

            return View(leaveType);
        }



        // POST: LeaveTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LeaveTypeEditVM leaveTypeEdit)
        {
            if (id != leaveTypeEdit.Id)
            {
                return NotFound();
            }
            if (await _leaveTypesService.CheckIfLeaveTypeNameExistsForEdit(leaveTypeEdit))
            {
                ModelState.AddModelError(nameof(leaveTypeEdit.Name), NameExistsValidationMessage);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await _leaveTypesService.Edit(leaveTypeEdit);
                }
                //două request-uri modifică același rând
                //datele din DB s-au schimbat între load și save
                //EF Core detectează conflict de versiune
                catch (DbUpdateConcurrencyException)
                {
                    if (!_leaveTypesService.LeaveTypeExists(leaveTypeEdit.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(leaveTypeEdit);
        }

        

        // GET: LeaveTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveType = await _leaveTypesService.Get<LeaveTypeReadOnlyVM>(id.Value);
            if (leaveType == null)
            {
                return NotFound();
            }

            return View(leaveType);
        }

        // POST: LeaveTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _leaveTypesService.Remove(id);
            return RedirectToAction(nameof(Index));
        }

        
    }
}
