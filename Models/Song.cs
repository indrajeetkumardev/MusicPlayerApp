namespace MusicBaseApp.Models
{
    public class Song
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string FilePath { get; set; }
        public string? CoverPath { get; set; }
    }
}