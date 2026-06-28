namespace JwtMusic.WebApi.Dtos.PlaylistDtos
{
    public class PlaylistSongDto
    {
        public int PlaylistSongId { get; set; }
        public int SongId { get; set; }
        public string Title { get; set; } = null!;
        public string ArtistName { get; set; } = null!;
        public string? CoverImageUrl { get; set; }
        public string? AudioUrl { get; set; }
        public string RequiredRole { get; set; } = "Basic";
        public string Duration { get; set; } = "00:00";
        public DateTime AddedAt { get; set; }
    }
}
