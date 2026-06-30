namespace JwtMusic.WebUI.Dtos.LikedSong
{
    public class ResultSongDto
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public string AudioUrl { get; set; }
        public string RequiredRoleName { get; set; }
        public int ArtistId { get; set; }
        public string ArtistName { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
