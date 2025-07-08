using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectManagementSystem.Web.Controllers; // To reference AuthController
using ProjectManagementSystem.Web.Models;    // To reference LoginViewModel, etc.
using System.Web.Mvc;                        // For ActionResult, JsonResult
using System.Threading.Tasks;
using Moq;                                   // For Moq
using Microsoft.AspNet.Identity.Owin;        // For SignInStatus
using Microsoft.AspNet.Identity;             // For ApplicationUser, IUserStore
using System.Security.Claims;
using System.Collections.Generic;
using System.Web;                            // For HttpContextBase, HttpRequestBase
using System.Configuration;                  // For ConfigurationManager (if reading directly)
using Microsoft.Owin;                        // For IOwinContext
using System.Collections.Specialized;        // For NameValueCollection
using System.Linq;                           // For LINQ extension methods like .Any()

namespace ProjectManagementSystem.Tests.Controllers
{
    [TestClass]
    public class AuthControllerTests
    {
        private Mock<ApplicationUserManager> _mockUserManager;
        private Mock<ApplicationSignInManager> _mockSignInManager;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockHttpRequest; // For OwinContext
        private AuthController _authController;
        private NameValueCollection _appSettings; // For faking ConfigurationManager

        [TestInitialize]
        public void TestInitialize()
        {
            _mockUserManager = new Mock<ApplicationUserManager>(Mock.Of<IUserStore<ApplicationUser>>());
            _mockSignInManager = new Mock<ApplicationSignInManager>(_mockUserManager.Object, Mock.Of<Microsoft.Owin.Security.IAuthenticationManager>());
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockHttpRequest = new Mock<HttpRequestBase>();

            // Setup OwinContext for GetUserManager and GetSignInManager
            var owinContext = new Mock<IOwinContext>();
            owinContext.Setup(oc => oc.Get<ApplicationUserManager>(It.IsAny<string>())).Returns(_mockUserManager.Object);
            owinContext.Setup(oc => oc.Get<ApplicationSignInManager>(It.IsAny<string>())).Returns(_mockSignInManager.Object);

            _mockHttpContext.Setup(hc => hc.Request).Returns(_mockHttpRequest.Object);
            _mockHttpContext.Setup(hc => hc.GetOwinContext()).Returns(owinContext.Object);

            // These settings are for the LoginSpa_WithValidCredentials_ReturnsSuccessAndToken test.
            // For the missing config test, we'd ideally want ConfigurationManager to return null for these.
            _appSettings = new NameValueCollection
            {
                { "JwtSecretKey", "TestSecretKey123456789012345678901234567890" },
                { "JwtIssuer", "TestIssuer" },
                { "JwtAudience", "TestAudience" }
            };

            _authController = new AuthController(_mockUserManager.Object, _mockSignInManager.Object);
            _authController.ControllerContext = new ControllerContext(_mockHttpContext.Object, new System.Web.Routing.RouteData(), _authController);
        }

        [TestMethod]
        public async Task LoginSpa_WithValidCredentials_ReturnsSuccessAndToken()
        {
            // Arrange
            var loginViewModel = new LoginViewModel { Email = "test@example.com", Password = "Password123", RememberMe = false };
            var applicationUser = new ApplicationUser { Id = "testUserId", UserName = "test@example.com", Email = "test@example.com" };

            _mockSignInManager
                .Setup(m => m.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, loginViewModel.RememberMe, false))
                .ReturnsAsync(SignInStatus.Success);
            _mockUserManager
                .Setup(m => m.FindByNameAsync(loginViewModel.Email))
                .ReturnsAsync(applicationUser);

            // Simulate that ConfigurationManager has the required settings for this specific test
            // This still doesn't change the static ConfigurationManager, but makes the intent clear for this test.
            // A better way is to inject IConfiguration or equivalent into the controller.
            ConfigurationManager.AppSettings["JwtSecretKey"] = "TestSecretKey123456789012345678901234567890";
            ConfigurationManager.AppSettings["JwtIssuer"] = "TestIssuer";
            ConfigurationManager.AppSettings["JwtAudience"] = "TestAudience";


            // Act
            var result = await _authController.LoginSpa(loginViewModel) as JsonResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a JsonResult.");
            Assert.IsNotNull(result.Data, "JsonResult.Data should not be null.");

            object dataObject = result.Data;
            var successProperty = dataObject.GetType().GetProperty("success");
            Assert.IsNotNull(successProperty, "'success' property not found.");
            Assert.AreEqual(true, (bool)successProperty.GetValue(dataObject), "Login should be successful.");

            var messageProperty = dataObject.GetType().GetProperty("message");
            Assert.IsNotNull(messageProperty, "'message' property not found for success details.");

            var tokenProperty = dataObject.GetType().GetProperty("token");
            Assert.IsNotNull(tokenProperty, "'token' property not found.");
            string token = tokenProperty.GetValue(dataObject) as string;
            Assert.IsFalse(string.IsNullOrEmpty(token), "Token should not be null or empty.");

            var userIdProperty = dataObject.GetType().GetProperty("userId");
            Assert.IsNotNull(userIdProperty, "'userId' property not found.");
            Assert.AreEqual(applicationUser.Id, (string)userIdProperty.GetValue(dataObject), "UserId should match.");

