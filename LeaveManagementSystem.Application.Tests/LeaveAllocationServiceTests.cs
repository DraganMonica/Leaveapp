using AutoMapper;
using LeaveManagementSystem.Application.Models.LeaveAllocations;
using LeaveManagementSystem.Application.Services;
using LeaveManagementSystem.Application.Services.LeaveAllocations;
using LeaveManagementSystem.Application.Services.Periods;
using LeaveManagementSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace LeaveManagementSystem.Application.Tests;

[TestFixture]
public class LeaveAllocationsServiceTests
{
    private ApplicationDbContext _context;
    private Mock<IUserService> _userServiceMock;
    private Mock<IMapper> _mapperMock;
    private Mock<IPeriodsService> _periodsServiceMock;
    private Mock<UserManager<ApplicationUser>> _userManagerMock;
    private LeaveAllocationsService _sut;//=system under test, adica obiectul pe care l testez

    //setez eu niste date pe care le testez
    private const string EmployeeId = "emp-001";

    private static Period CurrentPeriod => new()
    {
        Id = 1,
        Name = "2026",
        StartDate = new DateOnly(2026, 1, 1),
        EndDate = new DateOnly(2026, 12, 31)
    };

    private static ApplicationUser TestEmployee => new()
    {
        Id = EmployeeId,
        FirstName = "Ion",
        LastName = "Popescu",
        Email = "ion.popescu@leaveapp.com",
        DateOfBirth = new DateTime(1990, 5, 15),
        DateOfEmployment = new DateOnly(2025,5,18)
    };
    // ─── Helper: UserManager are constructor complicat → necesita IUserStore mock ─
    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var storeMock = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }
    //Setup 

    [SetUp]
    public void SetUp()
    {
        // Fresh in-memory DB per test to avoid state leaking between tests
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _periodsServiceMock = new Mock<IPeriodsService>();
        _userManagerMock = CreateUserManagerMock();

        _periodsServiceMock
            .Setup(s => s.GetCurrentPeriod())
            .ReturnsAsync(CurrentPeriod);

        // Default: orice angajat cautat → TestEmployee (fara data angajarii)
        _userManagerMock
            .Setup(u => u.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(TestEmployee);

        _sut = new LeaveAllocationsService(
            _context,
            _userServiceMock.Object,
            _userManagerMock.Object, 
            _mapperMock.Object,
            _periodsServiceMock.Object);
    }

    [TearDown] 
    //                        inchide context ul dupa fiecare test(curat memoria)
    public void TearDown() => _context.Dispose();

    // 1.AllocateLeave


    [Test]// imi creeaza alocarile
    public async Task AllocateLeave_WhenNoAllocationsExist_CreatesOneAllocationPerLeaveType()
    {
        // Arrange
        _context.LeaveTypes.AddRange(
            new LeaveType { Id = 1, Name = "Concediu anual", NumberOfDays = 24 },
            new LeaveType { Id = 2, Name = "Medical", NumberOfDays = 12 });
        await _context.SaveChangesAsync();

        // Act
        await _sut.AllocateLeave(EmployeeId);
        // Assert
        var allocations = await _context.LeaveAllocations
            .Where(a => a.EmployeeId == EmployeeId)
            .ToListAsync();

        Assert.That(allocations, Has.Count.EqualTo(2),
            "Ar trebui să existe câte o alocare pentru fiecare tip de concediu.");
    }

    [Test]
    public async Task AllocateLeave_WhenAllocationAlreadyExists_SkipsThatLeaveType()
    {
        // Arrange –aici adaug tipurile de leavetype
        _context.LeaveTypes.AddRange(
            new LeaveType { Id = 1, Name = "Concediu anual", NumberOfDays = 24 },
            new LeaveType { Id = 2, Name = "Medical", NumberOfDays = 12 });

        // x ulescu are deja concediu anual chiar daca e de 10 zile, si ma intereseaza sa se adauge doar medical
        _context.LeaveAllocations.Add(new LeaveAllocation
        {
            EmployeeId = EmployeeId,
            LeaveTypeId = 1,
            PeriodId = CurrentPeriod.Id,
            Days = 10
        });

        await _context.SaveChangesAsync();

        // Act
        await _sut.AllocateLeave(EmployeeId);

        // Assert
        var allocations = await _context.LeaveAllocations
            .Where(a => a.EmployeeId == EmployeeId)
            .ToListAsync();

        Assert.That(allocations, Has.Count.EqualTo(2),
            "Tipul deja alocat nu trebuie sa fie duplicat.");
        // verific daca sunt exact id urile de la leave type pe care le am introdus mai sus
        Assert.That(allocations.Select(a => a.LeaveTypeId),
            Is.EquivalentTo(new[] { 1, 2 }));
    }

    //Zilele sunt calculate proportional in functie de lunile ramase
    [Test]
    public async Task AllocateLeave_DaysAreCalculatedProRata_BasedOnPeriodStart_WhenDateOfEmploymentIsNull()
    {
        // Arrange - accent pe nr de zile
        _context.LeaveTypes.Add(new LeaveType { Id = 1, Name = "Test", NumberOfDays = 12 });
        await _context.SaveChangesAsync();

        var period = CurrentPeriod;
        var startDate = period.StartDate;

        var monthsRemaining = ((period.EndDate.Year - startDate.Year) * 12)
                            + period.EndDate.Month - startDate.Month + 1;

        var expectedDays = (int)Math.Ceiling(decimal.Divide(12, 12) * monthsRemaining);

        // Act - se creeaza alocarea
        await _sut.AllocateLeave(EmployeeId);

        // Assert
        var allocation = await _context.LeaveAllocations
            .SingleAsync(a => a.EmployeeId == EmployeeId);

        Assert.That(allocation.Days, Is.EqualTo(expectedDays),
            "Zilele alocate nu sunt calculate corect pe baza perioadei curente cand data angajarii lipseste.");
    }


    //verifica daca alocarea are Employee+ period corect
    [Test]
    public async Task AllocateLeave_SetsCorrectPeriodAndEmployee()
    {
        // Arrange
        _context.LeaveTypes.Add(new LeaveType { Id = 1, Name = "Concediu", NumberOfDays = 24 });
        await _context.SaveChangesAsync();

        // Act
        await _sut.AllocateLeave(EmployeeId);

        // Assert
        var allocation = await _context.LeaveAllocations.SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(allocation.EmployeeId, Is.EqualTo(EmployeeId));
            Assert.That(allocation.PeriodId, Is.EqualTo(CurrentPeriod.Id));
            Assert.That(allocation.LeaveTypeId, Is.EqualTo(1));
        });
    }

    
    // 2.GetEmployeeAllocations
    [Test]
    public async Task GetEmployeeAllocations_WithNullUserId_UsesLoggedInUser()
    {
        // Arrange
        _userServiceMock.Setup(s => s.GetLoggedInUser()).ReturnsAsync(TestEmployee);
        _mapperMock
            .Setup(m => m.Map<List<LeaveAllocation>, List<LeaveAllocationVM>>(
                It.IsAny<List<LeaveAllocation>>()))
            .Returns(new List<LeaveAllocationVM>());

        // Act
        var result = await _sut.GetEmployeeAllocations(null);

        // Assert
        //ii dau ce am in testempl
        _userServiceMock.Verify(s => s.GetLoggedInUser(), Times.Once);
        _userServiceMock.Verify(s => s.GetUserById(It.IsAny<string>()), Times.Never);
        Assert.That(result.Id, Is.EqualTo(EmployeeId));
    }

    [Test]
    public async Task GetEmployeeAllocations_WithSpecificUserId_FetchesThatUser()
    {
        // Arrange
        const string targetId = "trg-007";
        var manager = new ApplicationUser
        {
            Id = targetId
        };

        //daca cineva mi cere un user cu id ul de mai sus, da i manager
        _userServiceMock.Setup(s => s.GetUserById(targetId)).ReturnsAsync(manager);
        _mapperMock
            .Setup(m => m.Map<List<LeaveAllocation>, List<LeaveAllocationVM>>(
                It.IsAny<List<LeaveAllocation>>()))
            .Returns(new List<LeaveAllocationVM>());

        // Act
        var result = await _sut.GetEmployeeAllocations(targetId);

        // Assert
        _userServiceMock.Verify(s => s.GetUserById(targetId), Times.Once);
        Assert.That(result.Id, Is.EqualTo(targetId));
    }

    [Test]
    public async Task GetEmployeeAllocations_IsCompletedAllocation_TrueWhenCountsMatch()
    {
        // Arrange 
        _context.LeaveTypes.AddRange(
            new LeaveType { Id = 1, Name = "Concediu anual", NumberOfDays = 24 },
            new LeaveType { Id = 2, Name = "Medical", NumberOfDays = 12 });

        _context.Periods.Add(CurrentPeriod);

        //
        _context.LeaveAllocations.AddRange(
            new LeaveAllocation { EmployeeId = EmployeeId, LeaveTypeId = 1, PeriodId = CurrentPeriod.Id, Days = 10 },
            new LeaveAllocation { EmployeeId = EmployeeId, LeaveTypeId = 2, PeriodId = CurrentPeriod.Id, Days = 5 });

        await _context.SaveChangesAsync();

        _userServiceMock.Setup(s => s.GetLoggedInUser()).ReturnsAsync(TestEmployee);
        _mapperMock
            .Setup(m => m.Map<List<LeaveAllocation>, List<LeaveAllocationVM>>(
                It.IsAny<List<LeaveAllocation>>()))
            .Returns(new List<LeaveAllocationVM> { new(), new() });

        // Act
        var result = await _sut.GetEmployeeAllocations(null);

        // Assert
        Assert.That(result.IsCompletedAllocation, Is.True,
            "IsCompletedAllocation trebuie sa fie true cand numarul de alocari egaleaza numarul de tipuri.");
    }

    [Test]
    public async Task GetEmployeeAllocations_IsCompletedAllocation_FalseWhenAllocationsMissing()
    {
        // Arrange – 2 leave types, only 1 allocation
        _context.LeaveTypes.AddRange(
            new LeaveType { Id = 1, Name = "Concediu anual", NumberOfDays = 24 },
            new LeaveType { Id = 2, Name = "Medical", NumberOfDays = 12 });

        _context.Periods.Add(CurrentPeriod);

        _context.LeaveAllocations.Add(
            new LeaveAllocation { EmployeeId = EmployeeId, LeaveTypeId = 1, PeriodId = 1, Days = 10 });
            //nu adaug 2, altfel se face iscompleted=false
        await _context.SaveChangesAsync();

        _userServiceMock.Setup(s => s.GetLoggedInUser()).ReturnsAsync(TestEmployee);
        _mapperMock
            .Setup(m => m.Map<List<LeaveAllocation>, List<LeaveAllocationVM>>(
                It.IsAny<List<LeaveAllocation>>()))
            .Returns(new List<LeaveAllocationVM> { new() });

        // Act
        var result = await _sut.GetEmployeeAllocations(null);

        // Assert
        Assert.That(result.IsCompletedAllocation, Is.False);
    }
    

    
    // 3.EditAllocation
    [Test]
    public async Task EditAllocation_UpdatesDaysOnExistingRecord()
    {
        // Arrange
        _context.Periods.Add(CurrentPeriod);
        _context.LeaveTypes.Add(new LeaveType { Id = 1, Name = "Concediu", NumberOfDays = 24 });
        _context.LeaveAllocations.Add(new LeaveAllocation
        {
            Id = 1,
            EmployeeId = EmployeeId,
            LeaveTypeId = 1,
            PeriodId = 1,
            Days = 10
        });
        await _context.SaveChangesAsync();

        var editVM = new LeaveAllocationEditVM { Id = 1, Days = 20 };

        // Act
        await _sut.EditAllocation(editVM);

        // Assert
        var updated = await _context.LeaveAllocations.FindAsync(1);
        Assert.That(updated!.Days, Is.EqualTo(20),
            "Zilele trebuie să fie actualizate la valoarea din VM.");
    }

    
    // 4.GetCurrentAllocation

    [Test]
    public async Task GetCurrentAllocation_ReturnsAllocationForCurrentPeriod()
    {
        // Arrange
        _context.Periods.Add(CurrentPeriod);
        _context.LeaveTypes.Add(new LeaveType { Id = 1, Name = "Concediu", NumberOfDays = 24 });
        _context.LeaveAllocations.Add(new LeaveAllocation
        {
            EmployeeId = EmployeeId,
            LeaveTypeId = 1,
            PeriodId = CurrentPeriod.Id,
            Days = 18
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCurrentAllocation(leaveTypeId: 1, employeeId: EmployeeId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Days, Is.EqualTo(18));
        Assert.That(result.PeriodId, Is.EqualTo(CurrentPeriod.Id));
    }

    [Test]
    public void GetCurrentAllocation_WhenNotFound_ThrowsInvalidOperationException()
    {
        // Arrange – nothing in DB - FirstAsync throws
        // Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GetCurrentAllocation(leaveTypeId: 99, employeeId: EmployeeId),
            "Daca nu exista alocarea, FirstAsync trebuie sa arunce exceptie.");
    }

}