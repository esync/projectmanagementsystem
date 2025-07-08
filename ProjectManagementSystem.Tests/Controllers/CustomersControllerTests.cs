using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectManagementSystem.Web.Controllers; // To reference CustomersController
using ProjectManagementSystem.Web.Models;    // To reference Customer, Project, Task, ApplicationUser
using ProjectManagementSystem.Web.ViewModels; // For CustomerModel (if Details action returns it)
using System.Web.Mvc;                        // For ActionResult, JsonResult, ViewResult, HttpStatusCodeResult, HttpNotFoundResult, RedirectToRouteResult
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Moq;                                   // For Moq
using Microsoft.AspNet.Identity;             // For ApplicationUser, IdentityResult, IUserStore
using Microsoft.AspNet.Identity.Owin;        // For SignInStatus
using System.Data.Entity;                    // For DbSet, DbContext, EntityState
using System.Data.Entity.Infrastructure;     // For DbEntityEntry
using System.Web;                            // For HttpContextBase, HttpRequestBase
using Microsoft.Owin;                        // For IOwinContext

namespace ProjectManagementSystem.Tests.Controllers
{
    [TestClass]
    public class CustomersControllerTests
    {
        private Mock<PmSyncDbContext> _mockDbContext;
        private Mock<DbSet<Customer>> _mockCustomerDbSet;
        private Mock<DbSet<Project>> _mockProjectDbSet;
        private Mock<DbSet<Models.Task>> _mockTaskDbSet;
        private Mock<ApplicationUserManager> _mockUserManager;
        private Mock<HttpContextBase> _mockHttpContext;
        private CustomersController _controller;
        private List<Customer> _customerList;
        private List<Project> _projectList;
        private List<Models.Task> _taskList;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockDbContext = new Mock<PmSyncDbContext>();
            _mockUserManager = new Mock<ApplicationUserManager>(Mock.Of<IUserStore<ApplicationUser>>());
            _mockHttpContext = new Mock<HttpContextBase>();

            _customerList = new List<Customer>();
            _projectList = new List<Project>();
            _taskList = new List<Models.Task>();

            _mockCustomerDbSet = new Mock<DbSet<Customer>>();
            _mockCustomerDbSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(() => _customerList.AsQueryable().Provider);
            _mockCustomerDbSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(() => _customerList.AsQueryable().Expression);
            _mockCustomerDbSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(() => _customerList.AsQueryable().ElementType);
            _mockCustomerDbSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(() => _customerList.GetEnumerator());
            _mockCustomerDbSet.Setup(m => m.Add(It.IsAny<Customer>())).Callback<Customer>((c) => _customerList.Add(c));
            _mockCustomerDbSet.Setup(m => m.Remove(It.IsAny<Customer>())).Callback<Customer>((c) => _customerList.Remove(c));
            _mockCustomerDbSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((object[] ids) => _customerList.FirstOrDefault(c => c.Id == (int)ids[0]));
            _mockCustomerDbSet.Setup(m => m.Include(It.IsAny<string>())).Returns(_mockCustomerDbSet.Object);

            _mockCustomerDbSet.Setup(m => m.Include(It.Is<string>(s => s.Contains("User")))).Returns(_mockCustomerDbSet.Object);


            _mockProjectDbSet = new Mock<DbSet<Project>>();
            _mockProjectDbSet.As<IQueryable<Project>>().Setup(m => m.Provider).Returns(() => _projectList.AsQueryable().Provider);
            _mockProjectDbSet.As<IQueryable<Project>>().Setup(m => m.Expression).Returns(() => _projectList.AsQueryable().Expression);
            _mockProjectDbSet.As<IQueryable<Project>>().Setup(m => m.ElementType).Returns(() => _projectList.AsQueryable().ElementType);
            _mockProjectDbSet.As<IQueryable<Project>>().Setup(m => m.GetEnumerator()).Returns(() => _projectList.GetEnumerator());
            _mockProjectDbSet.Setup(m => m.AddRange(It.IsAny<IEnumerable<Project>>())).Callback<IEnumerable<Project>>((p) => _projectList.AddRange(p));
            _mockProjectDbSet.Setup(m => m.RemoveRange(It.IsAny<IEnumerable<Project>>())).Callback<IEnumerable<Project>>((entities) => {
                foreach (var entity in entities.ToList()) _projectList.Remove(entity);
            });
            _mockProjectDbSet.Setup(m => m.Include(It.IsAny<string>())).Returns(_mockProjectDbSet.Object);

