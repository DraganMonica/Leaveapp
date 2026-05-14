using LeaveManagementSystem.Application.Models.Managers;
using LeaveManagementSystem.Application.Services.LeaveAllocations;
using LeaveManagementSystem.Application.Services.Managers;
using LeaveManagementSystem.Common.Static;
using LeaveManagementSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LeaveManagementSystem.Application.Tests;

[TestFixture]
public class ManagersServiceTests
{
    private ApplicationDbContext _context;
    private Mock<UserManager<ApplicationUser>> _userManagerMock;
    private Mock<ILeaveAllocationsService> _leaveAllocationsMock;
    private ManagersService _service;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _leaveAllocationsMock = new Mock<ILeaveAllocationsService>();

        _service = new ManagersService(
            _userManagerMock.Object,
            _leaveAllocationsMock.Object,
            _context,
            Mock.Of<ILogger<ManagersService>>());
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }




    //1) GetAllManagers
    //verifica daca metoda returneaza corect managerii, general managerul(daca metoda returneaza datele corecte deja mapate)
    [Test]
    public async Task GetAllManagers_ReturnsBothManagersAndGeneralManagers()
    {
        //arrange

        //creez managerii
        var managers = new List<ApplicationUser>
        {
            new() { Id = "manager1", FirstName = "Ion", LastName = "Pop", Email = "ion@test.com" }
        };
        var generalManagers = new List<ApplicationUser>
        {
            new() { Id = "gm", FirstName = "Maria", LastName = "Ion", Email = "maria@test.com" }
        };

        // cand service ul cere managerii, da mi ce e mai jos
        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.Manager))
            .ReturnsAsync(managers);
        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.GeneralManager))
            .ReturnsAsync(generalManagers);


        //act
        var result = await _service.GetAllManagers();


        //assert
        Assert.That(result.Managers.Count, Is.EqualTo(1));
        Assert.That(result.GeneralManagers.Count, Is.EqualTo(1));
        Assert.That(result.Managers[0].Email, Is.EqualTo("ion@test.com"));
    }



    //verifica daca metoda returneaza liste goale cand nu exista manageri
    [Test]
    public async Task GetAllManagers_ReturnsEmptyLists_WhenNoManagersExist()
    {
        //el nu are manageri sau generalmananger, se simuleaza liste goale pt rolurile de mai jos
        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.Manager))
            .ReturnsAsync(new List<ApplicationUser>());
        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.GeneralManager))
            .ReturnsAsync(new List<ApplicationUser>());


        //act
        var result = await _service.GetAllManagers();


        //assert
        Assert.That(result.Managers, Is.Empty);
        Assert.That(result.GeneralManagers, Is.Empty);
    }




    //2)GetManagerById
    //verifica daca metoda returneaza un ManagerVM corect cand user ul exista
    [Test]
    public async Task GetManagerById_ReturnsManagerVM_WhenUserExists()
    {
        //arrange
        var user = new ApplicationUser
        {
            Id = "m1",
            FirstName = "Ion",
            LastName = "Pop",
            Email = "ion@test.com"
        };

        _userManagerMock
            .Setup(m => m.FindByIdAsync("m1"))
            .ReturnsAsync(user);


        //act
        var result = await _service.GetManagerById("m1");


        //assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo("ion@test.com"));
        Assert.That(result.FirstName, Is.EqualTo("Ion"));
    }



    //verifica ca metoda returneaza null, daca user ul nu exista
    [Test]
    public async Task GetManagerById_ReturnsNull_WhenUserNotFound()
    {
        //lista este goala, simulez pe db ul care nu are manager
        _userManagerMock
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null);


        //act
        var result = await _service.GetManagerById("ghost-id");


        //assert
        Assert.That(result, Is.Null);
    }



    //3)CreateManager 
    //verifica ca dupa crearea maangerului se aloca concediul
    [Test]
    public async Task CreateManager_AllocatesLeave_AfterSuccessfulCreation()
    {
        //arrange
        //  user ul s a salvat corect si ii s au atribuit corect rolurile- ma intereseaza sa fie successfull in ambele situatii, nu valorile in sine
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(m => m.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(m => m.GetUserIdAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("manager-id");

        var model = new CreateManagerVM
        {
            FirstName = "Ion",
            LastName = "Pop",
            Email = "ion@test.com",
            Password = "Pass123!",
            DateOfBirth = new DateOnly(1990, 1, 1),
            DateOfEmployment = new DateOnly(2020, 1, 1)
        };


        //act
        await _service.CreateManager(model);


        //assert
        //verific ca AllocateLeave a fost apelat exact o data
        _leaveAllocationsMock.Verify(
            s => s.AllocateLeave("manager-id"),
            Times.Once);
    }




    [Test]
    public async Task CreateGeneralManager_DoesNotAllocateLeave()
    {
        // general managerul nu primeste alocare de concediu - nu am pus accentul pe el
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var model = new CreateManagerVM
        {
            FirstName = "Maria",
            LastName = "Ion",
            Email = "maria@test.com",
            Password = "Pass123!",
            DateOfBirth = new DateOnly(1985, 5, 10),
            DateOfEmployment = new DateOnly(2018, 3, 1)
        };

        await _service.CreateGeneralManager(model);

        // AllocateLeave NU trebuie apelat niciodata pentru GeneralManager
        _leaveAllocationsMock.Verify(
            s => s.AllocateLeave(It.IsAny<string>()),
            Times.Never);
    }




    //4)AssignManagerToGeneralManager
    [Test]
    // 
    public async Task AssignManagerToGeneralManager_DoesNotDuplicate_WhenAlreadyAssigned()
    {
        // prima asignare-manuala(m-1 apartine de gm-1)
        _context.GeneralManagerManagers.Add(new GeneralManagerManager
        {
            GeneralManagerId = "gm-1",
            ManagerId = "m-1"
        });
        await _context.SaveChangesAsync();

        // a doua asignare identica — trebuie ignorata
        await _service.AssignManagerToGeneralManager("gm-1", "m-1");

        var count = await _context.GeneralManagerManagers.CountAsync();
        Assert.That(count, Is.EqualTo(1));
    }



    [Test]
    public async Task AssignManagerToGeneralManager_DoesNothing_WhenIdsAreEmpty()
    {
        // validare parametri null/empty 
        await _service.AssignManagerToGeneralManager("", "m-1");
        await _service.AssignManagerToGeneralManager("gm-1", "");

        var count = await _context.GeneralManagerManagers.CountAsync();
        Assert.That(count, Is.EqualTo(0));
    }




    //5)RemoveManagerFromGeneralManager
    [Test]
    public async Task RemoveManagerFromGeneralManager_RemovesRelation_WhenExists()
    {
        //se adauga relatia dintr manager si generalmananger
        _context.GeneralManagerManagers.Add(new GeneralManagerManager
        {
            GeneralManagerId = "gm-1",
            ManagerId = "m-1"
        });
        await _context.SaveChangesAsync();

        //act
        await _service.RemoveManagerFromGeneralManager("gm-1", "m-1");


        //assert
        var count = await _context.GeneralManagerManagers.CountAsync();
        Assert.That(count, Is.EqualTo(0));
    }



    //6)GetMyManagers
    // gm-1 are 2 manageri
    // Trebuie sa returneze mnagerii lui gm-1
    [Test]
    public async Task GetMyManagers_ReturnsOnlyManagersForGivenGeneralManager()
    {
        //arrange
        var manager1 = new ApplicationUser { Id = "m-1", FirstName = "Ion", LastName = "Pop", Email = "m1@test.com" };
        var manager2 = new ApplicationUser { Id = "m-2", FirstName = "Ana", LastName = "Dan", Email = "m2@test.com" };
        

        _context.Users.AddRange(manager1, manager2);
        _context.GeneralManagerManagers.AddRange(
            new GeneralManagerManager { GeneralManagerId = "gm-1", ManagerId = "m-1", Manager = manager1 },
            new GeneralManagerManager { GeneralManagerId = "gm-1", ManagerId = "m-2", Manager = manager2 }
        );
        await _context.SaveChangesAsync();


        //act
        var result = await _service.GetMyManagers("gm-1");


        //assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(m => m.Id == "m-1" || m.Id == "m-2"), Is.True);
    }



    [Test]
    public async Task GetMyManagers_ReturnsEmpty_WhenNoManagersAssigned()
    {
        var result = await _service.GetMyManagers("gm-no-managers");

        Assert.That(result, Is.Empty);
    }




    //7)DeleteManager
    [Test]
    public async Task DeleteManager_RemovesRelations_AndDeletesUser()
    {
        // arrange
        var user = new ApplicationUser { Id = "m-1", Email = "m1@test.com" };

        _context.GeneralManagerManagers.Add(new GeneralManagerManager
        {
            GeneralManagerId = "gm-1",
            ManagerId = "m-1"
        });
        await _context.SaveChangesAsync();

        _userManagerMock
            .Setup(m => m.FindByIdAsync("m-1"))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // act
        await _service.DeleteManager("m-1");

        // assert
        var relationsCount = await _context.GeneralManagerManagers.CountAsync();
        Assert.That(relationsCount, Is.EqualTo(0));

        _userManagerMock.Verify(m => m.DeleteAsync(user), Times.Once);
    }


    //8)UpdateManager
    [Test]
    public async Task UpdateManager_UpdatesUser_WhenExists()
    {
        //arrange
        var user = new ApplicationUser
        {
            Id = "m-1",
            Email = "old@test.com",
            FirstName = "Old",
            LastName = "Name"
        };

        _userManagerMock
            .Setup(m => m.FindByIdAsync("m-1"))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var model = new EditManagerVM
        {
            Email = "new@test.com",
            FirstName = "New",
            LastName = "Name",
            DateOfBirth = new DateOnly(1990, 1, 1),
            DateOfEmployment = new DateOnly(2020, 1, 1)
        };


        //act
        await _service.UpdateManager("m-1", model);


        //assert
        Assert.That(user.Email, Is.EqualTo("new@test.com"));
        Assert.That(user.FirstName, Is.EqualTo("New"));

        _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
    }



    [Test]
    public async Task UpdateManager_DoesNothing_WhenUserNotFound()
    {
        _userManagerMock
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null);

        var model = new EditManagerVM
        {
            Email = "new@test.com",
            FirstName = "New",
            LastName = "Name",
            DateOfBirth = new DateOnly(1990, 1, 1),
            DateOfEmployment = new DateOnly(2020, 1, 1)
        };

        Assert.DoesNotThrowAsync(async () =>
            await _service.UpdateManager("ghost", model));
    }

}