            var userNameProperty = dataObject.GetType().GetProperty("userName");
            Assert.IsNotNull(userNameProperty, "'userName' property not found.");
            Assert.AreEqual(applicationUser.UserName, (string)userNameProperty.GetValue(dataObject), "UserName should match.");
        }

        [TestMethod]
        public async Task LoginSpa_WithInvalidCredentials_ReturnsFailure()
        {
            // Arrange
            var loginViewModel = new LoginViewModel { Email = "test@example.com", Password = "WrongPassword" };

            _mockSignInManager
                .Setup(m => m.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, false, false))
                .ReturnsAsync(SignInStatus.Failure);

            // Act
            var result = await _authController.LoginSpa(loginViewModel) as JsonResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a JsonResult.");
            Assert.IsNotNull(result.Data, "JsonResult.Data should not be null.");

            dynamic data = result.Data;

            var successProperty = data.GetType().GetProperty("success");
            Assert.IsNotNull(successProperty, "'success' property not found.");
            Assert.AreEqual(false, (bool)successProperty.GetValue(data), "Login should not be successful.");

            var messageProperty = data.GetType().GetProperty("message");
            Assert.IsNotNull(messageProperty, "'message' property not found.");
            string message = messageProperty.GetValue(data) as string;
            Assert.AreEqual("Invalid login attempt.", message, "Error message should indicate invalid login.");
        }

        [TestMethod]
        public async Task LoginSpa_WithInvalidModelState_ReturnsError()
        {
            // Arrange
            var loginViewModel = new LoginViewModel { Email = "", Password = "TestPassword" };
            _authController.ModelState.AddModelError("Email", "The Email field is required.");

            // Act
            var result = await _authController.LoginSpa(loginViewModel) as JsonResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a JsonResult.");
            Assert.IsNotNull(result.Data, "JsonResult.Data should not be null.");

            dynamic data = result.Data;

            var successProperty = data.GetType().GetProperty("success");
            Assert.IsNotNull(successProperty, "'success' property not found.");
            Assert.AreEqual(false, (bool)successProperty.GetValue(data), "Request should not be successful due to invalid ModelState.");

            var errorsProperty = data.GetType().GetProperty("errors");
            Assert.IsNotNull(errorsProperty, "'errors' property not found.");

            var errorsList = errorsProperty.GetValue(data) as IEnumerable<string>;
            if (errorsList == null) {
                var errorsEnumerable = errorsProperty.GetValue(data) as System.Collections.IEnumerable;
                if (errorsEnumerable != null) errorsList = errorsEnumerable.Cast<string>();
            }

            Assert.IsNotNull(errorsList, "Errors list should not be null.");
            Assert.IsTrue(errorsList.Any(e => e.Contains("The Email field is required.")), "Error message for Email field should be present.");
        }

        [TestMethod]
        public async Task LoginSpa_WithMissingJwtConfig_ReturnsErrorOnSuccessfulLogin()
        {
            // Arrange
            var loginViewModel = new LoginViewModel { Email = "test@example.com", Password = "Password123" };
            var applicationUser = new ApplicationUser { Id = "testUserId", UserName = "test@example.com", Email = "test@example.com" };

            _mockSignInManager
                .Setup(m => m.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, false, false))
                .ReturnsAsync(SignInStatus.Success);
            _mockUserManager
                .Setup(m => m.FindByNameAsync(loginViewModel.Email))
                .ReturnsAsync(applicationUser);

            // Store original AppSettings values to restore them later, to avoid impacting other tests.
            var originalJwtSecretKey = ConfigurationManager.AppSettings["JwtSecretKey"];
            var originalJwtIssuer = ConfigurationManager.AppSettings["JwtIssuer"];
            var originalJwtAudience = ConfigurationManager.AppSettings["JwtAudience"];

            // Simulate missing JWT configuration by temporarily setting them to null/empty
            ConfigurationManager.AppSettings["JwtSecretKey"] = null;
            ConfigurationManager.AppSettings["JwtIssuer"] = "";
            ConfigurationManager.AppSettings["JwtAudience"] = null;

            // Act
            var result = await _authController.LoginSpa(loginViewModel) as JsonResult;

            // Assert
            Assert.IsNotNull(result, "Result should be a JsonResult.");
            Assert.IsNotNull(result.Data, "JsonResult.Data should not be null.");
            dynamic data = result.Data;
            var successProperty = data.GetType().GetProperty("success");
            var messageProperty = data.GetType().GetProperty("message");
            Assert.IsNotNull(successProperty, "'success' property not found.");
            Assert.IsNotNull(messageProperty, "'message' property not found.");

            bool success = (bool)successProperty.GetValue(data);
            string message = messageProperty.GetValue(data) as string;

            Assert.IsFalse(success, "Login should not be successful if JWT config is missing.");
            Assert.AreEqual("Authentication configuration error.", message, "Error message should indicate JWT configuration error.");

            // Restore original AppSettings to avoid side-effects on other tests
            ConfigurationManager.AppSettings["JwtSecretKey"] = originalJwtSecretKey;
            ConfigurationManager.AppSettings["JwtIssuer"] = originalJwtIssuer;
            ConfigurationManager.AppSettings["JwtAudience"] = originalJwtAudience;
        }
    }
}
