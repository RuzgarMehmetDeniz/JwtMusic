using JwtMusic.WebApi.Context;
using JwtMusic.WebApi.Dtos.SongDtos;
using JwtMusic.WebApi.Entities;
using Microsoft.AspNetCore.Authorization;
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
        private readonly JwtContext _context; // Veritabanı bağlantınız

        public SongsController(JwtContext context)
        {
            _context = context;
        }

        // 1. Tüm Şarkıları Listeleme (Sanatçı Adı Dahil)
        [HttpGet]
        public async Task<IActionResult> SongList()
        {
            var values = await _context.Songs
                .Include(x => x.Artist) // İlişkili tablodan Artist bilgisini yükle
                .Select(s => new ResultSongDto
                {
                    SongId = s.SongId,
                    Title = s.Title,
                    CoverImageUrl = s.CoverImageUrl,
                    AudioUrl = s.AudioUrl,
                    Duration = s.Duration,
                    PlayCount = s.PlayCount,
                    IsPremium = s.IsPremium,
                    ReleaseDate = s.ReleaseDate,
                    ArtistId = s.ArtistId,
                    ArtistName = s.Artist != null ? s.Artist.Name : "Bilinmeyen Sanatçı"
                })
                .ToListAsync();

            return Ok(values);
        }

        // 2. ID'ye Göre Şarkı Getirme
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSongById(int id)
        {
            var song = await _context.Songs
                .Include(x => x.Artist)
                .FirstOrDefaultAsync(x => x.SongId == id);

            if (song == null)
            {
                return NotFound("Aradığınız şarkı veritabanında bulunamadı.");
            }

            var result = new ResultSongDto
            {
                SongId = song.SongId,
                Title = song.Title,
                CoverImageUrl = song.CoverImageUrl,
                AudioUrl = song.AudioUrl,
                Duration = song.Duration,
                PlayCount = song.PlayCount,
                IsPremium = song.IsPremium,
                ReleaseDate = song.ReleaseDate,
                ArtistId = song.ArtistId,
                ArtistName = song.Artist != null ? song.Artist.Name : "Bilinmeyen Sanatçı"
            };

            return Ok(result);
        }

        // 3. Yeni Şarkı Ekleme
        [HttpPost]
        public async Task<IActionResult> CreateSong(CreateSongDto createSongDto)
        {
            var song = new Song
            {
                Title = createSongDto.Title,
                CoverImageUrl = createSongDto.CoverImageUrl,
                AudioUrl = createSongDto.AudioUrl,
                Duration = TimeSpan.Parse(createSongDto.Duration), // "00:03:24" string ifadesini TimeSpan'e çevirir
                PlayCount = 0, // Yeni eklenen şarkı 0 dinlenmeyle başlar
                IsPremium = createSongDto.IsPremium,
                ReleaseDate = createSongDto.ReleaseDate,
                ArtistId = createSongDto.ArtistId
            };

            await _context.Songs.AddAsync(song);
            await _context.SaveChangesAsync();

            return Ok("Şarkı başarıyla listeye eklendi.");
        }

        // 4. Şarkı Güncelleme
        [HttpPut]
        public async Task<IActionResult> UpdateSong(UpdateSongDto updateSongDto)
        {
            var song = await _context.Songs.FindAsync(updateSongDto.SongId);
            if (song == null)
            {
                return NotFound("Güncellenmek istenen şarkı bulunamadı.");
            }

            // Verileri DTO'dan alıp Entity'e aktarıyoruz
            song.Title = updateSongDto.Title;
            song.CoverImageUrl = updateSongDto.CoverImageUrl;
            song.AudioUrl = updateSongDto.AudioUrl;
            song.Duration = TimeSpan.Parse(updateSongDto.Duration);
            song.IsPremium = updateSongDto.IsPremium;
            song.ReleaseDate = updateSongDto.ReleaseDate;
            song.ArtistId = updateSongDto.ArtistId;

            _context.Songs.Update(song);
            await _context.SaveChangesAsync();

            return Ok("Şarkı bilgileri başarıyla güncellendi.");
        }

        // 5. Şarkı Silme
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSong(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
            {
                return NotFound("Silinmek istenen şarkı bulunamadı.");
            }

            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();

            return Ok("Şarkı başarıyla sistemden silindi.");
        }
    }
}