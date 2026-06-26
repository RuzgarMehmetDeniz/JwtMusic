using JwtMusic.WebApi.Services.ArtistFollowService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtMusic.WebApi.Controllers
{
    [Authorize] // Sadece geçerli bir JWT Token'ı olan (giriş yapmış) kullanıcılar istek atabilir
    [ApiController]
    [Route("api/[controller]")]
    public class FollowsController : ControllerBase
    {
        private readonly IArtistFollowService _followService;

        public FollowsController(IArtistFollowService followService)
        {
            _followService = followService;
        }

        // Takip Et / Takipten Çık butonu için tek endpoint (Toggle)
        [HttpPost("toggle/{artistId}")]
        public async Task<IActionResult> ToggleFollow(int artistId)
        {
            // JWT Token içindeki NameIdentifier claim'inden giriş yapan kullanıcının ID'sini çekiyoruz
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var isFollowing = await _followService.ToggleFollowAsync(userId, artistId);

            return Ok(new
            {
                isFollowing = isFollowing,
                message = isFollowing ? "Sanatçı takip edildi." : "Sanatçı takipten çıkarıldı."
            });
        }

        // Giriş yapan kullanıcının takip ettiği sanatçı ID listesini döner (Sayfa yenilendiğinde koruma sağlamak için)
        [HttpGet("my-follows")]
        public async Task<IActionResult> GetMyFollows()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var followedArtistIds = await _followService.GetFollowedArtistIdsAsync(userId);
            return Ok(followedArtistIds); // Örn çıktı: [2, 5, 12]
        }
    }
}
