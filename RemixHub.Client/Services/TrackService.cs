using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using RemixHub.Shared.ViewModels;

namespace RemixHub.Client.Services
{
    public interface ITrackService
    {
        Task<TracksResponseViewModel> GetTracksAsync(TrackFilterViewModel filter);
        Task<TrackDetailViewModel> GetTrackAsync(int id);
        Task<TrackViewModel> UploadTrackAsync(RemixHub.Shared.ViewModels.TrackUploadViewModel model);
        Task<StemViewModel> UploadStemAsync(int trackId, RemixHub.Shared.ViewModels.StemUploadViewModel model);
        Task<TrackViewModel> CreateRemixAsync(int trackId, RemixHub.Shared.ViewModels.RemixCreateViewModel model);
        Task<bool> UpdateTrackAsync(TrackViewModel model);
        Task<bool> DeleteTrackAsync(int id);
    }

    public class TrackService : ITrackService
    {
        private readonly HttpClient _httpClient;

        public TrackService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TracksResponseViewModel> GetTracksAsync(TrackFilterViewModel filter)
        {
            var queryString = BuildQueryString(filter);
            var response = await _httpClient.GetFromJsonAsync<TracksResponseViewModel>($"api/tracks{queryString}");
            return response ?? new TracksResponseViewModel();
        }

        public async Task<TrackDetailViewModel> GetTrackAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<TrackDetailViewModel>($"api/tracks/{id}");
            return response ?? new TrackDetailViewModel();
        }

        public async Task<TrackViewModel> UploadTrackAsync(RemixHub.Shared.ViewModels.TrackUploadViewModel model)
        {
            using var content = new MultipartFormDataContent();
            
            if (model.TrackFile != null)
            {
                // Handle various file types
                if (model.TrackFile is IBrowserFile browserFile)
                {
                    // Client-side Blazor file
                    var fileContent = new StreamContent(browserFile.OpenReadStream(maxAllowedSize: 52428800)); // 50MB
                    content.Add(fileContent, "TrackFile", browserFile.Name);
                }
                // Other file type handling could be added here if needed
            }
            
            content.Add(new StringContent(model.Title ?? ""), "Title");
            content.Add(new StringContent(model.Artist ?? ""), "Artist");
            content.Add(new StringContent(model.Album ?? ""), "Album");
            content.Add(new StringContent(model.GenreId.ToString()), "GenreId");
            
            if (model.SubgenreId.HasValue)
                content.Add(new StringContent(model.SubgenreId.Value.ToString()), "SubgenreId");
                
            content.Add(new StringContent(model.Description ?? ""), "Description");
            
            if (model.Bpm.HasValue)
                content.Add(new StringContent(model.Bpm.Value.ToString()), "Bpm");
                
            content.Add(new StringContent(model.MusicalKey ?? ""), "MusicalKey");

            var response = await _httpClient.PostAsync("api/tracks", content);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<TrackViewModel>() ?? new TrackViewModel();
        }

        public async Task<StemViewModel> UploadStemAsync(int trackId, RemixHub.Shared.ViewModels.StemUploadViewModel model)
        {
            using var content = new MultipartFormDataContent();
            
            if (model.StemFile != null)
            {
                // Handle various file types
                if (model.StemFile is IBrowserFile browserFile)
                {
                    // Client-side Blazor file
                    var fileContent = new StreamContent(browserFile.OpenReadStream(maxAllowedSize: 31457280)); // 30MB
                    content.Add(fileContent, "StemFile", browserFile.Name);
                }
                // Other file type handling could be added here if needed
            }
            
            content.Add(new StringContent(model.Name), "Name");
            content.Add(new StringContent(model.InstrumentTypeId.ToString()), "InstrumentTypeId");
            content.Add(new StringContent(model.Description ?? ""), "Description");

            var response = await _httpClient.PostAsync($"api/tracks/{trackId}/stems", content);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<StemViewModel>() ?? new StemViewModel();
        }

        public async Task<TrackViewModel> CreateRemixAsync(int trackId, RemixHub.Shared.ViewModels.RemixCreateViewModel model)
        {
            using var content = new MultipartFormDataContent();
            
            if (model.RemixFile != null)
            {
                // Handle various file types
                if (model.RemixFile is IBrowserFile browserFile)
                {
                    // Client-side Blazor file
                    var fileContent = new StreamContent(browserFile.OpenReadStream(maxAllowedSize: 52428800)); // 50MB
                    content.Add(fileContent, "RemixFile", browserFile.Name);
                }
                // Other file type handling could be added here if needed
            }
            
            content.Add(new StringContent(model.Title), "Title");
            content.Add(new StringContent(model.RemixReason), "RemixReason");
            content.Add(new StringContent(model.StemsUsed), "StemsUsed");
            
            if (model.GenreId.HasValue)
                content.Add(new StringContent(model.GenreId.Value.ToString()), "GenreId");
                
            if (model.SubgenreId.HasValue)
                content.Add(new StringContent(model.SubgenreId.Value.ToString()), "SubgenreId");
                
            content.Add(new StringContent(model.Description ?? ""), "Description");

            var response = await _httpClient.PostAsync($"api/tracks/{trackId}/remix", content);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<TrackViewModel>() ?? new TrackViewModel();
        }

        public async Task<bool> UpdateTrackAsync(TrackViewModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/tracks/{model.TrackId}", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTrackAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/tracks/{id}");
            return response.IsSuccessStatusCode;
        }

        private string BuildQueryString(TrackFilterViewModel filter)
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(filter.Keyword))
                queryParams.Add($"keyword={Uri.EscapeDataString(filter.Keyword)}");
                
            if (filter.GenreId.HasValue)
                queryParams.Add($"genreId={filter.GenreId}");
                
            if (filter.MinBpm.HasValue)
                queryParams.Add($"minBpm={filter.MinBpm}");
                
            if (filter.MaxBpm.HasValue)
                queryParams.Add($"maxBpm={filter.MaxBpm}");
                
            if (filter.MinDuration.HasValue)
                queryParams.Add($"minDuration={filter.MinDuration}");
                
            if (filter.MaxDuration.HasValue)
                queryParams.Add($"maxDuration={filter.MaxDuration}");
                
            if (!string.IsNullOrEmpty(filter.Key))
                queryParams.Add($"key={Uri.EscapeDataString(filter.Key)}");
                
            if (filter.InstrumentTypeId.HasValue)
                queryParams.Add($"instrumentTypeId={filter.InstrumentTypeId}");
                
            if (filter.FromDate.HasValue)
                queryParams.Add($"fromDate={filter.FromDate.Value:yyyy-MM-dd}");
                
            if (!string.IsNullOrEmpty(filter.SortBy))
                queryParams.Add($"sortBy={Uri.EscapeDataString(filter.SortBy)}");
                
            if (filter.Page.HasValue)
                queryParams.Add($"page={filter.Page}");
                
            if (filter.PageSize.HasValue)
                queryParams.Add($"pageSize={filter.PageSize}");

            return queryParams.Any() ? $"?{string.Join("&", queryParams)}" : "";
        }
    }

    public class TracksResponseViewModel
    {
        public List<TrackViewModel> Tracks { get; set; } = new List<TrackViewModel>();
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
