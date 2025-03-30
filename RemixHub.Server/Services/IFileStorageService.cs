using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace RemixHub.Server.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveTrackFileAsync(IFormFile file);
        Task<string> SaveStemFileAsync(IFormFile file);
        Task<string> SaveRemixFileAsync(IFormFile file);
        Task<byte[]> GetTrackFileAsync(string filePath);
        Task<byte[]> GetStemFileAsync(string filePath);
        Task<bool> DeleteFileAsync(string filePath);
    }
}
