using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using RemixHub.Shared.ViewModels;
using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace RemixHub.Client.Services
{
    public interface ITrackService
    {
        Task<TracksResponseViewModel> GetTracksAsync(TrackFilterViewModel filter);
        Task<TrackDetailViewModel> GetTrackAsync(int id);
        Task<TrackViewModel> UploadTrackAsync(TrackUploadViewModel model);
        Task<StemViewModel> UploadStemAsync(int trackId, StemUploadViewModel model);
        Task<TrackViewModel> CreateRemixAsync(int trackId, RemixCreateViewModel model);
        Task<bool> UpdateTrackAsync(TrackViewModel model);
        Task<bool> DeleteTrackAsync(int id);
    }

    public class TrackService : ITrackService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public TrackService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
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

        public async Task<TrackViewModel> UploadTrackAsync(TrackUploadViewModel model)
        {
            // Get the auth token
            var token = await _localStorage.GetItemAsStringAsync("authToken");
            
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Authorization token is missing. User might need to log in again.");
                throw new UnauthorizedAccessException("Authentication token is missing. Please log in again.");
            }
            
            using var content = new MultipartFormDataContent();
            
            if (model.TrackFile != null)
            {
                // Handle file upload
                if (model.TrackFile is IBrowserFile browserFile)
                {
                    Console.WriteLine($"Preparing to upload file: {browserFile.Name}, Size: {browserFile.Size} bytes");
                    
                    try {
                        // IMPORTANT FIX: Copy the file to a memory buffer first
                        byte[] fileBytes = null;
                        var maxAllowedSize = 100 * 1024 * 1024; // 100MB max size
                        
                        using (var memoryStream = new MemoryStream())
                        {
                            await browserFile.OpenReadStream(maxAllowedSize).CopyToAsync(memoryStream);
                            fileBytes = memoryStream.ToArray();
                        }
                        
                        // Now create the content from the byte array
                        var fileContent = new ByteArrayContent(fileBytes);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(browserFile.ContentType);
                        
                        // Add to form - make sure name matches what server expects
                        content.Add(fileContent, "TrackFile", browserFile.Name);
                        
                        Console.WriteLine($"File {browserFile.Name} added to form data (size: {fileBytes.Length} bytes)");
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Error preparing file: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        if (ex.InnerException != null) {
                            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        }
                        throw;
                    }
                }
                else
                {
                    Console.WriteLine($"TrackFile is not IBrowserFile: {model.TrackFile.GetType().FullName}");
                }
            }
            else
            {
                Console.WriteLine("TrackFile is null");
            }
            
            // Add other form fields
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

            // Create a new HttpClient with the auth header explicitly set
            using var client = new HttpClient();
            client.BaseAddress = _httpClient.BaseAddress;
            
            // Make sure to include the auth token in the request
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            Console.WriteLine($"Sending upload request to: {client.BaseAddress}api/tracks");
            var response = await client.PostAsync("api/tracks", content);
            
            // Log response details for debugging
            Console.WriteLine($"Response status code: {response.StatusCode}");
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content: {responseContent}");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Upload failed: {response.StatusCode} - {responseContent}");
            }
            
            return await response.Content.ReadFromJsonAsync<TrackViewModel>() ?? new TrackViewModel();
        }

        public async Task<StemViewModel> UploadStemAsync(int trackId, StemUploadViewModel model)
        {
            // Get the auth token
            var token = await _localStorage.GetItemAsStringAsync("authToken");
            
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Authorization token is missing. User might need to log in again.");
                throw new UnauthorizedAccessException("Authentication token is missing. Please log in again.");
            }
            
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

            // Create a new HttpClient with the auth header explicitly set
            using var client = new HttpClient();
            client.BaseAddress = _httpClient.BaseAddress;
            
            // Explicitly attach the Authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await client.PostAsync($"api/tracks/{trackId}/stems", content);
            
            // Log response details regardless of success
            Console.WriteLine($"Response status code: {response.StatusCode}");
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content: {responseContent}");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Stem upload failed: {response.StatusCode} - {responseContent}");
            }
            
            return await response.Content.ReadFromJsonAsync<StemViewModel>() ?? new StemViewModel();
        }

        public async Task<TrackViewModel> CreateRemixAsync(int trackId, RemixCreateViewModel model)
        {
            // Get the auth token
            var token = await _localStorage.GetItemAsStringAsync("authToken");
            
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Authorization token is missing. User might need to log in again.");
                throw new UnauthorizedAccessException("Authentication token is missing. Please log in again.");
            }
            
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

            // Create a new HttpClient with the auth header explicitly set
            using var client = new HttpClient();
            client.BaseAddress = _httpClient.BaseAddress;
            
            // Explicitly attach the Authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await client.PostAsync($"api/tracks/{trackId}/remix", content);
            
            // Log response details regardless of success
            Console.WriteLine($"Response status code: {response.StatusCode}");
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content: {responseContent}");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Remix upload failed: {response.StatusCode} - {responseContent}");
            }
            
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