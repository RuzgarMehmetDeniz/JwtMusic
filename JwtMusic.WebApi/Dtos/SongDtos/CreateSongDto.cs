namespace JwtMusic.WebApi.Dtos.SongDtos
{
    public class CreateSongDto
    {
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public string AudioUrl { get; set; }
        public string Duration { get; set; }       // "00:03:24"
        public DateTime ReleaseDate { get; set; }
        public int ArtistId { get; set; }
        public string RequiredRoleId { get; set; } // AspNetRoles tablosundaki Id
    }
}