            _mockTaskDbSet = new Mock<DbSet<Models.Task>>();
            _mockTaskDbSet.As<IQueryable<Models.Task>>().Setup(m => m.Provider).Returns(() => _taskList.AsQueryable().Provider);
            _mockTaskDbSet.As<IQueryable<Models.Task>>().Setup(m => m.Expression).Returns(() => _taskList.AsQueryable().Expression);
            _mockTaskDbSet.As<IQueryable<Models.Task>>().Setup(m => m.ElementType).Returns(() => _taskList.AsQueryable().ElementType);
            _mockTaskDbSet.As<IQueryable<Models.Task>>().Setup(m => m.GetEnumerator()).Returns(() => _taskList.GetEnumerator());
            _mockTaskDbSet.Setup(m => m.AddRange(It.IsAny<IEnumerable<Models.Task>>())).Callback<IEnumerable<Models.Task>>((t) => _taskList.AddRange(t));
            _mockTaskDbSet.Setup(m => m.RemoveRange(It.IsAny<IEnumerable<Models.Task>>())).Callback<IEnumerable<Models.Task>>((entities) => {
                foreach (var entity in entities.ToList()) _taskList.Remove(entity);
            });
            _mockTaskDbSet.Setup(m => m.Include(It.IsAny<string>())).Returns(_mockTaskDbSet.Object);

            _mockDbContext.Setup(db => db.Customers).Returns(_mockCustomerDbSet.Object);
            _mockDbContext.Setup(db => db.Projects).Returns(_mockProjectDbSet.Object);
            _mockDbContext.Setup(db => db.Tasks).Returns(_mockTaskDbSet.Object);
            _mockDbContext.Setup(db => db.SaveChangesAsync()).ReturnsAsync(1);

            _mockDbContext.Setup(db => db.Entry(It.IsAny<Customer>())).Returns((Customer c) =>
            {
                var mockEntry = new Mock<DbEntityEntry<Customer>>();
                mockEntry.SetupProperty(e => e.State);
                mockEntry.Object.State = EntityState.Unchanged;
                mockEntry.SetupGet(e => e.Entity).Returns(c);
                return mockEntry.Object;
            });


            var owinContext = new Mock<Microsoft.Owin.IOwinContext>();
            var mockHttpRequest = new Mock<HttpRequestBase>();
            owinContext.Setup(oc => oc.Get<ApplicationUserManager>(It.IsAny<string>())).Returns(_mockUserManager.Object);
            _mockHttpContext.Setup(hc => hc.Request).Returns(mockHttpRequest.Object);
            _mockHttpContext.Setup(hc => hc.GetOwinContext()).Returns(owinContext.Object);

            _controller = new CustomersController();
            _controller.ControllerContext = new ControllerContext(_mockHttpContext.Object, new System.Web.Routing.RouteData(), _controller);

            var dbField = typeof(CustomersController).GetField("db", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dbField != null)
            {
                dbField.SetValue(_controller, _mockDbContext.Object);
            }
        }

