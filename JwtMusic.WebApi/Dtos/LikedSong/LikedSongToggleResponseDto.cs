namespace JwtMusic.WebApi.Dtos.LikedSong
{
    public class LikedSongToggleResponseDto
    {
        public string Message { get; set; }  // "Şarkı beğenildi" veya "Beğeni kaldırıldı"
        public bool IsLiked { get; set; }   // Kalbin dolu mu boş mu olacağı bilgisi (true/false)
    }
}
