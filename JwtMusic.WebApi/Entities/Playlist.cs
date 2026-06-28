namespace JwtMusic.WebApi.Entities
{
    public class Playlist
    {
        public int PlaylistId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Sahibi
        public string AppUserId { get; set; } = null!;
        public AppUser AppUser { get; set; } = null!;

        // Şarkılar
        public ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
    }
}
