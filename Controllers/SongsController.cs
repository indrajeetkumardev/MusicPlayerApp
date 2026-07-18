using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicBaseApp.Data;

namespace MusicBaseApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SongsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSongs()
        {
            var songs = await _context.Songs
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.Artist,
                    s.CoverPath,
                    s.FilePath
                })
                .ToListAsync();

            return Ok(songs);
        }
    }
}
