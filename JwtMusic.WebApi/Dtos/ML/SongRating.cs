namespace JwtMusic.WebApi.Dtos.ML
{
    public class SongRating
    {
        public float UserId { get; set; }

        public float SongId { get; set; }

        public float Label { get; set; }
    }
}
