using JwtMusic.WebApi.Context;
using JwtMusic.WebApi.Services.MLServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JwtMusic.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly RecommendationService _recommendationService;
        private readonly JwtContext _context;

        public RecommendationController(RecommendationService recommendationService, JwtContext context)
        {
            _recommendationService = recommendationService;
            _context = context;
        }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        // GET api/recommendation
        [HttpGet]
        public async Task<IActionResult> GetRecommendations([FromQuery] int count = 10)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // ML servisinden öneri ID'lerini al
            var recommendedIds = await _recommendationService.GetRecommendedSongIdsAsync(userId, count);

            if (!recommendedIds.Any())
                return Ok(new List<object>());

            // ID'lere göre şarkı detaylarını çek
            // NOT: RequiredRole direkt string field — Include kullanılmaz
            var songs = await _context.Songs
                .Where(s => recommendedIds.Contains(s.SongId))
                .Include(s => s.Artist)
                .Select(s => new
                {
                    songId = s.SongId,
                    title = s.Title,
                    artistName = s.Artist != null ? s.Artist.Name : "Bilinmiyor",
                    coverImageUrl = s.CoverImageUrl,
                    audioUrl = s.AudioUrl,
                    requiredRoleName = !string.IsNullOrEmpty(s.RequiredRole) ? s.RequiredRole : "Basic",
                    duration = s.Duration.ToString(@"mm\:ss")
                })
                .ToListAsync();

            // ML'in verdiği sırayı koru
            var orderedSongs = recommendedIds
                .Select(id => songs.FirstOrDefault(s => s.songId == id))
                .Where(s => s != null)
                .ToList();

            return Ok(orderedSongs);
        }
    }
}
