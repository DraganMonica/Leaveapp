using LeaveManagementSystem.Application.Services.PublicHolidays;

namespace LeaveManagementSystem.Web.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class PublicHolidaysController(IPublicHolidaysService _publicHolidaysService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var year = DateTime.Now.Year;
            var holidays = await _publicHolidaysService.GetHolidaysByYear(year);
            ViewBag.Year = year;
            return View(holidays);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, DateOnly date)
        {
            if (string.IsNullOrEmpty(name))
            {
                ModelState.AddModelError("name", "Name is required.");
                return View();
            }
            await _publicHolidaysService.CreateHoliday(name, date);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _publicHolidaysService.DeleteHoliday(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
