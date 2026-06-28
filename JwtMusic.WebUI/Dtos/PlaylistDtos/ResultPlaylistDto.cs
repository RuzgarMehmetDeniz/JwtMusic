namespace JwtMusic.WebUI.Dtos.PlaylistDtos
{
    public class ResultPlaylistDto
    {
        public int PlaylistId { get; set; }
        public string Name { get; set; } = null!;
        public int SongCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
