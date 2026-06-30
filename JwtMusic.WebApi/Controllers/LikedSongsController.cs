using JwtMusic.WebApi.Context;
using JwtMusic.WebApi.Dtos.LikedSong;
using JwtMusic.WebApi.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace YourProjectName.Controllers
{
    [Authorize] // Sadece giriş yapmış (Token sahibi) kullanıcılar erişebilir
    [Route("api/[controller]")]
    [ApiController]
    public class LikedSongsController : ControllerBase
    {
        private readonly JwtContext _context;

        public LikedSongsController(JwtContext context)
        {
            _context = context;
        }

        // 1. Şarkıyı Beğen / Beğeniyi Kaldır (Toggle)
        // POST: api/LikedSongs/toggle/5
        [HttpPost("toggle/{songId}")]
        public async Task<IActionResult> ToggleLikeSong(int songId)
        {
            // Token içinden giriş yapan kullanıcının ID'sini string olarak çekiyoruz
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Kullanıcı oturumu bulunamadı.");
            }

            // HATA BURADAYDI: int.Parse işlemi tamamen kaldırıldı. 
            // Çünkü Identity mekanizman string (Guid) bir Id üretiyor.

            // Veritabanında böyle bir şarkı var mı kontrolü
            var songExists = await _context.Songs.AnyAsync(s => s.SongId == songId);
            if (!songExists)
            {
                return NotFound("Beğenilmek istenen şarkı bulunamadı.");
            }

            // Kullanıcı bu şarkıyı daha önce beğenmiş mi? (userId artık string olarak karşılaştırılıyor)
            var likedSong = await _context.LikedSongs
                .FirstOrDefaultAsync(l => l.UserId == userId && l.SongId == songId);

            if (likedSong != null)
            {
                // Varsa: Beğeniyi kaldır (Veritabanından sil)
                _context.LikedSongs.Remove(likedSong);
                await _context.SaveChangesAsync();

                return Ok(new LikedSongToggleResponseDto
                {
                    Message = "Beğeni başarıyla kaldırıldı.",
                    IsLiked = false
                });
            }
            else
            {
                // Yoksa: Yeni beğeni ekle
                var newLike = new LikedSong
                {
                    UserId = userId, // string (Guid) değer doğrudan atanıyor
                    SongId = songId
                };

                _context.LikedSongs.Add(newLike);
                await _context.SaveChangesAsync();

                return Ok(new LikedSongToggleResponseDto
                {
                    Message = "Şarkı beğenilenlere eklendi.",
                    IsLiked = true
                });
            }
        }

        // 2. Giriş Yapan Kullanıcının Beğendiği Şarkıları Listele
        // GET: api/LikedSongs
        [HttpGet]
        public async Task<IActionResult> GetMyLikedSongs()
        {
            // Token içinden giriş yapan kullanıcının ID'sini string olarak alıyoruz
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Kullanıcı oturumu bulunamadı.");
            }

            // HATA BURADAYDI: int.Parse işlemi buradan da kaldırıldı.

            var likedSongs = await _context.LikedSongs
                .Where(l => l.UserId == userId) // string olarak filtreleme yapıyor
                .Include(l => l.Song)
                    .ThenInclude(s => s.Artist)
                .Select(l => new LikedSongListDto
                {
                    SongId = l.SongId,
                    Title = l.Song.Title,
                    Duration = l.Song.Duration.ToString(@"mm\:ss"),
                    ImageUrl = l.Song.CoverImageUrl,
                    ArtistName = l.Song.Artist != null ? l.Song.Artist.Name : "Bilinmeyen Sanatçı",
                    IsLiked = true,
                    CreatedAt = DateTime.Now
                })
                .ToListAsync();

            return Ok(likedSongs);
        }
    }
}