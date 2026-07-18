using Microsoft.AspNetCore.Mvc;
using MusicBaseApp.Data;
using MusicBaseApp.Models;
using MusicBaseApp.Services;

namespace MusicBaseApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;

        private static readonly string[] AllowedAudioExtensions = { ".mp3" };
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" };

        public AdminController(AppDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
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

            string mp3Url;
            try
            {
                mp3Url = await _cloudinaryService.UploadAudioAsync(model.Mp3File);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"MP3 upload failed: {ex.Message}");
                return View(model);
            }

            var coverUrl = string.Empty;
            if (model.CoverImage != null)
            {
                try
                {
                    coverUrl = await _cloudinaryService.UploadImageAsync(model.CoverImage);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Cover upload failed: {ex.Message}");
                    return View(model);
                }
            }

            var song = new Song
            {
                Title = model.Title,
                Artist = model.Artist,
                FilePath = mp3Url,
                CoverPath = coverUrl
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{song.Title}' uploaded successfully.";
            return RedirectToAction(nameof(Upload));
        }
    }
}
