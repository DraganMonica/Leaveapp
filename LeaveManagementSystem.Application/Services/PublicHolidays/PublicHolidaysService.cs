using LeaveManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LeaveManagementSystem.Application.Services.PublicHolidays;

public class PublicHolidaysService(ApplicationDbContext _context) : IPublicHolidaysService
{
    //returneaza toate sarbatorile legale dintr-un an specific, sortate dupa data
    //cand vreau sa afisez lista de zile libere
    public async Task<List<PublicHoliday>> GetHolidaysByYear(int year)
    {
        return await _context.PublicHolidays
            .Where(h => h.Year == year)
            .OrderBy(h => h.Date)
            .ToListAsync();
    }


    //creeaza o sarbatoare legala noua
    public async Task CreateHoliday(string name, DateOnly date)
    {
        var holiday = new PublicHoliday
        {
            Name = name,
            Date = date,
            Year = date.Year
        };
        _context.Add(holiday);
        await _context.SaveChangesAsync();
    }


    //sterge o sarbatoare dupa id. Daca nu exista, nu arunca exceptie
    public async Task DeleteHoliday(int id)
    {
        var holiday = await _context.PublicHolidays.FindAsync(id);
        if (holiday == null) return;

        _context.PublicHolidays.Remove(holiday);
        await _context.SaveChangesAsync();
    }


    //calculeaza cate zile lucratoare sunt intre doua date, fara a lua in calcul weekend urile si sarbatorile legale din db
    public async Task<int> GetWorkingDays(DateOnly start, DateOnly end)
    {
        var publicHolidays = await _context.PublicHolidays
            .Where(h => h.Date >= start && h.Date <= end)
            .Select(h => h.Date)
            .ToListAsync();

        int workingDays = 0;
         
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday &&date.DayOfWeek != DayOfWeek.Sunday && !publicHolidays.Contains(date))
            {
                workingDays++;
            }
        }
        return workingDays;
    }
}