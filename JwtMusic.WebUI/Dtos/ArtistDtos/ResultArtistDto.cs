namespace JwtMusic.WebUI.Dtos.ArtistDtos
{
    public class ResultArtistDto
    {
        public int ArtistId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Bio { get; set; }
        public long MonthlyListeners { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RequiredRole { get; set; }
    }
}