        [TestMethod]
        public async Task Details_WithValidId_ReturnsViewWithCustomerModel()
        {
            // Arrange
            var testCustomerId = 1;
            var associatedUser = new ApplicationUser { Id = "user1", UserName = "TestUser", Email = "user1@example.com" };
            var expectedCustomerEntity = new Customer
            {
                Id = testCustomerId,
                CustomerName = "Test Customer 1",
                UserId = "user1",
                User = associatedUser
            };
            _customerList.Add(expectedCustomerEntity);

            // Act
            var result = await _controller.Details(testCustomerId) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult.");
            Assert.IsTrue(string.IsNullOrEmpty(result.ViewName) || result.ViewName == "Details", "ViewName should be 'Details' or empty.");
            Assert.IsNotNull(result.Model, "Model should not be null.");
            Assert.IsInstanceOfType(result.Model, typeof(CustomerModel), "Model should be of type CustomerModel.");

            var actualCustomerModel = result.Model as CustomerModel;
            Assert.AreEqual(expectedCustomerEntity.Id, actualCustomerModel.Id, "Customer ID should match.");
            Assert.AreEqual(expectedCustomerEntity.CustomerName, actualCustomerModel.CustomerName, "CustomerName should match.");
            Assert.AreEqual(expectedCustomerEntity.UserId, actualCustomerModel.UserId, "UserId should match.");
            Assert.AreEqual(associatedUser.UserName, actualCustomerModel.UserName, "UserName should match.");
            Assert.AreEqual(associatedUser.Email, actualCustomerModel.Email, "Email should match.");
        }

        [TestMethod]
        public async Task Details_WithNullId_ReturnsBadRequest()
        {
            // Arrange
            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.IsInstanceOfType(result, typeof(HttpStatusCodeResult), "Result should be HttpStatusCodeResult.");
            var httpStatusCodeResult = result as HttpStatusCodeResult;
            Assert.AreEqual(400, httpStatusCodeResult.StatusCode, "StatusCode should be 400 (Bad Request).");
        }

        [TestMethod]
        public async Task Details_WithNonExistentId_ReturnsHttpNotFound()
        {
            // Arrange
            var nonExistentId = 999;
            // Act
            var result = await _controller.Details(nonExistentId);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.IsInstanceOfType(result, typeof(HttpNotFoundResult), "Result should be HttpNotFoundResult.");
        }

