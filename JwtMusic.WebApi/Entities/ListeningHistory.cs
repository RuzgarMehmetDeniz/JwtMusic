namespace JwtMusic.WebApi.Entities
{
    public class ListeningHistory
    {
        public int Id { get; set; }

        // AppUser ile ilişki
        public string AppUserId { get; set; } = null!;
        public AppUser AppUser { get; set; } = null!;

        // Song ile ilişki
        public int SongId { get; set; }
        public Song Song { get; set; } = null!;

        // Ne zaman dinlendi
        public DateTime ListenedAt { get; set; } = DateTime.UtcNow;
    }
}
