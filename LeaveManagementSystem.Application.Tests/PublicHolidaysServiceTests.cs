using LeaveManagementSystem.Application.Services.PublicHolidays;
using LeaveManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LeaveManagementSystem.Application.Tests;

[TestFixture]
public class PublicHolidaysServiceTests
{
    private ApplicationDbContext _context;
    private PublicHolidaysService _service;

    [SetUp]
    public void SetUp()
    {
        // InMemory DB cu nume unic per test — izolare totala
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new PublicHolidaysService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    //1)
    // daca user ul cere concediu intr o saptaman fara sarbatori, cate zile lucratoare sunt?
    [Test]
    public async Task GetWorkingDays_ReturnsCorrectCount_WhenNoHolidays()
    {
        //arrange
        // Luni - Vineri = 5 zile lucratoare, fara sarbatori
        var start = new DateOnly(2026, 1, 12);
        var end = new DateOnly(2026, 1, 16); 


        //act
        var result = await _service.GetWorkingDays(start, end);


        //assert
        Assert.That(result, Is.EqualTo(5));
    }


    // verifica ca weekend ul nu este luat in considerare
    [Test]
    public async Task GetWorkingDays_ExcludesWeekends()
    {
        
        var start = new DateOnly(2026, 1, 10); // sambata
        var end = new DateOnly(2026, 1, 11); // duminica

        var result = await _service.GetWorkingDays(start, end);

        Assert.That(result, Is.EqualTo(0));
    }


    // luni-vineri DAAAR cu o sarbatoare in mijloc => 4 zile
    [Test]
    public async Task GetWorkingDays_ExcludesPublicHolidays()
    {
        //arrange
        var holiday = new PublicHoliday
        {
            Name = "Test Holiday",
            Date = new DateOnly(2026, 1, 8), // Miercuri
            Year = 2025
        };
        _context.PublicHolidays.Add(holiday);
        await _context.SaveChangesAsync();

        var start = new DateOnly(2026, 1, 5);  // Luni
        var end = new DateOnly(2026, 1, 9); // Vineri


        //act
        var result = await _service.GetWorkingDays(start, end);


        //assert
        Assert.That(result, Is.EqualTo(4)); // 5 - 1 (sarbatoarea)
    }


    //cand user ul poate alege o zi si aceea zi sa fie SAMBATA/DUMINICA <=> 0 zile lucratoare
    [Test]
    public async Task GetWorkingDays_ReturnZero_WhenStartEqualsEnd_AndIsWeekend()
    {
        //arrange
        var saturday = new DateOnly(2026, 1, 10);


        //act
        var result = await _service.GetWorkingDays(saturday, saturday);


        //assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public async Task GetWorkingDays_ReturnsOne_WhenStartEqualsEnd_AndIsWeekday()
    {
        // O singura zi lucratoare => 1
        var monday = new DateOnly(2026, 1, 5);

        var result = await _service.GetWorkingDays(monday, monday);

        Assert.That(result, Is.EqualTo(1));
    }



    //2) 
    [Test]
    public async Task CreateHoliday_SavesHolidayToDatabase()
    {
        var date = new DateOnly(2026, 12, 25);

        await _service.CreateHoliday("Christmas", date);

        var saved = await _context.PublicHolidays.FirstOrDefaultAsync();
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved.Name, Is.EqualTo("Christmas"));
        Assert.That(saved.Date, Is.EqualTo(date));
    }

    [Test]
    public async Task CreateHoliday_SetsYearFromDate_Automatically()
    {
        // Anul trebuie extras automat din data — nu il trimitem separat
        var date = new DateOnly(2026, 6, 1);

        await _service.CreateHoliday("Children's Day", date);

        var saved = await _context.PublicHolidays.FirstOrDefaultAsync();
        Assert.That(saved.Year, Is.EqualTo(2026));
    }

    //3)

    [Test]
    public async Task DeleteHoliday_RemovesHolidayFromDatabase()
    {
        var holiday = new PublicHoliday
        {
            Name = "Test",
            Date = new DateOnly(2026, 5, 1),
            Year = 2026
        };
        _context.PublicHolidays.Add(holiday);
        await _context.SaveChangesAsync();

        await _service.DeleteHoliday(holiday.Id);

        var remaining = await _context.PublicHolidays.CountAsync();
        Assert.That(remaining, Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteHoliday_DoesNotThrow_WhenHolidayNotFound()
    {
        //daca nu gaseste obiectul, nu arunca exceptie
        Assert.DoesNotThrowAsync(async () =>
            await _service.DeleteHoliday(9999));
    }


    // trebuie sa intoarca doar sarbatorile din anul cerut
    [Test]
    public async Task GetHolidaysByYear_ReturnsOnlyHolidaysForGivenYear()
    {
        _context.PublicHolidays.AddRange(
            new PublicHoliday { Name = "H1", Date = new DateOnly(2025, 1, 1), Year = 2025 },
            new PublicHoliday { Name = "H2", Date = new DateOnly(2025, 6, 1), Year = 2025 },
            new PublicHoliday { Name = "H3", Date = new DateOnly(2024, 12, 1), Year = 2024 }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetHolidaysByYear(2025);

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(h => h.Year == 2025), Is.True);
    }

    [Test]
    public async Task GetHolidaysByYear_ReturnsHolidaysOrderedByDate()
    {
        _context.PublicHolidays.AddRange(
            new PublicHoliday { Name = "Late", Date = new DateOnly(2025, 12, 25), Year = 2025 },
            new PublicHoliday { Name = "Early", Date = new DateOnly(2025, 1, 1), Year = 2025 }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetHolidaysByYear(2025);

        Assert.That(result[0].Name, Is.EqualTo("Early"));
        Assert.That(result[1].Name, Is.EqualTo("Late"));
    }
}
