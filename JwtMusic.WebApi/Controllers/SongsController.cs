using JwtMusic.WebApi.Context;
using JwtMusic.WebApi.Dtos.SongDtos;
using JwtMusic.WebApi.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// using JwtMusic.WebApi.DAL; // Kendi AppDbContext sınıfının olduğu namespace'i buraya eklemelisin

namespace JwtMusic.WebApi.Controllers
{
    [Authorize] // Bu Controller'a erişmek için geçerli bir JWT gerekir
    [ApiController]
    [Route("api/[controller]")]
    public class SongsController : ControllerBase
    {
        private readonly JwtContext _context;
        private readonly UserManager<AppUser> _userManager;

        public SongsController(JwtContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Rol adını hiyerarşik seviyeye çevirir
        private static int RoleToLevel(string role) => role.ToUpper() switch
        {
            "ELIT" => 3,
            "PREMIUM" => 2,
            "GOLD" => 1,
            "BASIC" => 0,
            _ => -1
        };

        // Kullanıcının en yüksek paket seviyesini döner
        private async Task<int> GetUserLevelAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any()) return -1;
            return roles.Select(RoleToLevel).Max();
        }

        // 1. Tüm Şarkıları Listele
        // Roles tablosuyla JOIN yaparak RequiredRoleName'i çekiyoruz
        [HttpGet]
        public async Task<IActionResult> SongList()
        {
            var values = await _context.Songs
                .Include(x => x.Artist)
                .Join(_context.Roles,
                    song => song.RequiredRole,
                    role => role.Id,
                    (song, role) => new ResultSongDto
                    {
                        SongId = song.SongId,
                        Title = song.Title,
                        CoverImageUrl = song.CoverImageUrl,
                        AudioUrl = song.AudioUrl,
                        Duration = song.Duration,
                        PlayCount = song.PlayCount,
                        ReleaseDate = song.ReleaseDate,
                        ArtistId = song.ArtistId,
                        ArtistName = song.Artist != null ? song.Artist.Name : "Bilinmeyen Sanatçı",
                        RequiredRoleId = song.RequiredRole,
                        RequiredRoleName = role.Name
                    })
                .ToListAsync();

            return Ok(values);
        }

        // 2. ID'ye Göre Şarkı Getir — paket kontrolü VAR
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSongById(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var song = await _context.Songs
                .Include(x => x.Artist)
                .Join(_context.Roles,
                    s => s.RequiredRole,
                    r => r.Id,
                    (s, r) => new { s, RoleName = r.Name })
                .FirstOrDefaultAsync(x => x.s.SongId == id);

            if (song == null)
                return NotFound("Aradığınız şarkı bulunamadı.");

            var userLevel = await GetUserLevelAsync(user);
            var requiredLevel = RoleToLevel(song.RoleName ?? "BASIC");

            // Kullanıcının seviyesi şarkının gerektirdiği seviyeden düşükse engelle
            if (userLevel < requiredLevel)
            {
                return StatusCode(403, new
                {
                    message = $"Bu şarkıyı dinlemek için {song.RoleName} paketi gereklidir. Lütfen paketinizi yükseltin.",
                    required = song.RoleName
                });
            }

            // Erişim tamam — play count artır
            song.s.PlayCount++;
            await _context.SaveChangesAsync();

            return Ok(new ResultSongDto
            {
                SongId = song.s.SongId,
                Title = song.s.Title,
                CoverImageUrl = song.s.CoverImageUrl,
                AudioUrl = song.s.AudioUrl,
                Duration = song.s.Duration,
                PlayCount = song.s.PlayCount,
                ReleaseDate = song.s.ReleaseDate,
                ArtistId = song.s.ArtistId,
                ArtistName = song.s.Artist != null ? song.s.Artist.Name : "Bilinmeyen Sanatçı",
                RequiredRoleId = song.s.RequiredRole,
                RequiredRoleName = song.RoleName
            });
        }

        // 3. Yeni Şarkı Ekle
        [HttpPost]
        public async Task<IActionResult> CreateSong(CreateSongDto createSongDto)
        {
            var song = new Song
            {
                Title = createSongDto.Title,
                CoverImageUrl = createSongDto.CoverImageUrl,
                AudioUrl = createSongDto.AudioUrl,
                Duration = TimeSpan.Parse(createSongDto.Duration),
                PlayCount = 0,
                ReleaseDate = createSongDto.ReleaseDate,
                ArtistId = createSongDto.ArtistId,
                RequiredRole = createSongDto.RequiredRoleId
            };

            await _context.Songs.AddAsync(song);
            await _context.SaveChangesAsync();

            return Ok("Şarkı başarıyla listeye eklendi.");
        }

        // 4. Şarkı Güncelle
        [HttpPut]
        public async Task<IActionResult> UpdateSong(UpdateSongDto updateSongDto)
        {
            var song = await _context.Songs.FindAsync(updateSongDto.SongId);
            if (song == null)
                return NotFound("Güncellenmek istenen şarkı bulunamadı.");

            song.Title = updateSongDto.Title;
            song.CoverImageUrl = updateSongDto.CoverImageUrl;
            song.AudioUrl = updateSongDto.AudioUrl;
            song.Duration = TimeSpan.Parse(updateSongDto.Duration);
            song.ReleaseDate = updateSongDto.ReleaseDate;
            song.ArtistId = updateSongDto.ArtistId;
            song.RequiredRole = updateSongDto.RequiredRoleId;

            _context.Songs.Update(song);
            await _context.SaveChangesAsync();

            return Ok("Şarkı bilgileri başarıyla güncellendi.");
        }

        // 5. Şarkı Sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSong(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
                return NotFound("Silinmek istenen şarkı bulunamadı.");

            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();

            return Ok("Şarkı başarıyla sistemden silindi.");
        }
    }
}