namespace JwtMusic.WebUI.Dtos
{
    public class GetArtistWithMusicsDto
    {
        public int ArtistId { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public string ImageUrl { get; set; }

        // Sanatçıya ait şarkıların listesi
        public List<ResultMusicDto> Musics { get; set; }
    }
}
