using LeaveManagementSystem.Application.Services.Periods;
using LeaveManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LeaveManagementSystem.Application.Tests.Periods;


[TestFixture]
public class PeriodsServiceTests
{
    private ApplicationDbContext _context;
    private PeriodsService _service;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new PeriodsService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }



    [Test]
    public async Task GetCurrentPeriod_ReturnsCorrectPeriod_ForCurrentYear()
    {
        var currentYear = DateTime.Now.Year;

        _context.Periods.Add(new Period
        {
            Name = $"{currentYear}-{currentYear + 1}",
            StartDate = new DateOnly(currentYear, 1, 1),
            EndDate = new DateOnly(currentYear, 12, 31)
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetCurrentPeriod();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.EndDate.Year, Is.EqualTo(currentYear));
    }



    //metoda filtreaza corect si returneaza doar perioada relevanta pentru anul curent
    [Test]
    public async Task GetCurrentPeriod_ReturnsCorrectPeriod_IgnoresOtherYears()
    {
        //arrange
        var currentYear = DateTime.Now.Year;
        // Avem doua perioade — una pentru anul curent, una pentru trecut
        // Trebuie sa returneze doar cea pentru anul curent
        _context.Periods.AddRange(
            new Period
            {
                Name = $"{currentYear - 1}",
                StartDate = new DateOnly(currentYear - 1, 1, 1),
                EndDate = new DateOnly(currentYear - 1, 12, 31)
            },
            new Period
            {
                Name = $"{currentYear}",
                StartDate = new DateOnly(currentYear, 1, 1),
                EndDate = new DateOnly(currentYear, 12, 31)
            }
        );
        await _context.SaveChangesAsync();


        //act
        var result = await _service.GetCurrentPeriod();


        //assert
        Assert.That(result.EndDate.Year, Is.EqualTo(currentYear));
    }



    // SingleAsync arunca InvalidOperationException daca nu gaseste nimic
    // trebuie sa existe exact o perioada/an
    [Test]
    public async Task GetCurrentPeriod_ThrowsException_WhenNoPeriodExistsForCurrentYear()
    {
        //arrange
        var pastYear = DateTime.Now.Year - 1;//nu am data pentru anul curent

        _context.Periods.Add(new Period
        {
            Name = $"{pastYear}",
            StartDate = new DateOnly(pastYear, 1, 1),
            EndDate = new DateOnly(pastYear, 12, 31)
        });
        await _context.SaveChangesAsync();

        //ma astept ca metoda sa dea eroare de tipul InvalidOperationException pentru ca nu gaseste perioada curenta
        Assert.ThrowsAsync<InvalidOperationException>(() =>
             _service.GetCurrentPeriod());
    }



    // SingleAsync arunca InvalidOperationException si daca gaseste mai mult de unul
    // Sistem presupune exact 1 perioada per an — duplicatele sunt o eroare de date
    [Test]
    public async Task GetCurrentPeriod_ThrowsException_WhenMultiplePeriodsExistForCurrentYear()
    {
        //arrange
        var currentYear = DateTime.Now.Year;

        _context.Periods.AddRange(
            new Period
            {
                Name = "Period A",
                StartDate = new DateOnly(currentYear, 1, 1),
                EndDate = new DateOnly(currentYear, 6, 30)
            },
            new Period
            {
                Name = "Period B",
                StartDate = new DateOnly(currentYear, 7, 1),
                EndDate = new DateOnly(currentYear, 12, 31)
            }
        );
        await _context.SaveChangesAsync();


        //assert, unde apeleaza si metoda de la care se asteapta sa arunce exceptia
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetCurrentPeriod());
    }

    [Test]
    public async Task GetCurrentPeriod_ThrowsException_WhenNoPeriodsExistAtAll()
    {
        // acelasi comportament(lipseste perioada curenta)
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetCurrentPeriod());
    }
}