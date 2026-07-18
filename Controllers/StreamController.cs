using Microsoft.AspNetCore.Mvc;
using MusicBaseApp.Data;

namespace MusicBaseApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StreamController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StreamController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Stream(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null || string.IsNullOrEmpty(song.FilePath))
            {
                return NotFound();
            }

            return Redirect(song.FilePath);
        }
    }
}
