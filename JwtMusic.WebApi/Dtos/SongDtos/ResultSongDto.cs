namespace JwtMusic.WebApi.Dtos.SongDtos
{
    public class ResultSongDto
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public string AudioUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public long PlayCount { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int ArtistId { get; set; }
        public string ArtistName { get; set; }

        // IsPremium kaldırıldı → RequiredRole eklendi
        public string RequiredRoleId { get; set; }    // AspNetRoles Id
        public string RequiredRoleName { get; set; }  // Basic / Gold / Premium / Elit    }
    }
}