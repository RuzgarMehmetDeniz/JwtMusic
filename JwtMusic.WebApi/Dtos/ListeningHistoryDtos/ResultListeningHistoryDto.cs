namespace JwtMusic.WebApi.Dtos.ListeningHistoryDtos
{
    public class ResultListeningHistoryDto
    {
        public int Id { get; set; }
        public int SongId { get; set; }
        public string SongTitle { get; set; } = null!;
        public string ArtistName { get; set; } = null!;
        public string? CoverImageUrl { get; set; }
        public string? AudioUrl { get; set; }
        public string RequiredRoleName { get; set; } = "Basic";
        public DateTime ListenedAt { get; set; }
    }
}
