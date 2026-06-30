namespace JwtMusic.WebApi.Dtos.LikedSong
{
    public class LikedSongListDto
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; }       // Şarkı süresi (örn: "3:45")
        public string ImageUrl { get; set; }       // Şarkı / Albüm kapağı görseli
        public string ArtistName { get; set; }     // Sanatçı Adı
        public bool IsLiked { get; set; } = true;  // Bu listedekiler zaten beğenilmiştir
        public DateTime CreatedAt { get; set; }    // Beğenilme tarihi (tarihe göre sıralamak istersen)
    }
}
