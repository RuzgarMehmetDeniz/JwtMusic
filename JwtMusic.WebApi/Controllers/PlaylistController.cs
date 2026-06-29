using JwtMusic.WebApi.Context;
using JwtMusic.WebApi.Dtos.PlaylistDtos;
using JwtMusic.WebApi.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JwtMusic.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PlaylistController : ControllerBase
    {
        private readonly JwtContext _context;

        public PlaylistController(JwtContext context)
        {
            _context = context;
        }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        [HttpGet]
        public async Task<IActionResult> GetMyPlaylists()
        {
            var userId = GetUserId();

            var playlists = await _context.Playlists
                .Where(p => p.AppUserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ResultPlaylistDto
                {
                    PlaylistId = p.PlaylistId,
                    Name = p.Name,
                    SongCount = p.PlaylistSongs.Count,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(playlists);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlaylistDetail(int id)
        {
            var userId = GetUserId();

            var playlist = await _context.Playlists
                .Where(p => p.PlaylistId == id && p.AppUserId == userId)
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                        .ThenInclude(s => s.Artist)
                .FirstOrDefaultAsync();

            if (playlist == null)
                return NotFound(new { message = "Çalma listesi bulunamadı." });

            var dto = new PlaylistDetailDto
            {
                PlaylistId = playlist.PlaylistId,
                Name = playlist.Name,
                CreatedAt = playlist.CreatedAt,
                Songs = playlist.PlaylistSongs
                    .OrderByDescending(ps => ps.AddedAt)
                    .Select(ps => new PlaylistSongDto
                    {
                        PlaylistSongId = ps.PlaylistSongId,
                        SongId = ps.SongId,
                        Title = ps.Song.Title,
                        ArtistName = ps.Song.Artist != null ? ps.Song.Artist.Name : "Bilinmiyor",
                        CoverImageUrl = ps.Song.CoverImageUrl,
                        AudioUrl = ps.Song.AudioUrl,
                        RequiredRole = !string.IsNullOrEmpty(ps.Song.RequiredRole) ? ps.Song.RequiredRole : "Basic",
                        Duration = ps.Song.Duration.ToString(@"mm\:ss"),
                        AddedAt = ps.AddedAt
                    }).ToList()
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlaylist([FromBody] CreatePlaylistDto dto)
        {
            var userId = GetUserId();

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Liste adı boş olamaz." });

            var playlist = new Playlist
            {
                Name = dto.Name.Trim(),
                AppUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Çalma listesi oluşturuldu.", playlistId = playlist.PlaylistId });
        }


        [HttpPost("{id}/songs")]
        public async Task<IActionResult> AddSong(int id, [FromBody] AddSongToPlaylistDto dto)
        {
            var userId = GetUserId();

            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == id && p.AppUserId == userId);

            if (playlist == null)
                return NotFound(new { message = "Çalma listesi bulunamadı." });

            // Zaten ekli mi?
            var alreadyExists = await _context.PlaylistSongs
                .AnyAsync(ps => ps.PlaylistId == id && ps.SongId == dto.SongId);

            if (alreadyExists)
                return BadRequest(new { message = "Bu şarkı zaten listede mevcut." });

            var songExists = await _context.Songs.AnyAsync(s => s.SongId == dto.SongId);
            if (!songExists)
                return NotFound(new { message = "Şarkı bulunamadı." });

            var playlistSong = new PlaylistSong
            {
                PlaylistId = id,
                SongId = dto.SongId,
                AddedAt = DateTime.UtcNow
            };

            _context.PlaylistSongs.Add(playlistSong);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Şarkı listeye eklendi." });
        }


        [HttpDelete("{id}/songs/{playlistSongId}")]
        public async Task<IActionResult> RemoveSong(int id, int playlistSongId)
        {
            var userId = GetUserId();

            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == id && p.AppUserId == userId);

            if (playlist == null)
                return NotFound(new { message = "Çalma listesi bulunamadı." });

            var playlistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistSongId == playlistSongId && ps.PlaylistId == id);

            if (playlistSong == null)
                return NotFound(new { message = "Şarkı bulunamadı." });

            _context.PlaylistSongs.Remove(playlistSong);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Şarkı listeden çıkarıldı." });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlaylist(int id)
        {
            var userId = GetUserId();

            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == id && p.AppUserId == userId);

            if (playlist == null)
                return NotFound(new { message = "Çalma listesi bulunamadı." });

            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Çalma listesi silindi." });
        }
    }
}