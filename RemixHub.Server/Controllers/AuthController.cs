using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RemixHub.Server.Services;
using RemixHub.Shared.Models;
using RemixHub.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RemixHub.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify reCAPTCHA
            if (!await VerifyRecaptchaAsync(model.RecaptchaToken))
                return BadRequest(new { message = "CAPTCHA verification failed" });

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                DisplayName = model.Username,
                CreatedAt = DateTime.UtcNow,
                LastActive = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Add to "User" role
            await _userManager.AddToRoleAsync(user, "User");

            // Generate email verification token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Send verification email
            await _emailService.SendVerificationEmailAsync(user.Email, user.Id, token);

            return Ok(new { message = "Registration successful. Please check your email to verify your account." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { message = "Invalid email or password" });

            if (!user.EmailConfirmed)
                return BadRequest(new { message = "Please verify your email before logging in" });

            // Update last active timestamp
            user.LastActive = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Generate JWT token
            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    username = user.UserName,
                    email = user.Email,
                    displayName = user.DisplayName
                }
            });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Invalid verification link" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest(new { message = "User not found" });

            var decodedToken = HttpUtility.UrlDecode(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
                return BadRequest(new { message = "Email verification failed" });

            user.IsVerified = true;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Email verified successfully. You can now log in." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Ok(new { message = "If your email is registered, you will receive a password reset link." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);
            var resetLink = $"{_configuration["AppUrl"]}/reset-password?email={model.Email}&token={encodedToken}";

            await _emailService.SendPasswordResetEmailAsync(model.Email, resetLink);

            return Ok(new { message = "If your email is registered, you will receive a password reset link." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest(new { message = "Password reset failed" });

            var decodedToken = HttpUtility.UrlDecode(model.Token);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Password has been reset successfully. You can now log in with your new password." });
        }

        [HttpGet("check-auth")]
        public IActionResult CheckAuth()
        {
            // Changed from async method to synchronous since it doesn't use await
            return Ok(new { isAuthenticated = User.Identity?.IsAuthenticated ?? false });
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("DisplayName", user.DisplayName ?? user.UserName ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtKey"] ?? "defaultkeyforidevelopmentonly12345678"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"] ?? "7"));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtIssuer"],
                audience: _configuration["JwtAudience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<bool> VerifyRecaptchaAsync(string recaptchaToken)
        {
            // This would typically be an HTTP request to the Google reCAPTCHA verification endpoint
            // For simplicity, we're just returning true in this example
            // In a real implementation, you would validate the token with Google
            
            // Example of how you might implement this:
            /*
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    "https://www.google.com/recaptcha/api/siteverify",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "secret", _configuration["RecaptchaSecretKey"] },
                        { "response", recaptchaToken }
                    })
                );

                var responseContent = await response.Content.ReadAsStringAsync();
                var recaptchaResponse = JsonSerializer.Deserialize<RecaptchaResponse>(responseContent);
                
                return recaptchaResponse.Success;
            }
            */

            return true; // Replace with actual implementation
        }
    }
}
