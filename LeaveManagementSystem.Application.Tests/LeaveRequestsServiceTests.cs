using AutoMapper;
using LeaveManagementSystem.Application.MappingProfiles;
using LeaveManagementSystem.Application.Models.LeaveRequests;
using LeaveManagementSystem.Application.Services;
using LeaveManagementSystem.Application.Services.LeaveAllocations;
using LeaveManagementSystem.Application.Services.LeaveRequests;
using LeaveManagementSystem.Application.Services.Managers;
using LeaveManagementSystem.Application.Services.PublicHolidays;
using LeaveManagementSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LeaveManagementSystem.Application.Tests;

[TestFixture]
public class LeaveRequestsServiceTests
{
    //toate dependentele necesare pentru a testa LeaveRequestsService
    private ApplicationDbContext _context;
    private IMapper _mapper;
    private Mock<IUserService> _userServiceMock;
    private Mock<IPublicHolidaysService> _publicHolidaysMock;
    private Mock<ILeaveAllocationsService> _leaveAllocationsMock;
    private Mock<IManagersService> _managersServiceMock;
    private Mock<UserManager<ApplicationUser>> _userManagerMock;
    private LeaveRequestsService _service;

    // user logat-in service folosesc: await _userService.GetLoggedInUser();
    private readonly ApplicationUser _loggedInUser = new()
    {
        Id = "employee-1",
        Email = "employee@test.com",
        FirstName = "Ion",
        LastName = "Popescu"
    };

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        var services = new ServiceCollection();
        services.AddAutoMapper(cfg => { }, typeof(LeaveRequestAutoMapperProfile).Assembly);
        _mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();

        _userServiceMock = new Mock<IUserService>();
        _publicHolidaysMock = new Mock<IPublicHolidaysService>();
        _leaveAllocationsMock = new Mock<ILeaveAllocationsService>();
        _managersServiceMock = new Mock<IManagersService>();

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        // userul logat e _loggedInUser
        _userServiceMock
            .Setup(s => s.GetLoggedInUser())
            .ReturnsAsync(_loggedInUser);

