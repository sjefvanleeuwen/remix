using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RemixHub.Server.Data;
using RemixHub.Server.Services;
using RemixHub.Shared.Models; // Use shared models instead of DTOs
using RemixHub.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RemixHub.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TracksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;
        private readonly IAudioMetadataService _audioMetadata;
        private readonly ILogger<TracksController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailService _emailService;

        public TracksController(
            ApplicationDbContext context,
            IFileStorageService fileStorage,
            IAudioMetadataService audioMetadata,
            ILogger<TracksController> logger,
            IWebHostEnvironment environment,
            IEmailService emailService)
        {
            _context = context;
            _fileStorage = fileStorage;
            _audioMetadata = audioMetadata;
            _logger = logger;
            _environment = environment;
            _emailService = emailService;
        }

        // Define track status enum
        public enum TrackStatus
        {
            Pending = 0,
            Approved = 1,
            Rejected = 2
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrackViewModel>>> GetTracks([FromQuery] TrackFilterViewModel filter)
        {
            var tracksQuery = _context.Tracks
                .Include(t => t.User)
                .Include(t => t.Genre)
                .Include(t => t.Subgenre)
                .Where(t => t.IsApproved);

            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                tracksQuery = tracksQuery.Where(t => 
                    t.Title.Contains(filter.Keyword) || 
                    t.Artist.Contains(filter.Keyword) || 
                    t.Description.Contains(filter.Keyword));
            }

            int genreId = filter.GenreId ?? 0;
            if (genreId > 0)
            {
                tracksQuery = tracksQuery.Where(t => t.GenreId == genreId);
            }

            tracksQuery = filter.SortBy switch
            {
                "oldest" => tracksQuery.OrderBy(t => t.UploadDate),
                "title" => tracksQuery.OrderBy(t => t.Title),
                "artist" => tracksQuery.OrderBy(t => t.Artist),
                _ => tracksQuery.OrderByDescending(t => t.UploadDate)
            };

            var pageSize = filter.PageSize ?? 20;
            var page = filter.Page ?? 1;
            var totalCount = await tracksQuery.CountAsync();
            var tracks = await tracksQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var trackViewModels = tracks.Select(t => new TrackViewModel
            {
                TrackId = t.TrackId,
                Title = t.Title,
                Artist = t.Artist,
                Album = t.Album ?? string.Empty,
                GenreId = t.GenreId,
                GenreName = t.Genre.Name,
                SubgenreId = t.SubgenreId,
                SubgenreName = t.Subgenre?.Name ?? string.Empty,
                Description = t.Description,
                DurationSeconds = t.DurationSeconds,
                Bpm = t.Bpm,
                MusicalKey = t.MusicalKey,
                UploadDate = t.UploadDate,
                UserName = t.User.UserName ?? "Unknown",
                UserId = t.UserId
            }).ToList();

            return Ok(new TracksResponseViewModel
            {
                Tracks = trackViewModels,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TrackDetailViewModel>> GetTrack(int id)
        {
            var track = await _context.Tracks
                .Include(t => t.User)
                .Include(t => t.Genre)
                .Include(t => t.Subgenre)
                .Include(t => t.Stems)
                    .ThenInclude(s => s.InstrumentType)
                .FirstOrDefaultAsync(t => t.TrackId == id);

            if (track == null)
                return NotFound();

            if (!track.IsApproved && track.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid();

            var trackDetail = new TrackDetailViewModel
            {
                TrackId = track.TrackId,
                Title = track.Title,
                Artist = track.Artist,
                Album = track.Album ?? string.Empty,
                GenreId = track.GenreId,
                GenreName = track.Genre.Name,
                SubgenreId = track.SubgenreId,
                SubgenreName = track.Subgenre?.Name ?? string.Empty,
                Description = track.Description,
                DurationSeconds = track.DurationSeconds,
                Bpm = track.Bpm,
                MusicalKey = track.MusicalKey,
                BitRate = track.BitRate,
                SampleRate = track.SampleRate,
                UploadDate = track.UploadDate,
                UserName = track.User.UserName ?? "Unknown",
                UserId = track.UserId,
                Stems = track.Stems.Select(s => new StemViewModel
                {
                    StemId = s.StemId,
                    Name = s.Name,
                    Description = s.Description,
                    InstrumentTypeId = s.InstrumentTypeId,
                    InstrumentTypeName = s.InstrumentType.Name
                }).ToList()
            };

            return Ok(trackDetail);
        }

        [HttpPost]
        public async Task<IActionResult> UploadTrack([FromForm] TrackUpload model)
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Upload attempted with no authenticated user");
                    return Unauthorized();
                }

                _logger.LogInformation("User {UserId} is uploading a track", userId);

                // Enhanced file validation and logging
                _logger.LogDebug("======= TRACK UPLOAD DEBUG =======");
                if (model == null)
                {
                    _logger.LogWarning("Upload model is null");
                    return BadRequest(new { error = "No data received" });
                }
                
                _logger.LogDebug("Model received: Title={Title}, Artist={Artist}, GenreId={GenreId}", 
                    model.Title, model.Artist, model.GenreId);
                
                if (model.TrackFile == null)
                {
                    _logger.LogWarning("TrackFile property is null");
                    return BadRequest(new { error = "No file received" });
                }

                _logger.LogDebug("File object exists in model: IFormFile.Name={Name}", model.TrackFile.Name);
                
                if (model.TrackFile.Length == 0)
                {
                    _logger.LogWarning("TrackFile has zero length");
                    return BadRequest(new { error = "File is empty" });
                }

                // Log detailed file information
                _logger.LogInformation("File details - Name: {FileName}, Size: {FileSize} bytes, ContentType: {ContentType}",
                    model.TrackFile.FileName, model.TrackFile.Length, model.TrackFile.ContentType);
                
                // Log headers to identify any potential issues with the request
                foreach (var header in Request.Headers)
                {
                    _logger.LogDebug("Request Header: {Key} = {Value}", header.Key, header.Value);
                }

                // Process the file
                _logger.LogDebug("Processing file...");
                string filePath = await _fileStorage.SaveTrackFileAsync(model.TrackFile);
                _logger.LogDebug("File saved to path: {FilePath}", filePath);

                // Create track entity
                var track = new Track
                {
                    Title = model.Title ?? string.Empty,
                    Artist = model.Artist ?? string.Empty,
                    Album = model.Album,
                    Description = model.Description,
                    GenreId = model.GenreId,
                    SubgenreId = model.SubgenreId,
                    Bpm = model.Bpm,
                    MusicalKey = model.MusicalKey,
                    UserId = userId,
                    UploadDate = DateTime.UtcNow,
                    Status = (int)TrackStatus.Pending,
                    FilePath = filePath,
                    FileSize = model.TrackFile.Length,
                    FileFormat = Path.GetExtension(model.TrackFile.FileName).TrimStart('.').ToLower()
                };

                _logger.LogDebug("Track entity created, extracting metadata...");
                
                // Extract metadata
                try
                {
                    var metadata = await _audioMetadata.ExtractMetadataAsync(filePath);
                    _logger.LogDebug("Metadata extracted - Duration: {Duration}s, BitRate: {BitRate}, SampleRate: {SampleRate}", 
                        metadata.DurationSeconds, metadata.BitRate, metadata.SampleRate);
                    
                    track.DurationSeconds = metadata.DurationSeconds ?? 0;
                    track.BitRate = metadata.BitRate > 0 ? metadata.BitRate : 128;
                    track.SampleRate = metadata.SampleRate;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to extract metadata from track at {FilePath}", filePath);
                }

                _logger.LogDebug("Saving track to database...");
                _context.Tracks.Add(track);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Track saved successfully with ID: {TrackId}", track.TrackId);

                var trackViewModel = new TrackViewModel
                {
                    TrackId = track.TrackId,
                    Title = track.Title,
                    Artist = track.Artist,
                    GenreId = track.GenreId,
                    SubgenreId = track.SubgenreId,
                    Description = track.Description,
                    DurationSeconds = track.DurationSeconds,
                    Bpm = track.Bpm,
                    MusicalKey = track.MusicalKey,
                    UploadDate = track.UploadDate,
                    UserId = track.UserId
                };

                return Ok(trackViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during track upload");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("{trackId}/stems")]
        public async Task<ActionResult<StemViewModel>> UploadStem(int trackId, [FromForm] StemUploadViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.StemFile == null)
                return BadRequest("No file was uploaded.");

            var file = model.StemFile as IFormFile;
            if (file == null)
                return BadRequest("Invalid file format.");

            var track = await _context.Tracks.FindAsync(trackId);
            if (track == null)
                return NotFound("Track not found.");

            if (track.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Forbid("You don't have permission to add stems to this track.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".mp3", ".flac", ".wav" };

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Invalid file format. Supported formats: MP3, FLAC, WAV.");

            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "stems");
            var filePath = Path.Combine(uploadsFolder, fileName);

            Directory.CreateDirectory(uploadsFolder);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var metadata = await _audioMetadata.ExtractMetadataAsync(filePath);

            var stem = new Stem
            {
                Name = model.Name,
                Description = model.Description,
                TrackId = trackId,
                InstrumentTypeId = model.InstrumentTypeId, // Direct assignment
                FilePath = Path.Combine("uploads", "stems", fileName),
                FileSize = file.Length,
                FileFormat = extension.TrimStart('.').ToUpper(),
                DurationSeconds = metadata.DurationSeconds ?? 0,
                UploadDate = DateTime.UtcNow
            };

            _context.Stems.Add(stem);
            await _context.SaveChangesAsync();

            return new StemViewModel
            {
                StemId = stem.StemId,
                Name = stem.Name,
                Description = stem.Description,
                InstrumentTypeId = stem.InstrumentTypeId,
                InstrumentTypeName = await _context.InstrumentTypes
                    .Where(i => i.InstrumentTypeId == stem.InstrumentTypeId)
                    .Select(i => i.Name)
                    .FirstOrDefaultAsync()
            };
        }

        [Authorize]
        [HttpPost("{trackId}/remix")]
        public async Task<IActionResult> CreateRemix(int trackId, [FromForm] RemixCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (model.RemixFile == null)
                    return BadRequest("No file was uploaded.");

                var file = model.RemixFile as IFormFile;
                if (file == null)
                    return BadRequest("Invalid file format.");

                var originalTrack = await _context.Tracks
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.TrackId == trackId);

                if (originalTrack == null)
                    return NotFound("Original track not found.");

                if (!originalTrack.IsApproved)
                    return BadRequest("Cannot remix a track that hasn't been approved yet.");

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".mp3", ".flac", ".wav" };

                if (!allowedExtensions.Contains(extension))
                    return BadRequest("Invalid file format. Supported formats: MP3, FLAC, WAV.");

                var fileName = $"{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "tracks");
                var filePath = Path.Combine(uploadsFolder, fileName);

                Directory.CreateDirectory(uploadsFolder);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var metadata = await _audioMetadata.ExtractMetadataAsync(filePath);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var remixTrack = new Track
                {
                    Title = model.Title,
                    Artist = await _context.Users
                        .Where(u => u.Id == userId)
                        .Select(u => u.UserName)
                        .FirstOrDefaultAsync() ?? "Unknown Artist",
                    GenreId = model.GenreId ?? originalTrack.GenreId,
                    Description = model.Description ?? string.Empty,
                    MusicalKey = originalTrack.MusicalKey ?? string.Empty,
                    Bpm = originalTrack.Bpm as int?,
                    FilePath = Path.Combine("uploads", "tracks", fileName),
                    FileSize = file.Length,
                    FileFormat = extension.TrimStart('.').ToUpper(),
                    DurationSeconds = (int)metadata.DurationSeconds,
                    BitRate = metadata.BitRate,
                    SampleRate = metadata.SampleRate,
                    Status = (int)TrackStatus.Pending, // Tracks are pending approval by default
                    UserId = userId,
                    UploadDate = DateTime.UtcNow
                };

                _context.Tracks.Add(remixTrack);
                await _context.SaveChangesAsync();

                var remix = new Remix
                {
                    OriginalTrackId = trackId,
                    RemixTrackId = remixTrack.TrackId,
                    RemixReason = model.RemixReason,
                    StemsUsed = model.StemsUsed,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Remixes.Add(remix);
                await _context.SaveChangesAsync();

                if (originalTrack.User.Email != null)
                {
                    await NotifyRemixCreated(
                        originalTrack.User.Email,
                        originalTrack.Title,
                        remixTrack.Title, 
                        remixTrack.Artist);
                }

                var trackViewModel = new TrackViewModel
                {
                    TrackId = remixTrack.TrackId,
                    Title = remixTrack.Title,
                    Artist = remixTrack.Artist,
                    GenreId = remixTrack.GenreId,
                    SubgenreId = remixTrack.SubgenreId,
                    Description = remixTrack.Description,
                    DurationSeconds = remixTrack.DurationSeconds,
                    Bpm = remixTrack.Bpm,
                    MusicalKey = remixTrack.MusicalKey,
                    UploadDate = remixTrack.UploadDate,
                    UserId = remixTrack.UserId
                };

                return Ok(trackViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during remix creation");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrack(int id, TrackViewModel model)
        {
            if (id != model.TrackId)
                return BadRequest();

            var track = await _context.Tracks.FindAsync(id);
            if (track == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (track.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            UpdateTrackFromModel(track, model);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TrackExists(id))
                    return NotFound();
                else
                    throw;
            }

            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrack(int id)
        {
            var track = await _context.Tracks
                .Include(t => t.Stems)
                .Include(t => t.RemixCollection)
                .FirstOrDefaultAsync(t => t.TrackId == id);

            if (track == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (track.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            if (track.RemixCollection.Any())
                return BadRequest(new { message = "Cannot delete a track that has been remixed. Contact an admin for assistance." });

            if (System.IO.File.Exists(Path.Combine(_environment.WebRootPath, track.FilePath)))
            {
                System.IO.File.Delete(Path.Combine(_environment.WebRootPath, track.FilePath));
            }

            foreach (var stem in track.Stems)
            {
                if (System.IO.File.Exists(Path.Combine(_environment.WebRootPath, stem.FilePath)))
                {
                    System.IO.File.Delete(Path.Combine(_environment.WebRootPath, stem.FilePath));
                }
            }

            _context.Stems.RemoveRange(track.Stems);
            _context.Tracks.Remove(track);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool TrackExists(int id)
        {
            return _context.Tracks.Any(e => e.TrackId == id);
        }

        private async Task NotifyRemixCreated(string email, string originalTrackTitle, string remixTitle, string remixerName)
        {
            string subject = $"Your track '{originalTrackTitle}' has been remixed!";
            string body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h1 style='color: #333;'>Your track has been remixed!</h1>
                        <p>Hello from RemixHub!</p>
                        <p>We're excited to inform you that {remixerName} has created a remix of your track '{originalTrackTitle}'.</p>
                        <p>The remix is titled '{remixTitle}' and will be available on the platform once approved by our moderation team.</p>
                        <p>Log in to your RemixHub account to check it out!</p>
                        <p>Thank you for being a part of our creative community.</p>
                    </div>
                </body>
                </html>";

            await _emailService.SendEmailAsync(email, subject, body);
        }

        private async Task<Track> CreateTrackFromModel(TrackUpload model, string userId)
        {
            var track = new Track
            {
                Title = model.Title ?? string.Empty,
                Artist = model.Artist ?? string.Empty,
                Album = model.Album,
                Description = model.Description,
                GenreId = model.GenreId,
                SubgenreId = model.SubgenreId,
                Bpm = model.Bpm,
                MusicalKey = model.MusicalKey,
                UserId = userId,
                UploadDate = DateTime.UtcNow,
                Status = (int)TrackStatus.Pending
            };

            if (model.TrackFile != null)
            {
                track.FilePath = await _fileStorage.SaveTrackFileAsync(model.TrackFile);
                track.FileSize = model.TrackFile.Length;
                track.FileFormat = Path.GetExtension(model.TrackFile.FileName).TrimStart('.').ToLower();
            }

            return track;
        }

        private void UpdateTrackFromModel(Track track, TrackViewModel model)
        {
            track.Title = model.Title ?? string.Empty;
            track.Artist = model.Artist ?? string.Empty;
            track.Album = model.Album;
            track.Description = model.Description;
            track.GenreId = model.GenreId;
            track.SubgenreId = model.SubgenreId;
            track.Bpm = model.Bpm;
            track.MusicalKey = model.MusicalKey;
            track.Status = model.IsApproved ? (int)TrackStatus.Approved : (int)TrackStatus.Pending;
        }
    }
}