namespace JwtMusic.WebUI.Dtos.SongDtos
{
    public class CreateSongDto
    {
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public string AudioUrl { get; set; }
        public string Duration { get; set; } // Formdan string olarak almak kolaylık sağlar
        public bool IsPremium { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int ArtistId { get; set; }
    }
}
