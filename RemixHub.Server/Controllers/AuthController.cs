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
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.Json;

namespace RemixHub.Server.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ICaptchaService _captchaService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IEmailService emailService,
            ICaptchaService captchaService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
            _captchaService = captchaService;
            _logger = logger;
        }

        private string GetVerificationUrl(string userId, string token)
        {
            var request = HttpContext.Request;
            string appUrl = _configuration["AppUrl"];

            if (string.IsNullOrEmpty(appUrl))
            {
                appUrl = $"http://localhost:5002";
            }

            return $"{appUrl}/verify-email?userId={userId}&token={Uri.EscapeDataString(token)}";
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            try
            {
                _logger.LogInformation("Registration attempt with Username={Username}, Email={Email}, CaptchaKey={CaptchaKeyLength}, CaptchaResponse={CaptchaResponse}",
                    model.Username, model.Email, model.CaptchaKey?.Length ?? 0, model.CaptchaResponse);

                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    _logger.LogWarning("Model validation failed: {Errors}", errors);
                    return BadRequest(ModelState);
                }

                // Validate CAPTCHA
                if (string.IsNullOrEmpty(model.CaptchaKey) || string.IsNullOrEmpty(model.CaptchaResponse))
                {
                    _logger.LogWarning("CAPTCHA validation failed: Key or Response is empty");
                    ModelState.AddModelError("Captcha", "CAPTCHA verification is required");
                    return BadRequest(ModelState);
                }

                bool captchaValid = _captchaService.ValidateCaptcha(model.CaptchaKey, model.CaptchaResponse);
                _logger.LogInformation("CAPTCHA validation result: {Result}", captchaValid);

                if (!captchaValid)
                {
                    _logger.LogWarning("Invalid CAPTCHA response");
                    ModelState.AddModelError("Captcha", "CAPTCHA verification failed");
                    return BadRequest(ModelState);
                }

                // Create user
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    DisplayName = model.Username, // Set DisplayName to Username by default
                    CreatedAt = DateTime.UtcNow,
                    LastActive = DateTime.UtcNow,
                    IsVerified = false // Default to false until email is verified
                };

                _logger.LogDebug("Creating user with properties: {UserProperties}", JsonSerializer.Serialize(new 
                { 
                    user.UserName, 
                    user.Email, 
                    user.DisplayName, 
                    user.CreatedAt
                }));

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created successfully, adding to 'User' role");
                    // Add user to regular "User" role
                    await _userManager.AddToRoleAsync(user, "User");

                    try
                    {
                        // Generate email verification token
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                        // Get dynamic verification URL
                        var verificationUrl = GetVerificationUrl(user.Id, token);

                        _logger.LogInformation("Sending verification email with URL: {Url}", verificationUrl);

                        await _emailService.SendEmailAsync(
                            user.Email,
                            "Verify your RemixHub account",
                            $"Please verify your account by clicking <a href='{verificationUrl}'>here</a>."
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't fail registration if email sending fails
                        _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
                    }

                    _logger.LogInformation("User {UserId} registered successfully", user.Id);
                    return Ok(true);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    _logger.LogWarning("User registration failed with errors: {Errors}", 
                        string.Join("; ", result.Errors.Select(e => e.Description)));
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during registration");
                return StatusCode(500, "An unexpected error occurred during registration. Please try again later.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Check if user's email is confirmed
            if (!user.EmailConfirmed)
            {
                return BadRequest(new { message = "Please verify your email before logging in" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Generate JWT token
            string token = await GenerateJwtToken(user);
            
            // Debug and validate token structure 
            _logger.LogInformation("Generated JWT token for user {Email}, token format valid: {IsValid}", 
                user.Email, token.Contains(".") && token.Count(c => c == '.') == 2);
                
            if (!token.Contains(".") || token.Count(c => c == '.') != 2)
            {
                _logger.LogError("Invalid JWT token format generated for user {Email}: {Token}", user.Email, token);
                return StatusCode(500, "Error generating authentication token. Please try again.");
            }

            return Ok(new 
            {
                Token = token,
                User = new 
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    DisplayName = user.DisplayName
                }
            });
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Ensure we have a valid symmetric key
            string jwtKey = _configuration["JwtKey"];
            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 16)
            {
                _logger.LogError("JWT key is missing or too short (must be at least 16 characters)");
                jwtKey = "DefaultSecureKeyWithAtLeast32Chars!";
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            string configValue = _configuration["JwtExpireDays"] ?? "7"; 
            var expires = DateTime.Now.AddDays(Convert.ToDouble(configValue));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtIssuer"],
                audience: _configuration["JwtAudience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            // Create the token string and verify it has the correct format before returning
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
            // Log token structure details for debugging
            _logger.LogDebug("JWT Token - Header: {Header}, Payload length: {PayloadLength}", 
                token.Header.ToString(), token.Payload.Count);
                
            return tokenString;
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string userId, [FromQuery] string token)
        {
            _logger.LogInformation("Email verification requested - UserId: {UserId}, Token: {Token}", 
                userId, token);
            
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Email verification failed - Missing userId or token");
                return BadRequest("Invalid verification link. Missing parameters.");
            }

            // Clean the token of any potential encoding issues
            token = token.Trim().Replace(" ", "+"); // Replace spaces with '+' as they can get mixed up in URL encoding
            _logger.LogDebug("Cleaned token: {Token}", token);

            // Try to find the user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Email verification failed - User not found with ID: {UserId}", userId);
                return BadRequest("User not found.");
            }

            try
            {
                _logger.LogDebug("Confirming email with token length {TokenLength}", token.Length);
                
                var result = await _userManager.ConfirmEmailAsync(user, token);
                
                if (result.Succeeded)
                {
                    // Update IsVerified property
                    user.IsVerified = true;
                    await _userManager.UpdateAsync(user);
                    
                    _logger.LogInformation("Email verification succeeded for user {UserId}", userId);
                    return Ok(true);
                }
                else
                {
                    _logger.LogWarning("Email verification failed - Invalid token for user {UserId}: {Errors}", 
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    
                    // Log the token we have stored for debugging comparison
                    try {
                        var storedToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        _logger.LogDebug("For comparison, a newly generated token would be: {StoredToken}", storedToken);
                    } catch (Exception ex) {
                        _logger.LogError(ex, "Error generating comparison token");
                    }
                    
                    return BadRequest("Email verification failed. Invalid or expired token.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during email verification for user {UserId}", userId);
                return BadRequest("An error occurred during verification.");
            }
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
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Password Reset",
                    $"Please reset your password by clicking <a href='{resetLink}'>here</a>.",
                    true);
            }
            else
            {
                _logger.LogWarning("Password reset attempted for user with null or empty email");
            }
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

        [HttpPost("resend-activation")]
        public async Task<IActionResult> ResendActivation([FromBody] ResendActivationModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest("Email is required");
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // We don't want to reveal that a user doesn't exist
                return Ok();
            }

            if (user.EmailConfirmed)
            {
                // Account is already activated, but we don't reveal this information
                return Ok();
            }

            try
            {
                // Generate email verification token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // Get dynamic verification URL
                var verificationUrl = GetVerificationUrl(user.Id, token);

                _logger.LogInformation("Sending verification email with URL: {Url}", verificationUrl);

                await _emailService.SendEmailAsync(
                    user.Email,
                    "Activate your RemixHub account",
                    $"Please activate your account by clicking <a href='{verificationUrl}'>here</a>."
                );
                
                _logger.LogInformation("Activation email resent to user {Email}", model.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend activation email to {Email}", model.Email);
                return StatusCode(500, "Failed to send activation email");
            }

            return Ok();
        }
    }

    public class ResendActivationModel
    {
        public string Email { get; set; }
    }
}
