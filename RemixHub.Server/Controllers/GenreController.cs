using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RemixHub.Server.Data;
using RemixHub.Shared.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace RemixHub.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GenresController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetGenres()
        {
            var genres = await _context.Genres
                .Select(g => new GenreViewModel
                {
                    GenreId = g.GenreId,
                    Name = g.Name,
                    Description = g.Description,
                    ParentGenreId = g.ParentGenreId,
                    ParentGenreName = g.ParentGenre != null ? g.ParentGenre.Name : null
                })
                .ToListAsync();

            return Ok(genres);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGenre(int id)
        {
            var genre = await _context.Genres
                .Where(g => g.GenreId == id)
                .Select(g => new GenreViewModel
                {
                    GenreId = g.GenreId,
                    Name = g.Name,
                    Description = g.Description,
                    ParentGenreId = g.ParentGenreId,
                    ParentGenreName = g.ParentGenre != null ? g.ParentGenre.Name : null
                })
                .FirstOrDefaultAsync();

            if (genre == null)
                return NotFound();

            return Ok(genre);
        }
    }
}
