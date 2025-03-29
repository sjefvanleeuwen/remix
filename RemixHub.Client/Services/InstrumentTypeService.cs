using System.Net.Http.Json;
using RemixHub.Shared.ViewModels;

namespace RemixHub.Client.Services
{
    public interface IInstrumentTypeService
    {
        Task<List<InstrumentTypeViewModel>> GetInstrumentTypesAsync();
        Task<InstrumentTypeViewModel> GetInstrumentTypeAsync(int id);
    }

    public class InstrumentTypeService : IInstrumentTypeService
    {
        private readonly HttpClient _httpClient;

        public InstrumentTypeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<InstrumentTypeViewModel>> GetInstrumentTypesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<InstrumentTypeViewModel>>("api/instrumenttypes");
            return response ?? new List<InstrumentTypeViewModel>();
        }

        public async Task<InstrumentTypeViewModel> GetInstrumentTypeAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<InstrumentTypeViewModel>($"api/instrumenttypes/{id}");
            return response ?? new InstrumentTypeViewModel();
        }
    }
}
