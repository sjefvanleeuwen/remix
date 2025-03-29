using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RemixHub.Server.Data;
using RemixHub.Shared.Models;
using RemixHub.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemixHub.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("tracks/pending")]
        public async Task<ActionResult<IEnumerable<TrackViewModel>>> GetPendingTracks()
        {
            var pendingTracks = await _context.Tracks
                .Include(t => t.User)
                .Include(t => t.Genre)
                .Include(t => t.Subgenre)
                .Where(t => !t.IsApproved)
                .OrderBy(t => t.UploadDate)
                .ToListAsync();

            var trackViewModels = pendingTracks.Select(t => new TrackViewModel
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

            return Ok(trackViewModels);
        }

        [HttpPost("tracks/{id}/approve")]
        public async Task<IActionResult> ApproveTrack(int id)
        {
            var track = await _context.Tracks.FindAsync(id);
            if (track == null)
                return NotFound();

            track.IsApproved = true;
            track.ApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("tracks/{id}/reject")]
        public async Task<IActionResult> RejectTrack(int id, [FromBody] string reason)
        {
            var track = await _context.Tracks
                .Include(t => t.User)
                .Include(t => t.Stems)
                .FirstOrDefaultAsync(t => t.TrackId == id);

            if (track == null)
                return NotFound();

            _context.Stems.RemoveRange(track.Stems);
            _context.Tracks.Remove(track);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserViewModel>>> GetUsers()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .ToListAsync();

            var userViewModels = new List<UserViewModel>();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var trackCount = await _context.Tracks.CountAsync(t => t.UserId == user.Id);
                var remixCount = await _context.Remixes.CountAsync(r => r.RemixTrack.UserId == user.Id);

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "Unknown",
                    Email = user.Email ?? "No Email",
                    DisplayName = user.DisplayName ?? user.UserName ?? "Unknown",
                    IsVerified = user.IsVerified,
                    CreatedAt = user.CreatedAt,
                    LastActive = user.LastActive,
                    Roles = roles.ToList(),
                    TrackCount = trackCount,
                    RemixCount = remixCount
                });
            }

            return Ok(userViewModels);
        }

        [HttpPost("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            
            if (userRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
            }

            await _userManager.AddToRoleAsync(user, model.Role);
            
            return NoContent();
        }

        [HttpPost("users/{id}/ban")]
        public async Task<IActionResult> BanUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;
            
            await _userManager.UpdateAsync(user);
            
            return NoContent();
        }

        [HttpPost("users/{id}/unban")]
        public async Task<IActionResult> UnbanUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.LockoutEnabled = false;
            user.LockoutEnd = null;
            
            await _userManager.UpdateAsync(user);
            
            return NoContent();
        }

        [HttpGet("genres")]
        public async Task<ActionResult<IEnumerable<GenreViewModel>>> GetGenres()
        {
            var genres = await _context.Genres
                .Include(g => g.ParentGenre)
                .ToListAsync();

            var genreViewModels = genres.Select(g => new GenreViewModel
            {
                GenreId = g.GenreId,
                Name = g.Name,
                Description = g.Description,
                ParentGenreId = g.ParentGenreId,
                ParentGenreName = g.ParentGenre?.Name
            }).ToList();

            return Ok(genreViewModels);
        }

        [HttpPost("genres")]
        public async Task<ActionResult<GenreViewModel>> CreateGenre([FromBody] GenreViewModel model)
        {
            var genre = new Genre
            {
                Name = model.Name,
                Description = model.Description,
                ParentGenreId = model.ParentGenreId
            };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            model.GenreId = genre.GenreId;
            return CreatedAtAction(nameof(GetGenres), new { id = genre.GenreId }, model);
        }

        [HttpPut("genres/{id}")]
        public async Task<IActionResult> UpdateGenre(int id, [FromBody] GenreViewModel model)
        {
            if (id != model.GenreId)
                return BadRequest();

            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return NotFound();

            genre.Name = model.Name;
            genre.Description = model.Description ?? string.Empty;
            genre.ParentGenreId = model.ParentGenreId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("genres/{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return NotFound();

            var hasChildGenres = await _context.Genres.AnyAsync(g => g.ParentGenreId == id);
            var isUsedInTracks = await _context.Tracks.AnyAsync(t => t.GenreId == id || t.SubgenreId == id);

            if (hasChildGenres || isUsedInTracks)
                return BadRequest(new { message = "Cannot delete genre because it is in use" });

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
