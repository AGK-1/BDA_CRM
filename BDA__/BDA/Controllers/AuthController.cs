using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using BDA.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;


namespace BDA.Controllers
{
    [ApiController]
    [Route("Auth")]

    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly AppDbContext _context;
      //  private readonly AuthService _authService;
        public AuthController(UserManager<IdentityUser> userManager, IEmailSender emailSender, AppDbContext context)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;
        }
  

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // Manually construct the confirmation link with correct AuthController
                var confirmationLink = $"{Request.Scheme}://{Request.Host}/Auth/ConfirmEmail?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                if (string.IsNullOrEmpty(confirmationLink))
                {
                    return BadRequest("Failed to generate the confirmation link.");
                }

                // Send email with the confirmation link
                //await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                //$"Please confirm your account by clicking on this link: {confirmationLink}");
                await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                    $"<a href=''>Click here to confirm your email</a>");

                return Ok("Registration successful. Please check your email to confirm your account.");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return BadRequest(ModelState);
        }

        //public async Task<IActionResult> ConfirmEmail(string userId, string token)
        //{
        //    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        //    {
        //        return BadRequest("UserId and token are required.");
        //    }

        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null)
        //    {
        //        return NotFound("User not found.");
        //    }

        //    var result = await _userManager.ConfirmEmailAsync(user, token);
        //    if (result.Succeeded)
        //    {
        //        return Ok("Email confirmed successfully.");
        //    }

        //    return BadRequest("Error confirming email.");
        //}

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest("UserId and token are required.");
            }

            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.ConfirmEmailAsync(identityUser, token);
            if (result.Succeeded)
            {
                // Call method to transfer the data from AspNetUsers to Users table
                await TransferToUsersTable(identityUser);

                return Ok("Email confirmed and user transferred to custom Users table.");
            }

            return BadRequest("Error confirming email.");
        }



        [HttpPost("check-login")]
        public async Task<IActionResult> CheckLogin(LoginModel model)
        {
            // Find the user by email
            var user = await _userManager.FindByEmailAsync(model.Email);

            // Check if the user exists
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Check if the password matches
            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!passwordValid)
            {
                return Unauthorized("Invalid password");
            }

            var claims = new List<Claim>
            {
             new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
             };

            // Create a claims identity
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Create the authentication properties and set the cookie to expire in 30 minutes
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Keep the cookie even if the browser is closed
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // Set cookie expiration to 30 minutes
            };

            // Sign the user in with cookie authentication
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                          new ClaimsPrincipal(claimsIdentity),
                                          authProperties);

            // Return success response
            return Ok("Login successful. Cookie is set for 30 minutes.");
            // If email and password are valid

        }


        private async Task TransferToUsersTable(IdentityUser identityUser)
        {
            // Create a new User object for your custom Users table
            var newUser = new User
            {
                Email = identityUser.Email,
                Password = identityUser.PasswordHash, // Store the hashed password
                Name = "Admin",            // You can customize these default values
                Surname = "Admin",
                Company_name = "AdminCompany",  // Default values, or you can fetch these from the user input
                Company_domain = "admindomain.com",
                Role = "Admin",           // Set default role or fetch dynamically
                CreatedAt = DateTime.UtcNow  // Set creation time
            };

            // Save the new user to your custom Users table
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
        }

        
        [HttpPost("loginlog")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
               // var token = await _authService.Login(model);

                // Get user details (for simplicity, assume model.Email is valid)
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    throw new UnauthorizedAccessException("User does not exist");
                }

                // Return both token and username
                return Ok(new
                {
                    userName = user.Email,
                   // Token = token,
                    Username = user.Email // Include username in the response
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // Handle other exceptions
            }
        }

        [Authorize]
        [HttpGet("getuser")]
        public async Task<IActionResult> YBallios()
        {
            // Extract the email from the JWT token claims
            var userNameClaim = User.FindFirst(ClaimTypes.Email);

            if (userNameClaim != null)
            {
                string email = userNameClaim.Value;

                // Find the user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Create an anonymous object to return (excluding sensitive data)
                var userData = new
                {
                    user.Id,
                    user.Name,
                    user.Surname,
                    user.Email,
                    user.Company_name,
                    user.Company_domain,
                    user.Role,
                    user.CreatedAt
                };

                // Return the user data
                return Ok(userData);
            }
            else
            {
                return Unauthorized("User is not authenticated.");
            }
        }


        

    }
}


// Model for Registration
public class RegisterModel
{
    public string Email { get; set; }
    public string Password { get; set; }
}