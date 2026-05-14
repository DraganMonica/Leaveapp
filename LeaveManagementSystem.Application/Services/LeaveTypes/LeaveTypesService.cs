using AutoMapper;
using LeaveManagementSystem.Application.Models.LeaveTypes;
using LeaveManagementSystem.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace LeaveManagementSystem.Application.Services.LeaveTypes;

public class LeaveTypesService(ApplicationDbContext _context, IMapper _mapper,
             ILogger<LeaveTypesService> _logger, IMemoryCache _cache) : ILeaveTypesService
{

    private const string LeaveTypesCacheKey = "LeaveTypes_All";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);



    //returneaza toate tipurile de concediu din DB
    public async Task<List<LeaveTypeReadOnlyVM>> GetAll()
    {
        if (_cache.TryGetValue(LeaveTypesCacheKey, out List<LeaveTypeReadOnlyVM> cached))
        {
            _logger.LogInformation("Cache HIT - LeaveTypes");
            return cached;
        }

        _logger.LogInformation("Cache MISS - fetching LeaveTypes from DB");
        var data = await _context.LeaveTypes.ToListAsync();
        var viewData = _mapper.Map<List<LeaveTypeReadOnlyVM>>(data);

        _cache.Set(LeaveTypesCacheKey, viewData, CacheDuration);

        return viewData;
    }

    // returneaza un singur element dupa id
    public async Task<T?> Get<T>(int id) where T : class
    {
        var cacheKey = $"LeaveType_{id}";

        if (_cache.TryGetValue(cacheKey, out T cached))
        {
            _logger.LogInformation("Cache HIT - LeaveType {Id}", id);
            return cached;
        }

        _logger.LogInformation("Cache MISS - fetching LeaveType {Id} from DB", id);
        var data = await _context.LeaveTypes.FirstOrDefaultAsync(m => m.Id == id);

        if (data == null) return null;

        var viewData = _mapper.Map<T>(data);
        _cache.Set(cacheKey, viewData, CacheDuration);

        return viewData;
    }

    public async Task Remove(int id)
    {
        var data = await _context.LeaveTypes.FirstOrDefaultAsync(m => m.Id == id);
        if (data != null)
        {
            _context.Remove(data);
            await _context.SaveChangesAsync();

            // invalidaez cache dupa stergere
            _cache.Remove(LeaveTypesCacheKey);
            _cache.Remove($"LeaveType_{id}");
            _logger.LogInformation("Cache invalidated after removing LeaveType {Id}", id);
        }
    }

    public async Task Edit(LeaveTypeEditVM model)
    {
        var existing = await _context.LeaveTypes.FindAsync(model.Id);
        if (existing == null) return;

        _mapper.Map(model, existing);
        await _context.SaveChangesAsync();

        // invalidez cache dupa edit
        _cache.Remove(LeaveTypesCacheKey);
        _cache.Remove($"LeaveType_{model.Id}");
        _logger.LogInformation("Cache invalidated after editing LeaveType {Id}", model.Id);
    }


    public async Task Create(LeaveTypeCreateVM model)
    {
        _logger.LogInformation("Creating Leave Type: {Name} - {Days} days",
             model.Name, model.NumberOfDays);

        var leaveType = _mapper.Map<LeaveType>(model);
        _context.Add(leaveType);
        await _context.SaveChangesAsync();

        // invalidam cache dupa creare
        _cache.Remove(LeaveTypesCacheKey);
        _logger.LogInformation("Cache invalidated after creating LeaveType {Name}", model.Name);
    }

    public bool LeaveTypeExists(int id)
    {
        return _context.LeaveTypes.Any(e => e.Id == id);
    }

    public async Task<bool> CheckIfLeaveTypeNameExists(string name)
    {
        var lowercaseName = name.ToLower();
        return await _context.LeaveTypes.AnyAsync(q => q.Name.ToLower().Equals(lowercaseName));
    }

    //acelasi check ca mai sus, doar ca ignora id ul curent, altfel nu as putea salva edit ul fara sa schimb numele
    public async Task<bool> CheckIfLeaveTypeNameExistsForEdit(LeaveTypeEditVM leaveTypeEdit)
    {
        var lowercaseName = leaveTypeEdit.Name.ToLower();
        return await _context.LeaveTypes.AnyAsync(q => q.Name.ToLower().Equals(lowercaseName) && q.Id != leaveTypeEdit.Id);
    }

    //verifica daca numarul cerut de user depaseste numarul alocat de zile
    public async Task<bool> DaysExceedMaximum(int leaveTypeId, int days)
    {
        var leaveType = await _context.LeaveTypes.FindAsync(leaveTypeId);

        return leaveType.NumberOfDays< days;
    }
}
