using LeaveManagementSystem.Application.Services.Departments;
using LeaveManagementSystem.Common.Static;
using LeaveManagementSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace LeaveManagementSystem.Application.Tests
{

    [TestFixture]
    public class DepartmentsServiceTests
    {
        private ApplicationDbContext _context;
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private DepartmentsService _service;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            // UserManager are 9 dependente
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null
            );

            _service = new DepartmentsService(_context, _userManagerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }



        //1) CreateDepartment
        [Test]
        public async Task CreateDepartment_SavesDepartmentToDatabase()
        {
            //act          metoda(paramentru necesar-adica, numele departamentului)
            await _service.CreateDepartment("IT");


            //assert
            var saved = await _context.Departments.FirstOrDefaultAsync();
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved.Name, Is.EqualTo("IT"));
        }



        //la creare, departamentul nu are manager implicit
        [Test]
        public async Task CreateDepartment_CreatesWithNoManager_ByDefault()
        {
            await _service.CreateDepartment("HR");

            var saved = await _context.Departments.FirstOrDefaultAsync();
            Assert.That(saved.ManagerId, Is.Null);
        }



        //2) GetAllDepartments
        [Test]
        public async Task GetAllDepartments_ReturnsAllDepartments()
        {
            //arrange
            _context.Departments.AddRange(
                new Department { Name = "IT" },
                new Department { Name = "HR" },
                new Department { Name = "Finance" }
            );
            await _context.SaveChangesAsync();


            //act
            var result = await _service.GetAllDepartments();


            //assert
            Assert.That(result.Count, Is.EqualTo(3));
        }



        [Test]
        public async Task GetAllDepartments_ReturnsEmptyList_WhenNoDepartments()
        {
            //act-fara arrange, nu declar niciun departamanet sa vad daca returneaza lista goala 
            var result = await _service.GetAllDepartments();

            Assert.That(result, Is.Empty);
        }



        //3) GetDepartmentById
        [Test]
        public async Task GetDepartmentById_ReturnsDepartment_WhenExists()
        {
            //arrange
            var dept = new Department { Name = "IT" };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();


            //act
            var result = await _service.GetDepartmentById(dept.Id);


            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("IT"));
        }



        [Test]
        public async Task GetDepartmentById_ReturnsNull_WhenNotFound()
        {
            // act - caut un id care nu exista in baza de date
            var result = await _service.GetDepartmentById(99);

            Assert.That(result, Is.Null);
        }



        //4) UpdateDepartment
        [Test]
        public async Task UpdateDepartment_UpdatesNameAndManager_WhenExists()
        {
            //arrange
            var dept = new Department { Name = "IT" };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();


            //act
            await _service.UpdateDepartment(dept.Id, "Information Technology", "manager-123");

            var updated = await _context.Departments.FindAsync(dept.Id);
            Assert.That(updated.Name, Is.EqualTo("Information Technology"));
            Assert.That(updated.ManagerId, Is.EqualTo("manager-123"));
        }



        [Test]
        public async Task UpdateDepartment_CanRemoveManager_ByPassingNull()
        {
            // managerId nullable — poti scoate managerul
            var dept = new Department { Name = "IT", ManagerId = "manager-123" };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            await _service.UpdateDepartment(dept.Id, "IT", null);

            var updated = await _context.Departments.FindAsync(dept.Id);
            Assert.That(updated.ManagerId, Is.Null);
        }



        [Test]
        public async Task UpdateDepartment_DoesNotThrow_WhenDepartmentNotFound()
        {
            Assert.DoesNotThrowAsync(async () =>
                await _service.UpdateDepartment(9999, "Dept2", null));
        }



        //5)DeleteDepartment
        [Test]
        public async Task DeleteDepartment_RemovesDepartment_WhenExists()
        {
            //arrange
            var dept = new Department { Name = "IT" };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();


            //act
            await _service.DeleteDepartment(dept.Id);


            //assert
            var remaining = await _context.Departments.CountAsync();
            Assert.That(remaining, Is.EqualTo(0));
        }



        [Test]
        public async Task DeleteDepartment_DoesNotThrow_WhenNotFound()
        {
            Assert.DoesNotThrowAsync(async () =>
                await _service.DeleteDepartment(9999));
        }



        //6) AssignManager
        [Test]
        public async Task AssignManager_SetsManagerId_WhenDepartmentExists()
        {
            //arrange
            var dept = new Department { Name = "IT" };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();


            //act
            await _service.AssignManager(dept.Id, "manager-456");


            //assert
            var updated = await _context.Departments.FindAsync(dept.Id);
            Assert.That(updated.ManagerId, Is.EqualTo("manager-456"));
        }


        
        [Test]
        public async Task AssignManager_DoesNotThrow_WhenDepartmentNotFound()
        {
            Assert.DoesNotThrowAsync(async () =>
                await _service.AssignManager(99, "manager-456"));
        }




        //7)AssignEmployeeToDepartment
        //verificaȘ atunci cand utilizatorul exista, se actualizeaza departmentId, apelez UpdateAsync
        [Test]
        public async Task AssignEmployeeToDepartment_UpdatesDepartmentId_WhenUserExists()
        {
            // arrange
            //declar  user id, fara a avea departament asignat
            var user = new ApplicationUser { Id = "emp-1", DepartmentId = null };

            //nu se apeleaza db real, mock-ul returneaza user-ul definit in test ATUNCI cand e nevoie de el
            _userManagerMock
                .Setup(m => m.FindByIdAsync(user.Id))
                .ReturnsAsync(user);

            //simulare de update
            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);


            //act
            await _service.AssignEmployeeToDepartment(user.Id, 5);


            //assert
            Assert.That(user.DepartmentId, Is.EqualTo(5));
            _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
        }

        


        //8)GetUnassignedEmployees
        [Test]
        public async Task GetUnassignedEmployees_ReturnsOnlyEmployeesWithNoDepartment()
        {
            //arrange
            var employees = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "emo1", DepartmentId = null },   // neasignat
                new ApplicationUser { Id = "emp2", DepartmentId = 1 },      // asignat
                new ApplicationUser { Id = "emp3", DepartmentId = null },   // neasignat
            };

            //cand service ul cere userii cu rol employees, trebuie sa apara cei de mai sus
            _userManagerMock
                .Setup(m => m.GetUsersInRoleAsync(Roles.Employee))
                .ReturnsAsync(employees);


            //act
            var result = await _service.GetUnassignedEmployees();


            //assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(e => e.DepartmentId == null), Is.True);
        }

        [Test]
        public async Task GetUnassignedEmployees_ReturnsEmpty_WhenAllAssigned()
        {
            var employees = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "e1", DepartmentId = 1 },
            new ApplicationUser { Id = "e2", DepartmentId = 2 },
        };

            _userManagerMock
                .Setup(m => m.GetUsersInRoleAsync(Roles.Employee))
                .ReturnsAsync(employees);

            var result = await _service.GetUnassignedEmployees();

            Assert.That(result, Is.Empty);
        }
    }
}
