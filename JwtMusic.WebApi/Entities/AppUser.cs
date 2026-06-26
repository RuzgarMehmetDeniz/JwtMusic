using Microsoft.AspNetCore.Identity;

namespace JwtMusic.WebApi.Entities
{
    public class AppUser:IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? ImageUrl { get; set; }

        public ICollection<UserArtistFollow> UserArtistFollows { get; set; } = new List<UserArtistFollow>();
    }
}
