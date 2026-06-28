namespace JwtMusic.WebApi.Dtos.PlaylistDtos
{
    public class PlaylistDetailDto
    {
        public int PlaylistId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<PlaylistSongDto> Songs { get; set; } = new();
    }
}
