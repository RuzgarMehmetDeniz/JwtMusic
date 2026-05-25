namespace JwtMusic.WebApi.Dtos
{
    public class ResultMusicDto
    {
        public int MusicId { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public string RequiredRole { get; set; }
        public int ArtistId { get; set; }
        public string ArtistName { get; set; }
    }
}