namespace JwtMusic.WebUI.Dtos.LikedSong
{
    public class LikedSongListDto
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; } // "03:45" formatında string
        public string ImageUrl { get; set; } // Şarkı listesinde görünecek kapak resmi
        public string ArtistName { get; set; }
        public bool IsLiked { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
