using LeaveManagementSystem.Application.Services;
using LeaveManagementSystem.Common.Static;
using LeaveManagementSystem.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace LeaveManagementSystem.Application.Tests;

[TestFixture]
public class UserServiceTests
{
    private Mock<UserManager<ApplicationUser>> _userManagerMock;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private UserService _service;

    // userul logat care se afla in majoritatea testelor
    private readonly ApplicationUser _loggedInUser = new()
    {
        Id = "current-user",
        Email = "current@test.com",
        FirstName = "Ion",
        LastName = "Pop",
        DepartmentId = 1
    };


    //simulez un user logat si mediul web (HttpContext) a.i  testele sa ruleze fara ASP.NET real.
    [SetUp]
    public void SetUp()
    {
        
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        // simuleaza un HttpContext activ cu un ClaimsPrincipal fals
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _loggedInUser.Id)
        }, "mock"));

        var httpContextMock = new Mock<HttpContext>();
        //service ul meu va primi acest context, unde am si user ul
        httpContextMock.Setup(c => c.User).Returns(claimsPrincipal);

        //simulez request ul 
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        // GetUserAsync returneaza userul logat implicit
        _userManagerMock
            .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(_loggedInUser);

        _service = new UserService(_userManagerMock.Object, _httpContextAccessorMock.Object);
    }




    //1)GetLoggedInUser
    [Test]
    public async Task GetLoggedInUser_ReturnsCurrentUser_WhenAuthenticated()
    {
        var result = await _service.GetLoggedInUser();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("current-user"));
    }




    [Test]
    public async Task GetLoggedInUser_ReturnsNull_WhenHttpContextIsNull()
    {
        // HttpContext null; test fara request activ
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext)null);
        _userManagerMock
            .Setup(m => m.GetUserAsync(null))
            .ReturnsAsync((ApplicationUser)null);

        var result = await _service.GetLoggedInUser();

        Assert.That(result, Is.Null);
    }




    //2)GetUserById
    [Test]
    public async Task GetUserById_ReturnsUser_WhenExists()
    {
        var user = new ApplicationUser { Id = "user-42", Email = "user42@test.com" };

        _userManagerMock
            .Setup(m => m.FindByIdAsync("user-42"))
            .ReturnsAsync(user);

        var result = await _service.GetUserById("user-42");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo("user42@test.com"));
    }




    [Test]
    public async Task GetUserById_ReturnsNull_WhenNotFound()
    {
        _userManagerMock
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null);

        var result = await _service.GetUserById("ghost-id");

        Assert.That(result, Is.Null);
    }



    //3)GetEmployees — Manager vede doar dept lui
    [Test]
    public async Task GetEmployees_AsManager_ReturnsOnlyEmployeesFromSameDepartment()
    {
        // Manager logat in dept 1
        // emp1 — dept 1, rol Employee => trebuie returnat
        // emp2 — dept 2, rol Employee => exclus (alt departament)
        var emp1 = new ApplicationUser { Id = "emp-1", DepartmentId = 1 };
        var emp2 = new ApplicationUser { Id = "emp-2", DepartmentId = 2 };

        _userManagerMock
            .Setup(m => m.GetRolesAsync(_loggedInUser))
            .ReturnsAsync(new List<string> { Roles.Manager });

        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.Employee))
            .ReturnsAsync(new List<ApplicationUser> { emp1, emp2 });

        // emp1 si emp2 nu sunt Manager sau GeneralManager
        _userManagerMock
            .Setup(m => m.IsInRoleAsync(It.IsAny<ApplicationUser>(), Roles.Manager))
            .ReturnsAsync(false);
        _userManagerMock
            .Setup(m => m.IsInRoleAsync(It.IsAny<ApplicationUser>(), Roles.GeneralManager))
            .ReturnsAsync(false);

        var result = await _service.GetEmployees();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo("emp-1"));
    }




    [Test]
    public async Task GetEmployees_AsManager_ExcludesHimself()
    {
        // Managerul logat are si rol Employee — nu trebuie sa apara in lista lui
        var selfAsEmployee = new ApplicationUser
        {
            Id = "current-user", // acelasi Id ca userul logat
            DepartmentId = 1
        };

        _userManagerMock
            .Setup(m => m.GetRolesAsync(_loggedInUser))
            .ReturnsAsync(new List<string> { Roles.Manager });

        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.Employee))
            .ReturnsAsync(new List<ApplicationUser> { selfAsEmployee });

        _userManagerMock
            .Setup(m => m.IsInRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var result = await _service.GetEmployees();

        Assert.That(result, Is.Empty);
    }




    [Test]
    public async Task GetEmployees_AsManager_ExcludesUsersWithManagerRole()
    {
        // Un user din acelasi departament dar cu rol Manager => exclus
        var otherManager = new ApplicationUser { Id = "mgr-2", DepartmentId = 1 };

        _userManagerMock
            .Setup(m => m.GetRolesAsync(_loggedInUser))
            .ReturnsAsync(new List<string> { Roles.Manager });

        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.Employee))
            .ReturnsAsync(new List<ApplicationUser> { otherManager });

        // otherManager ARE rol Manager => trebuie exclus
        _userManagerMock
            .Setup(m => m.IsInRoleAsync(otherManager, Roles.Manager))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(m => m.IsInRoleAsync(otherManager, Roles.GeneralManager))
            .ReturnsAsync(false);

        var result = await _service.GetEmployees();

        Assert.That(result, Is.Empty);
    }



    //4)GetEmployees — GeneralManager vede doar managerii
    [Test]
    public async Task GetEmployees_AsGeneralManager_ReturnsOnlyManagers()
    {
        var mgr1 = new ApplicationUser { Id = "mgr-1" };
        var mgr2 = new ApplicationUser { Id = "mgr-2" };

        _userManagerMock
            .Setup(m => m.GetRolesAsync(_loggedInUser))
            .ReturnsAsync(new List<string> { Roles.GeneralManager });

        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.Manager))
            .ReturnsAsync(new List<ApplicationUser> { mgr1, mgr2 });

        var result = await _service.GetEmployees();

        Assert.That(result.Count, Is.EqualTo(2));
    }



    [Test]
    public async Task GetEmployees_AsGeneralManager_ExcludesHimself()
    {
        // Daca GeneralManagerul are si rol Manager, nu apare in lista lui
        var selfAsManager = new ApplicationUser { Id = "current-user" };

        _userManagerMock
            .Setup(m => m.GetRolesAsync(_loggedInUser))
            .ReturnsAsync(new List<string> { Roles.GeneralManager });

        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.Manager))
            .ReturnsAsync(new List<ApplicationUser> { selfAsManager });

        var result = await _service.GetEmployees();

        Assert.That(result, Is.Empty);
    }



    //5)GetEmployees — Administrator vede tot, deduplicat
    [Test]
    public async Task GetEmployees_AsAdmin_ReturnsAllUsersDistinct()
    {
        // Un user poate fi in mai multe roluri — DistinctBy previne duplicate
        var sharedUser = new ApplicationUser { Id = "shared-1" }; // e si Employee si Manager
        var pureEmployee = new ApplicationUser { Id = "emp-1" };
        var generalMgr = new ApplicationUser { Id = "gm-1" };

        _userManagerMock
            .Setup(m => m.GetRolesAsync(_loggedInUser))
            .ReturnsAsync(new List<string> { Roles.Administrator });

        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.Employee))
            .ReturnsAsync(new List<ApplicationUser> { pureEmployee, sharedUser });

        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.Manager))
            .ReturnsAsync(new List<ApplicationUser> { sharedUser }); // apare din nou

        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.GeneralManager))
            .ReturnsAsync(new List<ApplicationUser> { generalMgr });

        var result = await _service.GetEmployees();

        // sharedUser apare in Employee si Manager — DistinctBy trebuie sa il pastreze o singura data
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.Select(u => u.Id).Distinct().Count(), Is.EqualTo(3));
    }




    [Test]
    public async Task GetEmployees_AsAdmin_ExcludesCurrentUser()
    {
        var adminAsEmployee = new ApplicationUser { Id = "current-user" };
        var otherEmployee = new ApplicationUser { Id = "emp-1" };

        _userManagerMock
            .Setup(m => m.GetRolesAsync(_loggedInUser))
            .ReturnsAsync(new List<string> { Roles.Administrator });

        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.Employee))
            .ReturnsAsync(new List<ApplicationUser> { adminAsEmployee, otherEmployee });

        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.Manager))
            .ReturnsAsync(new List<ApplicationUser>());

        _userManagerMock
            .Setup(m => m.GetUsersInRoleAsync(Roles.GeneralManager))
            .ReturnsAsync(new List<ApplicationUser>());

        var result = await _service.GetEmployees();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo("emp-1"));
    }




    //6)GetEmployees — rol necunoscut
    [Test]
    public async Task GetEmployees_ReturnsEmptyList_WhenUserHasNoRelevantRole()
    {
        // Un user fara rol de Manager/GeneralManager/Admin
        _userManagerMock
            .Setup(m => m.GetRolesAsync(_loggedInUser))
            .ReturnsAsync(new List<string> { Roles.Employee });

        var result = await _service.GetEmployees();

        Assert.That(result, Is.Empty);
    }
}
