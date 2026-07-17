using Microsoft.AspNetCore.Mvc;
using MusicBaseApp.Data;
using MusicBaseApp.Models;

namespace MusicBaseApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public IActionResult Upload() => View(new SongUploadViewModel());

        [HttpPost]
        public async Task<IActionResult> Upload(SongUploadViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var mp3FileName = $"{Guid.NewGuid()}.mp3";
            var mp3Path = Path.Combine(uploadsFolder, mp3FileName);
            using (var stream = new FileStream(mp3Path, FileMode.Create))
                await model.Mp3File.CopyToAsync(stream);

            string? coverFileName = null;
            if (model.CoverImage != null)
            {
                coverFileName = $"{Guid.NewGuid()}.jpg";
                var coverPath = Path.Combine(uploadsFolder, coverFileName);
                using (var stream = new FileStream(coverPath, FileMode.Create))
                    await model.CoverImage.CopyToAsync(stream);
            }

            var song = new Song
            {
                Title = model.Title,
                Artist = model.Artist,
                FilePath = $"/uploads/{mp3FileName}",
                CoverPath = coverFileName != null ? $"/uploads/{coverFileName}" : null
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Song uploaded!";
            return RedirectToAction(nameof(Upload));
        }
    }
}