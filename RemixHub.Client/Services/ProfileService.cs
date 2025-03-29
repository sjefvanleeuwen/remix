using System.Net.Http.Json;
using RemixHub.Shared.ViewModels;

namespace RemixHub.Client.Services
{
    public interface IProfileService
    {
        Task<UserProfileViewModel> GetProfileAsync();
        Task<bool> UpdateProfileAsync(UserProfileViewModel model);
        Task<List<TrackViewModel>> GetUserTracksAsync();
        Task<List<RemixViewModel>> GetUserRemixesAsync();
    }

    public class ProfileService : IProfileService
    {
        private readonly HttpClient _httpClient;

        public ProfileService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserProfileViewModel> GetProfileAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<UserProfileViewModel>("api/profile");
            return response ?? new UserProfileViewModel();
        }

        public async Task<bool> UpdateProfileAsync(UserProfileViewModel model)
        {
            var response = await _httpClient.PutAsJsonAsync("api/profile", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<TrackViewModel>> GetUserTracksAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<TrackViewModel>>("api/profile/tracks");
            return response ?? new List<TrackViewModel>();
        }

        public async Task<List<RemixViewModel>> GetUserRemixesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<RemixViewModel>>("api/profile/remixes");
            return response ?? new List<RemixViewModel>();
        }
    }

    public class UserProfileViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
        public string SocialLinks { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActive { get; set; }
        public int TrackCount { get; set; }
        public int RemixCount { get; set; }
    }
}
