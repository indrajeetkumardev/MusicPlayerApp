using System.ComponentModel.DataAnnotations;

namespace MusicBaseApp.Models
{
    public class SongUploadViewModel
    {
        [Required(ErrorMessage = "Song title is required")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Artist name is required")]
        public string Artist { get; set; } = string.Empty;

        [Required(ErrorMessage = "MP3 file is required")]
        public IFormFile Mp3File { get; set; } = null!;

        public IFormFile? CoverImage { get; set; }
    }
}