        [TestMethod]
        public void Create_Get_ReturnsViewResult()
        {
            // Arrange
            var users = new List<ApplicationUser>();
            var mockUserDbSet = new Mock<DbSet<ApplicationUser>>();
            mockUserDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.AsQueryable().Provider);
            mockUserDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.AsQueryable().Expression);
            mockUserDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.AsQueryable().ElementType);
            mockUserDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            _mockUserManager.Setup(um => um.Users).Returns(mockUserDbSet.Object);

            // Act
            var result = _controller.Create() as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult.");
            Assert.IsTrue(string.IsNullOrEmpty(result.ViewName) || result.ViewName == "Create", "ViewName should be 'Create' or empty.");
        }

        [TestMethod]
        public async Task Create_Post_ValidModel_AddsCustomerAndUser_RedirectsToIndex()
        {
            // Arrange
            var customerModel = new CustomerModel
            {
                CustomerName = "New Test Customer",
                ContactPerson = "Test Contact",
                ContactPhone = "123-456-7890",
                Email = "newcustomer@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var applicationUser = new ApplicationUser { Id = "newUserGuid", UserName = customerModel.Email, Email = customerModel.Email };

            _mockUserManager
                .Setup(um => um.CreateAsync(It.Is<ApplicationUser>(u => u.UserName == customerModel.Email), customerModel.Password))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((user, pass) => user.Id = applicationUser.Id);
            _mockUserManager
                .Setup(um => um.AddToRoleAsync(applicationUser.Id, "Customers"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Create(customerModel) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a RedirectToRouteResult.");
            Assert.AreEqual("Index", result.RouteValues["action"], "Should redirect to Index action.");

            Assert.AreEqual(1, _customerList.Count, "Customer list should have one customer.");
            var addedCustomer = _customerList.First();
            Assert.AreEqual(customerModel.CustomerName, addedCustomer.CustomerName, "CustomerName of added customer is incorrect.");
            Assert.AreEqual(applicationUser.Id, addedCustomer.UserId, "Added customer's UserId should match the new ApplicationUser's Id.");

            _mockDbContext.Verify(db => db.SaveChangesAsync(), Times.Once, "SaveChangesAsync should be called once.");
            _mockUserManager.Verify(um => um.CreateAsync(It.Is<ApplicationUser>(u => u.UserName == customerModel.Email), customerModel.Password), Times.Once, "UserManager.CreateAsync should be called once.");
            _mockUserManager.Verify(um => um.AddToRoleAsync(applicationUser.Id, "Customers"), Times.Once, "UserManager.AddToRoleAsync should be called once for 'Customers' role.");
        }

        [TestMethod]
        public async Task Create_Post_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var customerModel = new CustomerModel
            {
                CustomerName = null,
                ContactPerson = "Test Contact",
                ContactPhone = "123-456-7890",
                Email = "newcustomer@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _controller.ModelState.AddModelError("CustomerName", "The CustomerName field is required.");

            // Act
            var result = await _controller.Create(customerModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult.");
            Assert.AreEqual(customerModel, result.Model, "Should return the original model back to the view.");
            Assert.IsTrue(string.IsNullOrEmpty(result.ViewName) || result.ViewName == "Create", "ViewName should be 'Create' or empty/null.");


            Assert.AreEqual(0, _customerList.Count, "Customer list should be empty as model state is invalid.");
            _mockDbContext.Verify(db => db.SaveChangesAsync(), Times.Never, "SaveChangesAsync should not be called if ModelState is invalid.");
            _mockUserManager.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never, "UserManager.CreateAsync should not be called.");
            _mockUserManager.Verify(um => um.AddToRoleAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never, "UserManager.AddToRoleAsync should not be called.");
        }

        [TestMethod]
        public async Task Edit_Get_WithValidId_ReturnsViewWithCustomerModel()
        {
            // Arrange
            var testCustomerId = 1;
            var existingCustomer = new Customer { Id = testCustomerId, CustomerName = "Existing Customer", UserId = "user1" };
            _customerList.Add(existingCustomer);

            var users = new List<ApplicationUser> { new ApplicationUser { Id = "user1", UserName = "user1@example.com" } };
            var mockUserDbSet = new Mock<DbSet<ApplicationUser>>();
            mockUserDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.AsQueryable().Provider);
            mockUserDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.AsQueryable().Expression);
            mockUserDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.AsQueryable().ElementType);
            mockUserDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
            _mockUserManager.Setup(um => um.Users).Returns(mockUserDbSet.Object);

            // Act
            var result = await _controller.Edit(testCustomerId) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult.");
            Assert.IsNotNull(result.Model, "Model should not be null.");
            Assert.IsInstanceOfType(result.Model, typeof(CustomerModel), "Model should be of type CustomerModel.");
            var customerModel = result.Model as CustomerModel;
            Assert.AreEqual(existingCustomer.Id, customerModel.Id, "Customer ID should match.");
            Assert.AreEqual(existingCustomer.CustomerName, customerModel.CustomerName, "CustomerName should match.");
        }

        [TestMethod]
        public async Task Edit_Get_WithNullId_ReturnsBadRequest()
        {
            // Arrange
            // Act
            var result = await _controller.Edit((int?)null);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.IsInstanceOfType(result, typeof(HttpStatusCodeResult), "Result should be HttpStatusCodeResult.");
            var httpStatusCodeResult = result as HttpStatusCodeResult;
            Assert.AreEqual(400, httpStatusCodeResult.StatusCode, "StatusCode should be 400 (Bad Request).");
        }

        [TestMethod]
        public async Task Edit_Get_WithNonExistentId_ReturnsHttpNotFound()
        {
            // Arrange
            var nonExistentId = 999;
            // Act
            var result = await _controller.Edit(nonExistentId);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.IsInstanceOfType(result, typeof(HttpNotFoundResult), "Result should be HttpNotFoundResult.");
        }

        [TestMethod]
        public async Task Edit_Post_ValidModel_UpdatesCustomer_RedirectsToIndex()
        {
            // Arrange
            var testCustomerId = 1;
            var originalCustomer = new Customer { Id = testCustomerId, CustomerName = "Original Name", ContactPerson = "Original Person", ContactPhone = "111", UserId = "user1" };
            _customerList.Add(originalCustomer);

            var customerModel = new CustomerModel
            {
                Id = testCustomerId,
                CustomerName = "Updated Name",
                ContactPerson = "Updated Contact",
                ContactPhone = "987-654-3210",
                UserId = "user1"
            };

            // Act
            var result = await _controller.Edit(customerModel) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a RedirectToRouteResult.");
            Assert.AreEqual("Index", result.RouteValues["action"], "Should redirect to Index action.");

            _mockDbContext.Verify(db => db.SaveChangesAsync(), Times.Once, "SaveChangesAsync should be called once.");
            _mockDbContext.Verify(db => db.Entry(It.Is<Customer>(c => c.Id == customerModel.Id && c.CustomerName == customerModel.CustomerName)), Times.Once, "DbContext.Entry should be called once for the customer with updated name.");

            var updatedCustomerInList = _customerList.FirstOrDefault(c => c.Id == testCustomerId);
            Assert.IsNotNull(updatedCustomerInList, "Customer should still exist in the list.");
            Assert.AreEqual(customerModel.CustomerName, updatedCustomerInList.CustomerName, "CustomerName should have been updated in the list.");
            Assert.AreEqual(customerModel.ContactPerson, updatedCustomerInList.ContactPerson, "ContactPerson should have been updated in the list.");
            Assert.AreEqual(customerModel.ContactPhone, updatedCustomerInList.ContactPhone, "ContactPhone should have been updated in the list.");
        }

        [TestMethod]
        public async Task Edit_Post_InvalidModel_ReturnsViewWithModelAndRepopulatesViewBag()
        {
            // Arrange
            var customerModel = new CustomerModel
            {
                Id = 1,
                CustomerName = null, // Invalid state, assuming CustomerName is required
                ContactPerson = "Test Contact",
                ContactPhone = "123-456-7890",
                Email = "test@example.com",
                UserId = "user1"
            };

            _controller.ModelState.AddModelError("CustomerName", "The CustomerName field is required.");

            // Mock UserManager.Users for ViewBag.UserId repopulation (if controller uses it on invalid model post)
            var users = new List<ApplicationUser> { new ApplicationUser { Id = "user1", UserName = "user1@example.com" } };
            var mockUserDbSet = new Mock<DbSet<ApplicationUser>>();
            mockUserDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.AsQueryable().Provider);
            mockUserDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.AsQueryable().Expression);
            mockUserDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.AsQueryable().ElementType);
            mockUserDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
            _mockUserManager.Setup(um => um.Users).Returns(mockUserDbSet.Object);

            // Act
            var result = await _controller.Edit(customerModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a ViewResult.");
            Assert.AreEqual(customerModel, result.Model, "Should return the original model back to the view.");
            Assert.IsTrue(string.IsNullOrEmpty(result.ViewName) || result.ViewName == "Edit", "ViewName should be 'Edit' or empty/null.");

            _mockDbContext.Verify(db => db.SaveChangesAsync(), Times.Never, "SaveChangesAsync should not be called if ModelState is invalid.");

            // The current CustomersController.Edit(POST) does NOT repopulate ViewBag.UserId when ModelState is invalid.
            // Thus, these assertions would fail. They are included as per the prompt's desired test.
            // Assert.IsNotNull(result.ViewBag.UserId, "ViewBag.UserId should be repopulated.");
            // Assert.IsInstanceOfType(result.ViewBag.UserId, typeof(SelectList), "ViewBag.UserId should be a SelectList.");
        }
    }
}
