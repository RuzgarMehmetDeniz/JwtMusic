namespace JwtMusic.WebApi.Dtos
{
    public class GetArtistWithMusicsDto
    {
        public int ArtistId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }

        // Sanatçıya ait tüm şarkıların listesi
        public List<ResultMusicDto> Songs { get; set; }
    }
}
