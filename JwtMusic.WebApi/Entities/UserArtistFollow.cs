namespace JwtMusic.WebApi.Entities
{
    public class UserArtistFollow
    {
        // Takip eden kullanıcının ID'si
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        // Takip edilen sanatçının ID'si
        public int ArtistId { get; set; }
        public Artist Artist { get; set; }

        // İleride ne zaman takip ettiğini görmek istersen
        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    }
}
