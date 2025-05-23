using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ProjectManagementSystem.Web.Models; // For LoginViewModel

// JWT specific using statements
using System.IdentityModel.Tokens.Jwt; // For JwtSecurityTokenHandler, JwtRegisteredClaimNames
using Microsoft.IdentityModel.Tokens;  // For SecurityKey, SymmetricSecurityKey, SigningCredentials, SecurityAlgorithms (for newer System.IdentityModel.Tokens.Jwt v5+)
                                        // For System.IdentityModel.Tokens.Jwt v4.x, these might be in System.IdentityModel.Tokens
using System.Security.Claims;
using System.Text;
using System.Configuration; // For ConfigurationManager
using System; // For Guid, DateTimeOffset, DateTimeUtcNow


namespace ProjectManagementSystem.Web.Controllers
{
    public class AuthController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AuthController()
        {
        }

        public AuthController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("api/auth/login")] // Using attribute routing for clarity
        public async Task<ActionResult> LoginSpa(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                // Consider returning HttpStatusCode.BadRequest and a more structured error response
                return Json(new { success = false, errors = errors }); 
            }

            // Ensure this is false for API login; SPA will handle "remember me" via token persistence
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var user = await UserManager.FindByNameAsync(model.Email); // Or FindByEmailAsync if email is the username field

                    var secretKey = ConfigurationManager.AppSettings["JwtSecretKey"];
                    var issuer = ConfigurationManager.AppSettings["JwtIssuer"];
                    var audience = ConfigurationManager.AppSettings["JwtAudience"];

                    if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                    {
                        // Log this critical error: JWT configuration is missing
                        // Consider a more generic error message for the client for security
                        return Json(new { success = false, message = "Authentication configuration error." }); 
                    }
                    
                    // For System.IdentityModel.Tokens.Jwt v4.x, SymmetricSecurityKey and HmacSha256Signature are in System.IdentityModel.Tokens
                    // For v5+ (Microsoft.IdentityModel.Tokens), they are in Microsoft.IdentityModel.Tokens
                    // Given the package version 4.0.4, we'll use the types expected by that version.
                    // Note: System.IdentityModel.Tokens.InMemorySymmetricSecurityKey is for older WIF,
                    // for System.IdentityModel.Tokens.Jwt v4, it's typically SymmetricKey or a direct byte array.
                    // Let's try with Microsoft.IdentityModel.Tokens.SymmetricSecurityKey as it's more common with JwtSecurityTokenHandler.
                    // If it's v4, it might need System.IdentityModel.Tokens.SecurityKey and specific algorithm strings.
                    
                    var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                    var signingCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                        securityKey, 
                        Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature
                    );

                    var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id), // 'sub' is standard for subject (user ID)
                        new Claim(JwtRegisteredClaimNames.NameId, user.UserName), // Can also use ClaimTypes.NameIdentifier
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),                        
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                        // Add other claims as needed, e.g., roles
                        // var roles = await UserManager.GetRolesAsync(user.Id);
                        // foreach (var role in roles) { claimsList.Add(new Claim(ClaimTypes.Role, role)); }
                    };

                    var token = new JwtSecurityToken(
                        issuer: issuer,
                        audience: audience,
                        claims: claims,
                        notBefore: DateTime.UtcNow,
                        expires: DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
                        signingCredentials: signingCredentials);
                    
                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                    return Json(new { success = true, token = tokenString, userId = user.Id, userName = user.UserName });
                case SignInStatus.LockedOut:
                    return Json(new { success = false, message = "User account locked out." });
                case SignInStatus.RequiresVerification:
                    return Json(new { success = false, message = "Login requires verification." });
                case SignInStatus.Failure:
                default:
                    return Json(new { success = false, message = "Invalid login attempt." });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
