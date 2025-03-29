using System.Net.Http.Json;
using RemixHub.Shared.ViewModels;

namespace RemixHub.Client.Services
{
    public interface IGenreService
    {
        Task<List<GenreViewModel>> GetGenresAsync();
        Task<GenreViewModel> GetGenreAsync(int id);
        Task<GenreViewModel> CreateGenreAsync(GenreViewModel model);
        Task<bool> UpdateGenreAsync(GenreViewModel model);
        Task<bool> DeleteGenreAsync(int id);
    }

    public class GenreService : IGenreService
    {
        private readonly HttpClient _httpClient;

        public GenreService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<GenreViewModel>> GetGenresAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<GenreViewModel>>("api/admin/genres");
            return response ?? new List<GenreViewModel>();
        }

        public async Task<GenreViewModel> GetGenreAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<GenreViewModel>($"api/admin/genres/{id}");
            return response ?? new GenreViewModel();
        }

        public async Task<GenreViewModel> CreateGenreAsync(GenreViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/admin/genres", model);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<GenreViewModel>() ?? new GenreViewModel();
        }

        public async Task<bool> UpdateGenreAsync(GenreViewModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/admin/genres/{model.GenreId}", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteGenreAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/admin/genres/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
