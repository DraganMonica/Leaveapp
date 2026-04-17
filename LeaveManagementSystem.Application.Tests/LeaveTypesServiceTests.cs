using AutoMapper;
using LeaveManagementSystem.Application.Models.LeaveTypes;
using LeaveManagementSystem.Application.Services.LeaveTypes;
using LeaveManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LeaveManagementSystem.Application.Tests;

[TestFixture]
public class LeaveTypesServiceTests
{
    private ApplicationDbContext _context;
    private IMapper _mapper;
    private LeaveTypesService _service;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            //db fake, unic pentru fiecare test
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<LeaveType, LeaveTypeReadOnlyVM>();
            cfg.CreateMap<LeaveType, LeaveTypeEditVM>().ReverseMap();
            cfg.CreateMap<LeaveTypeCreateVM, LeaveType>();
        });
        _mapper = mapperConfig.CreateMapper();

        // Mock pentru ILogger, nu ne intereseaza comportamentul lui in aceste teste
        var logger = new Mock<ILogger<LeaveTypesService>>().Object;
        _service = new LeaveTypesService(_context, _mapper, logger);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    
    //1)
    [Test]
    public async Task Create_SavesLeaveTypeToDatabase()
    {
        //arrange
        var model = new LeaveTypeCreateVM { Name = "Annual Leave", NumberOfDays = 20 };


        //act
        await _service.Create(model);

        //verific db ul
        var saved = await _context.LeaveTypes.FirstOrDefaultAsync();


        //assert
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved.Name, Is.EqualTo("Annual Leave"));
        Assert.That(saved.NumberOfDays, Is.EqualTo(20));
    }

    //2)

    [Test]
    public async Task GetAll_ReturnsAllLeaveTypes_MappedToReadOnlyVM()
    {
        //arrange
        _context.LeaveTypes.AddRange(
            new LeaveType { Name = "Annual", NumberOfDays = 20 },
            new LeaveType { Name = "Sick", NumberOfDays = 10 }
        );
        await _context.SaveChangesAsync();

        //act
        var result = await _service.GetAll();


        //assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result, Is.All.InstanceOf<LeaveTypeReadOnlyVM>());
    }

    [Test]
    public async Task GetAll_ReturnsEmptyList_WhenNoLeaveTypes()
    {
        //fara arrange, nu am leave types, nu adaug nimic in db


        //act
        var result = await _service.GetAll();


        //assert
        Assert.That(result, Is.Empty);
    }



    //3)
    [Test]
    public async Task Get_ReturnsCorrectVM_WhenIdExists()
    {
        //arrange
        var leaveType = new LeaveType { Name = "Annual", NumberOfDays = 20 };
        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();


        //act
        var result = await _service.Get<LeaveTypeReadOnlyVM>(leaveType.Id);


        //assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Annual"));
    }


    [Test]
    public async Task Get_ReturnsNull_WhenIdDoesNotExist()
    {
        var result = await _service.Get<LeaveTypeReadOnlyVM>(999);

        Assert.That(result, Is.Null);
    }



    //4)
    [Test]
    public async Task Edit_UpdatesExistingLeaveType_WithNewValues()
    {
        //arrange
        //declar un leave type, il adaug in db
        var leaveType = new LeaveType { Name = "Annual Leave", NumberOfDays = 20 };
        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();

        //apoi creez un model de edit cu aceleasi id dar alte valori
        var model = new LeaveTypeEditVM
        {
            Id = leaveType.Id,
            Name = "Extended Annual Leave",
            NumberOfDays = 25
        };


        //act
        await _service.Edit(model);


        //assert-verific daca valorile au fost actualizate in db
        var updated = await _context.LeaveTypes.FindAsync(leaveType.Id);
        Assert.That(updated.Name, Is.EqualTo("Extended Annual Leave"));
        Assert.That(updated.NumberOfDays, Is.EqualTo(25));
    }


    [Test]
    public async Task Edit_DoesNotCreateNewRecord_WhenUpdating()
    {
        //arrange
        //creez un leavetype, il adaug in db
        var leaveType = new LeaveType { Name = "Annual", NumberOfDays = 20 };
        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();

        //creez un model de edit cu aceleasi id dar cu alte valori
        var model = new LeaveTypeEditVM
        {
            Id = leaveType.Id,
            Name = "Annual Updated",
            NumberOfDays = 22
        };


        //act
        await _service.Edit(model);


        //assert
        //verific daca a fost creat un nou record in loc sa fie actualizat cel existent
        var count = await _context.LeaveTypes.CountAsync();
        Assert.That(count, Is.EqualTo(1)); //daca ar fi fost creat un nou record, count ar fi 2
    }


    

    //5)
    [Test]
    public async Task Remove_DeletesLeaveType_WhenExists()
    {
        //arrange
        var leaveType = new LeaveType { Name = "Annual", NumberOfDays = 20 };
        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();


        //act
        await _service.Remove(leaveType.Id);


        //assert
        var remaining = await _context.LeaveTypes.CountAsync();
        Assert.That(remaining, Is.EqualTo(0));
    }

    [Test]
    public async Task Remove_DoesNotThrow_WhenIdDoesNotExist()
    {
        Assert.DoesNotThrowAsync(async () =>
            await _service.Remove(9999));
    }



    //6)
    [Test]
    public async Task CheckIfLeaveTypeNameExists_ReturnsTrue_WhenNameExists()
    {
        _context.LeaveTypes.Add(new LeaveType { Name = "Annual Leave", NumberOfDays = 20 });
        await _context.SaveChangesAsync();

        var result = await _service.CheckIfLeaveTypeNameExists("Annual Leave");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CheckIfLeaveTypeNameExists_IsCaseInsensitive()
    {
        _context.LeaveTypes.Add(new LeaveType { Name = "Annual Leave", NumberOfDays = 20 });
        await _context.SaveChangesAsync();

        var result = await _service.CheckIfLeaveTypeNameExists("annual Leave");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CheckIfLeaveTypeNameExists_ReturnsFalse_WhenNameDoesNotExist()
    {
        var result = await _service.CheckIfLeaveTypeNameExists("Nonexistent");

        Assert.That(result, Is.False);
    }

    //7)

    [Test]
    public async Task CheckIfLeaveTypeNameExistsForEdit_ReturnsFalse_ForSameRecord()
    {
        var leaveType = new LeaveType { Name = "Annual Leave", NumberOfDays = 20 };
        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();

        var model = new LeaveTypeEditVM { Id = leaveType.Id, Name = "Annual Leave", NumberOfDays = 20 };
        var result = await _service.CheckIfLeaveTypeNameExistsForEdit(model);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckIfLeaveTypeNameExistsForEdit_ReturnsTrue_WhenAnotherRecordHasSameName()
    {
        _context.LeaveTypes.AddRange(
            new LeaveType { Name = "Annual Leave", NumberOfDays = 20 },
            new LeaveType { Name = "Sick Leave", NumberOfDays = 10 }
        );
        await _context.SaveChangesAsync();

        var sickLeave = await _context.LeaveTypes.FirstAsync(x => x.Name == "Sick Leave");
        var model = new LeaveTypeEditVM { Id = sickLeave.Id, Name = "Annual Leave", NumberOfDays = 10 };

        var result = await _service.CheckIfLeaveTypeNameExistsForEdit(model);

        Assert.That(result, Is.True);
    }

    // =====================================================
    // DaysExceedMaximum
    // =====================================================

    [Test]
    public async Task DaysExceedMaximum_ReturnsTrue_WhenRequestedDaysExceedLimit()
    {
        var leaveType = new LeaveType { Name = "Annual", NumberOfDays = 20 };
        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();

        var result = await _service.DaysExceedMaximum(leaveType.Id, 25);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DaysExceedMaximum_ReturnsFalse_WhenRequestedDaysWithinLimit()
    {
        var leaveType = new LeaveType { Name = "Annual", NumberOfDays = 20 };
        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();

        var result = await _service.DaysExceedMaximum(leaveType.Id, 15);

        Assert.That(result, Is.False);
    }

    // =====================================================
    // LeaveTypeExists
    // =====================================================

    [Test]
    public async Task LeaveTypeExists_ReturnsTrue_WhenIdExists()
    {
        var leaveType = new LeaveType { Name = "Annual", NumberOfDays = 20 };
        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync();

        var result = _service.LeaveTypeExists(leaveType.Id);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task LeaveTypeExists_ReturnsFalse_WhenIdDoesNotExist()
    {
        var result = _service.LeaveTypeExists(9999);

        Assert.That(result, Is.False);
    }
}