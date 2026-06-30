namespace JwtMusic.WebApi.Dtos.LikedSong
{
    public class SongListWithLikeStatusDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; }
        public string ImageUrl { get; set; }
        public string ArtistName { get; set; }
        public bool IsLiked { get; set; } 
    }
}
