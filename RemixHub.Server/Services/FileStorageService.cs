using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RemixHub.Server.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _uploadDirectory;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
        {
            _uploadDirectory = configuration["Storage:UploadDirectory"] ?? "Uploads";
            _logger = logger;
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(_uploadDirectory))
            {
                Directory.CreateDirectory(_uploadDirectory);
                _logger.LogInformation("Created upload directory: {Directory}", _uploadDirectory);
            }
            
            // Create subdirectories for different file types
            EnsureDirectoryExists(Path.Combine(_uploadDirectory, "Tracks"));
            EnsureDirectoryExists(Path.Combine(_uploadDirectory, "Stems"));
            EnsureDirectoryExists(Path.Combine(_uploadDirectory, "Remixes"));
        }
        
        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                _logger.LogInformation("Created directory: {Directory}", path);
            }
        }

        public async Task<string> SaveTrackFileAsync(IFormFile file)
        {
            return await SaveFileAsync(file, "Tracks");
        }

        public async Task<string> SaveStemFileAsync(IFormFile file)
        {
            return await SaveFileAsync(file, "Stems");
        }

        public async Task<string> SaveRemixFileAsync(IFormFile file)
        {
            return await SaveFileAsync(file, "Remixes");
        }

        private async Task<string> SaveFileAsync(IFormFile file, string subfolder)
        {
            try
            {
                _logger.LogDebug("======= FILE SAVING DEBUG =======");
                _logger.LogDebug("Starting file save for {Subfolder}", subfolder);
                
                if (file == null)
                {
                    _logger.LogWarning("SaveFileAsync called with null file parameter");
                    throw new ArgumentException("File is null");
                }

                _logger.LogDebug("File info - Name: {FileName}, Length: {FileLength}, ContentType: {ContentType}",
                    file.FileName, file.Length, file.ContentType);
                
                if (file.Length == 0)
                {
                    _logger.LogWarning("Empty file upload attempt - filename: {FileName}", file.FileName);
                    throw new ArgumentException("File is empty");
                }

                // Create a unique filename to avoid conflicts
                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                string folderPath = Path.Combine(_uploadDirectory, subfolder);
                string filePath = Path.Combine(folderPath, fileName);
                
                _logger.LogDebug("Target folder exists: {Exists}", Directory.Exists(folderPath));
                _logger.LogDebug("Upload directory: {Directory}", _uploadDirectory);
                _logger.LogDebug("Folder path: {FolderPath}", folderPath);
                _logger.LogDebug("File path: {FilePath}", filePath);

                _logger.LogInformation("Saving file to: {FilePath}", filePath);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    _logger.LogDebug("FileStream created, copying file data...");
                    await file.CopyToAsync(stream);
                    _logger.LogDebug("File data copied successfully");
                }
                
                // Verify file was created
                bool fileExists = File.Exists(filePath);
                long fileSize = fileExists ? new FileInfo(filePath).Length : 0;
                _logger.LogDebug("File created: {FileExists}, Size on disk: {FileSize} bytes", fileExists, fileSize);
                
                // Return a relative path that can be stored in the database
                string relativePath = Path.Combine(subfolder, fileName);
                _logger.LogDebug("Returning relative path: {RelativePath}", relativePath);
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file: {ErrorMessage}", ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {InnerError}", ex.InnerException.Message);
                }
                throw;
            }
        }

        public async Task<byte[]> GetTrackFileAsync(string filePath)
        {
            return await GetFileAsync(filePath);
        }

        public async Task<byte[]> GetStemFileAsync(string filePath)
        {
            return await GetFileAsync(filePath);
        }

        private async Task<byte[]> GetFileAsync(string relativePath)
        {
            string fullPath = Path.Combine(_uploadDirectory, relativePath);
            
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File not found: {FilePath}", fullPath);
                throw new FileNotFoundException($"File not found: {relativePath}");
            }
            
            _logger.LogInformation("Reading file: {FilePath}", fullPath);
            return await File.ReadAllBytesAsync(fullPath);
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                string fullPath = Path.Combine(_uploadDirectory, filePath);
                
                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning("Delete attempt for non-existent file: {FilePath}", fullPath);
                    return false;
                }
                
                File.Delete(fullPath);
                _logger.LogInformation("Deleted file: {FilePath}", fullPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return false;
            }
        }
    }
}
