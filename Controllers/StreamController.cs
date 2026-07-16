using Microsoft.AspNetCore.Mvc;
using MusicBaseApp.Data;

namespace MusicBaseApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StreamController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StreamController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Stream(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            var webRootPath = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
            var fullPath = Path.Combine(webRootPath, song.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            return PhysicalFile(fullPath, "audio/mpeg", enableRangeProcessing: true);
        }
    }
}
