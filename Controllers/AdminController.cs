using Microsoft.AspNetCore.Mvc;
using MusicBaseApp.Data;
using MusicBaseApp.Models;

namespace MusicBaseApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedAudioExtensions = { ".mp3" };
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" };

        public AdminController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View(new SongUploadViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(SongUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var mp3Ext = Path.GetExtension(model.Mp3File.FileName).ToLowerInvariant();
            if (!AllowedAudioExtensions.Contains(mp3Ext))
            {
                ModelState.AddModelError(nameof(model.Mp3File), "Only .mp3 files are allowed.");
                return View(model);
            }

            if (model.CoverImage != null)
            {
                var coverExt = Path.GetExtension(model.CoverImage.FileName).ToLowerInvariant();
                if (!AllowedImageExtensions.Contains(coverExt))
                {
                    ModelState.AddModelError(nameof(model.CoverImage), "Only .jpg/.jpeg/.png files are allowed.");
                    return View(model);
                }
            }

            var webRootPath = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var mp3FileName = $"{Guid.NewGuid()}{mp3Ext}";
            var mp3FullPath = Path.Combine(uploadsFolder, mp3FileName);
            using (var stream = new FileStream(mp3FullPath, FileMode.Create))
            {
                await model.Mp3File.CopyToAsync(stream);
            }

            string? coverFileName = null;
            if (model.CoverImage != null)
            {
                var coverExt = Path.GetExtension(model.CoverImage.FileName).ToLowerInvariant();
                coverFileName = $"{Guid.NewGuid()}{coverExt}";
                var coverFullPath = Path.Combine(uploadsFolder, coverFileName);
                using var coverStream = new FileStream(coverFullPath, FileMode.Create);
                await model.CoverImage.CopyToAsync(coverStream);
            }

            var song = new Song
            {
                Title = model.Title,
                Artist = model.Artist,
                FilePath = $"/uploads/{mp3FileName}",
                CoverPath = coverFileName != null ? $"/uploads/{coverFileName}" : string.Empty
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{song.Title}' uploaded successfully.";
            return RedirectToAction(nameof(Upload));
        }
    }
}
