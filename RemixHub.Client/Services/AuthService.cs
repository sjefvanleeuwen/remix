using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using RemixHub.Client.Auth;
using RemixHub.Shared.ViewModels;

namespace RemixHub.Client.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterViewModel model);
        Task<bool> LoginAsync(LoginViewModel model);
        Task<bool> LogoutAsync();
        Task<bool> VerifyEmailAsync(string userId, string token);
        Task<bool> ForgotPasswordAsync(ForgotPasswordViewModel model);
        Task<bool> ResetPasswordAsync(ResetPasswordViewModel model);
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> RegisterAsync(RegisterViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LoginAsync(LoginViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", model);
            
            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResponseViewModel>();
            
            if (result == null || string.IsNullOrEmpty(result.Token))
                return false;

            await _localStorage.SetItemAsync("authToken", result.Token);
            ((JwtAuthenticationStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
            
            return true;
        }

        public async Task<bool> LogoutAsync()
        {
            await ((JwtAuthenticationStateProvider)_authStateProvider).LogoutAsync();
            return true;
        }

        public async Task<bool> VerifyEmailAsync(string userId, string token)
        {
            var response = await _httpClient.GetAsync($"api/auth/verify-email?userId={userId}&token={token}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", model);
            return response.IsSuccessStatusCode;
        }
    }

    public class LoginResponseViewModel
    {
        public string Token { get; set; }
        public UserInfoViewModel User { get; set; }
    }

    public class UserInfoViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
    }
}
