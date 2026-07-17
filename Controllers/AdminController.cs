using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicBaseApp.Data;
using MusicBaseApp.Models;
using Supabase;  // 🔥🔥🔥 Supabase Namespace (MySqlX नहीं!)

namespace MusicBaseApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly Supabase.Client _supabaseClient;  // 🔥 Supabase.Client
        private readonly IConfiguration _configuration;

        private static readonly string[] AllowedAudioExtensions = { ".mp3" };
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" };

        // Bucket Name - यह वही है जो आपने Supabase में बनाया था
        private const string BucketName = "musicplayer-uploads";

        public AdminController(AppDbContext context, Supabase.Client supabaseClient, IConfiguration configuration)
        {
            _context = context;
            _supabaseClient = supabaseClient;
            _configuration = configuration;
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

            // 1. Validate MP3 file
            if (model.Mp3File == null || model.Mp3File.Length == 0)
            {
                ModelState.AddModelError(nameof(model.Mp3File), "MP3 file is required.");
                return View(model);
            }

            var mp3Ext = Path.GetExtension(model.Mp3File.FileName).ToLowerInvariant();
            if (!AllowedAudioExtensions.Contains(mp3Ext))
            {
                ModelState.AddModelError(nameof(model.Mp3File), "Only .mp3 files are allowed.");
                return View(model);
            }

            // 2. Validate Cover Image (if provided)
            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                var coverExt = Path.GetExtension(model.CoverImage.FileName).ToLowerInvariant();
                if (!AllowedImageExtensions.Contains(coverExt))
                {
                    ModelState.AddModelError(nameof(model.CoverImage), "Only .jpg/.jpeg/.png files are allowed.");
                    return View(model);
                }
            }

            // 3. Generate unique filenames
            var mp3FileName = $"{Guid.NewGuid()}{mp3Ext}";
            string? coverFileName = null;
            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                var coverExt = Path.GetExtension(model.CoverImage.FileName).ToLowerInvariant();
                coverFileName = $"{Guid.NewGuid()}{coverExt}";
            }

            // 4. Upload MP3 to Supabase Storage
            var storage = _supabaseClient.Storage;
            try
            {
                using (var mp3Stream = model.Mp3File.OpenReadStream())
                {
                    using (var ms = new MemoryStream())
                    {
                        await mp3Stream.CopyToAsync(ms);
                        var mp3Bytes = ms.ToArray();
                        var response = await storage.From(BucketName).Upload(mp3Bytes, mp3FileName);
                        if (response == null)
                        {
                            ModelState.AddModelError("", "MP3 file upload to Supabase failed.");
                            return View(model);
                        }
                    }
                }

                // 5. Upload Cover Image (if any) to Supabase Storage
                if (model.CoverImage != null && model.CoverImage.Length > 0 && coverFileName != null)
                {
                    using (var coverStream = model.CoverImage.OpenReadStream())
                    {
                        using (var ms = new MemoryStream())
                        {
                            await coverStream.CopyToAsync(ms);
                            var coverBytes = ms.ToArray();
                            var response = await storage.From(BucketName).Upload(coverBytes, coverFileName);
                            if (response == null)
                            {
                                ModelState.AddModelError("", "Cover image upload to Supabase failed.");
                                return View(model);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Upload failed: {ex.Message}");
                return View(model);
            }

            // 6. Build public URLs
            var supabaseUrl = _configuration["Supabase:Url"];
            var baseUrl = $"{supabaseUrl}/storage/v1/object/public/{BucketName}";
            var mp3Url = $"{baseUrl}/{mp3FileName}";
            var coverUrl = coverFileName != null ? $"{baseUrl}/{coverFileName}" : string.Empty;

            // 7. Save to Database
            var song = new Song
            {
                Title = model.Title,
                Artist = model.Artist,
                FilePath = mp3Url,
                CoverPath = coverUrl
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"'{song.Title}' uploaded successfully to Supabase!";
            return RedirectToAction(nameof(Upload));
        }
    }
}