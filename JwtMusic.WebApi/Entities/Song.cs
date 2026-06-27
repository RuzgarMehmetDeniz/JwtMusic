namespace JwtMusic.WebApi.Entities
{
    public class Song
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public string AudioUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public long PlayCount { get; set; }
        public DateTime ReleaseDate { get; set; }

        // IsPremium kaldırıldı
        // AspNetRoles tablosundaki Id (string) tutuluyor
        public string RequiredRole { get; set; }

        public int ArtistId { get; set; }
        public Artist Artist { get; set; }
    }
}
