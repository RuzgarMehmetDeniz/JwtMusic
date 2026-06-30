namespace JwtMusic.WebUI.Dtos.LikedSong
{
    public class ResultLikedSongDto
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; }
        public string ImageUrl { get; set; }
        public string ArtistName { get; set; }
        public bool IsLiked { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