        //ctor
        _service = new LeaveRequestsService(
            _mapper,
            _userServiceMock.Object,
            _context,
            _leaveAllocationsMock.Object,
            _publicHolidaysMock.Object,
            _userManagerMock.Object,
            _managersServiceMock.Object,
            Mock.Of<ILogger<LeaveRequestsService>>());
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    //1)HasOverlappingRequest 
    [Test]
    public async Task HasOverlappingRequest_ReturnsTrue_WhenPendingRequestOverlaps()
    {
        //arrange
        // cerere existenta: 10-20 ian
        _context.LeaveRequests.Add(new LeaveRequest
        {
            EmployeeId = "employee-1",
            LeaveTypeId = 1,
            StartDate = new DateOnly(2026, 1, 10),
            EndDate = new DateOnly(2026, 1, 20),
            LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Pending
        });
        await _context.SaveChangesAsync();

        // cerere noua: 15-25 ian => overlap
        var model = new LeaveRequestCreateVM
        {
            LeaveTypeId = 1,
            StartDate = new DateOnly(2026, 1, 15),
            EndDate = new DateOnly(2026, 1, 25)
        };


        //act
        var result = await _service.HasOverlappingRequest(model);


        //assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasOverlappingRequest_ReturnsFalse_WhenNoOverlap()
    {
        //arrange
        //cerere existenta: 1-5 ian
        _context.LeaveRequests.Add(new LeaveRequest
        {
            EmployeeId = "employee-1",
            LeaveTypeId = 1,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 1, 5),
            LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Pending
        });
        await _context.SaveChangesAsync();

        //cerere noua: 10-15 ian => fara overlap
        var model = new LeaveRequestCreateVM
        {
            LeaveTypeId = 1,
            StartDate = new DateOnly(2026, 1, 10),
            EndDate = new DateOnly(2026, 1, 15)
        };


        //act
        var result = await _service.HasOverlappingRequest(model);


        //assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasOverlappingRequest_ReturnsFalse_WhenExistingRequestIsCancelled()
    {
        //arrange
        // o cerere cancelled NU blocheaza o noua cerere pe acelasi interval
        _context.LeaveRequests.Add(new LeaveRequest
        {
            EmployeeId = "employee-1",
            LeaveTypeId = 1,
            StartDate = new DateOnly(2026, 1, 10),
            EndDate = new DateOnly(2026, 1, 20),
            LeaveRequestStatusId = (int)LeaveRequestStatusEnum.Cancelled
        });
        await _context.SaveChangesAsync();

        var model = new LeaveRequestCreateVM
        {
            LeaveTypeId = 1,
            StartDate = new DateOnly(2026, 1, 10),
            EndDate = new DateOnly(2026, 1, 20)
        };


        //act
        var result = await _service.HasOverlappingRequest(model);


        //assert
        Assert.That(result, Is.False);
    }




    //2)RequestHasNoWorkingDays
    [Test]
    public async Task RequestHasNoWorkingDays_ReturnsTrue_WhenOnlyWeekendsSelected()
    {
        //indiferent de interval intoarce 0 zile lucratoare pentru ca sunt doar weekend uri
        _publicHolidaysMock
            .Setup(s => s.GetWorkingDays(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(0);

        var model = new LeaveRequestCreateVM
        {
            StartDate = new DateOnly(2026, 1, 10), // Sambata
            EndDate = new DateOnly(2026, 1, 11)    // Duminica
        };

        var result = await _service.RequestHasNoWorkingDays(model);

        Assert.That(result, Is.True);
    }



    [Test]
    public async Task RequestHasNoWorkingDays_ReturnsFalse_WhenWorkingDaysExist()
    {
        //arrange
        _publicHolidaysMock
            .Setup(s => s.GetWorkingDays(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(5);//simulez ca sunt 5 zile lucratoare in intervalul selectat

        var model = new LeaveRequestCreateVM
        {
            StartDate = new DateOnly(2026, 1, 5),//luni
            EndDate = new DateOnly(2026, 1, 9)//vineri
        };


        //act
        var result = await _service.RequestHasNoWorkingDays(model);


        //assert
        Assert.That(result, Is.False);
    }




    //3)RequestDatesExceedAllocation
    [Test]
    public async Task RequestDatesExceedAllocation_ReturnsTrue_WhenNotEnoughDaysLeft()
    {
        //arrange
        var currentYear = DateTime.Now.Year;

        // Setup perioada curenta
        var period = new Period
        {
            Name = $"{currentYear}-{currentYear+1}",
            StartDate = new DateOnly(currentYear, 1, 1),
            EndDate = new DateOnly(currentYear, 12, 31)
        };
        _context.Periods.Add(period);

        // angajatul are doar 3 zile ramase
        var allocation = new LeaveAllocation
        {
            LeaveTypeId = 1,
            EmployeeId = "employee-1",
            Days = 3,
            PeriodId = period.Id
        };
        _context.LeaveAllocations.Add(allocation);
        await _context.SaveChangesAsync();

        // cere 5 zile => depaseste number of allocation 
        var model = new LeaveRequestCreateVM
        {
            LeaveTypeId = 1,
            StartDate = new DateOnly(currentYear, 3, 2),
            EndDate = new DateOnly(currentYear, 3, 6) // 5 zile calendaristice
        };


        //act
        var result = await _service.RequestDatesExceedAllocation(model);


        //assert
        Assert.That(result, Is.True);
    }




    [Test]
    public async Task RequestDatesExceedAllocation_ReturnsFalse_WhenEnoughDaysAvailable()
    {
        //arrange   
        var currentYear = DateTime.Now.Year;

        var period = new Period
        {
            Name = $"{currentYear}-{currentYear + 1}",
            StartDate = new DateOnly(currentYear, 1, 1),
            EndDate = new DateOnly(currentYear, 12, 31)
        };
        _context.Periods.Add(period);

        // angajatul are 20 de zile (mai mult decat suficient)
        var allocation = new LeaveAllocation
        {
            LeaveTypeId = 1,
            EmployeeId = "employee-1",
            Days = 20,
            PeriodId = period.Id
        };
        _context.LeaveAllocations.Add(allocation);
        await _context.SaveChangesAsync();

        var model = new LeaveRequestCreateVM
        {
            LeaveTypeId = 1,
            StartDate = new DateOnly(currentYear, 3, 3),
            EndDate = new DateOnly(currentYear, 3, 5) // 3 zile
        };


        //act
        var result = await _service.RequestDatesExceedAllocation(model);


        //assert
        Assert.That(result, Is.False);
    }
}