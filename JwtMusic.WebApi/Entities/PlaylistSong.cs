namespace JwtMusic.WebApi.Entities
{
    public class PlaylistSong
    {
        public int PlaylistSongId { get; set; }

        public int PlaylistId { get; set; }
        public Playlist Playlist { get; set; } = null!;

        public int SongId { get; set; }
        public Song Song { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
