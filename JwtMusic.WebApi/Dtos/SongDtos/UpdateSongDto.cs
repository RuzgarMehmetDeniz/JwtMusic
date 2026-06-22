namespace JwtMusic.WebApi.Dtos.SongDtos
{
    public class UpdateSongDto
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public string AudioUrl { get; set; }
        public string Duration { get; set; }
        public bool IsPremium { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int ArtistId { get; set; }
    }
}
