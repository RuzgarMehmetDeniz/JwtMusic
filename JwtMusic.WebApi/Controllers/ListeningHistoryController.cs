using JwtMusic.WebApi.Context;
using JwtMusic.WebApi.Dtos.ListeningHistoryDtos;
using JwtMusic.WebApi.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using System.Security.Claims;

namespace JwtMusic.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ListeningHistoryController : ControllerBase
    {
        private readonly JwtContext _context;

        public ListeningHistoryController(JwtContext context)
        {
            _context = context;
        }

        // ── Giriş yapan kullanıcının ID'sini token'dan al ──────────────────
        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        [HttpPost]
        public async Task<IActionResult> AddToHistory([FromBody] CreateListeningHistoryDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Şarkı gerçekten var mı?
            var songExists = await _context.Songs.AnyAsync(s => s.SongId == dto.SongId);
            if (!songExists)
                return NotFound(new { message = "Şarkı bulunamadı." });

            var history = new ListeningHistory
            {
                AppUserId = userId,
                SongId = dto.SongId,
                ListenedAt = DateTime.UtcNow
            };

            _context.ListeningHistories.Add(history);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Dinleme geçmişe eklendi.", historyId = history.Id });
        }

        [HttpGet]
        public async Task<IActionResult> GetMyHistory()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var history = await _context.ListeningHistories
                .Where(h => h.AppUserId == userId)
                .OrderByDescending(h => h.ListenedAt)
                .Take(50)
                .Include(h => h.Song)
                    .ThenInclude(s => s.Artist)   // Artist navigation property varsa
                .Select(h => new ResultListeningHistoryDto
                {
                    Id = h.Id,
                    SongId = h.SongId,
                    SongTitle = h.Song.Title,
                    ArtistName = h.Song.Artist != null ? h.Song.Artist.Name : "Bilinmiyor",
                    CoverImageUrl = h.Song.CoverImageUrl,
                    AudioUrl = h.Song.AudioUrl,
                    RequiredRoleName = h.Song.RequiredRole != null ? h.Song.RequiredRole : "Basic",
                    ListenedAt = h.ListenedAt
                })
                .ToListAsync();

            return Ok(history);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHistory(int id)
        {
            var userId = GetUserId();
            var record = await _context.ListeningHistories
                .FirstOrDefaultAsync(h => h.Id == id && h.AppUserId == userId);

            if (record == null)
                return NotFound(new { message = "Kayıt bulunamadı veya bu kayıt size ait değil." });

            _context.ListeningHistories.Remove(record);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kayıt silindi." });
        }

        // ══════════════════════════════════════════════════════════════════
        // DELETE api/listeninghistory/clear
        // Kullanıcının tüm geçmişini temizler.
        // ══════════════════════════════════════════════════════════════════
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearHistory()
        {
            var userId = GetUserId();
            var records = _context.ListeningHistories
                .Where(h => h.AppUserId == userId);

            _context.ListeningHistories.RemoveRange(records);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tüm geçmiş temizlendi." });
        }
    }
}