namespace JwtMusic.WebApi.Dtos.SongDtos
{
    public class CreateSongDto
    {
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public string AudioUrl { get; set; } // YouTube Linki gelecek
        public string Duration { get; set; } // Postman'den kolay gönderilmesi için string (Örn: "00:03:24")
        public bool IsPremium { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int ArtistId { get; set; }
    }
}
