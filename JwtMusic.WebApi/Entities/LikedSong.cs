namespace JwtMusic.WebApi.Entities
{
    public class LikedSong
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public int SongId { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
        public virtual Song Song { get; set; }

    }
}
