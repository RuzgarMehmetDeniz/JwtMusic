namespace JwtMusic.WebUI.Dtos
{
    public class ResultMusicDto
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public string AudioUrl { get; set; }
        public string FilePath => AudioUrl; // View içindeki .filePath çağrısını karşılasın diye
        public TimeSpan Duration { get; set; }
        public int PlayCount { get; set; }
        public bool IsPremium { get; set; }
        public string RequiredRole => IsPremium ? "Premium" : "Free";
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } // Şarkı çalarken sanatçı adını göstermek için
    }
}